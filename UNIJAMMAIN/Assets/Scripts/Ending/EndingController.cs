using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;

// 연출 한 줄의 데이터를 담을 클래스
public class EndingAction
{
    public int index;
    public string id;
    public string speakerKey;
    public string rawSpeaker;    // [추가된 부분] "X", "~" 등을 원본 그대로 확인하기 위한 변수
    public float duration;     // 대기 시간 (변화 없는 시간)
    public float conversion;   // 전환 시간 (페이드 인/아웃 진행 시간)
    public Color nameColor;
    public Color textColor;
    public bool isMiddleHighlight;
    public Ease easeType;      // 적용될 Curve(Ease) 타입
}

public class EndingController : MonoBehaviour
{
    [Header("Image Connects")]
    [SerializeField] Image backGround;
    [SerializeField] Image lineImage;
    [SerializeField] RectTransform upDark;
    [SerializeField] RectTransform downDark;

    [Header("Text Connect")]
    [SerializeField] Text name;
    [SerializeField] Text Impact_Name;
    [SerializeField] Text content_Text;
    [SerializeField] Text impact_Content_Text;

    // =============== 새로 추가된 Part 2 조절용 변수들 ===============
    [Header("Ending Part 2 Settings")]
    [SerializeField] private RectTransform scrollTarget; // 위로 올라갈 오브젝트(예: 크레딧 텍스트 부모)
    [SerializeField] private Ease scrollEase = Ease.InOutQuad; // 빠르다가 느려짐을 반복하기 가장 좋은 곡선

    [SerializeField] private float startPosY = 4747f;

    [SerializeField] private float targetPosY1 = 3100f;
    [SerializeField] private float duration1 = 4f; // 4747 -> 3100 까지 가는 데 걸리는 시간

    [SerializeField] private float targetPosY2 = 1200f;
    [SerializeField] private float duration2 = 4f; // 3100 -> 1200 까지 가는 데 걸리는 시간

    [SerializeField] private float targetPosY3 = -600f;
    [SerializeField] private float duration3 = 4f; // 1200 -> -600 까지 가는 데 걸리는 시간

    [SerializeField] private float targetPosY4 = -2200f;
    [SerializeField] private float duration4 = 4f; // -600 -> -2200 까지 가는 데 걸리는 시간

    [SerializeField] private float targetPosY5 = -3680f;
    [SerializeField] private float duration5 = 5f; // -2200 -> -4747 까지 가는 데 걸리는 시간

    [SerializeField] private float endPosY = -4747f;
    [SerializeField] private float durationEnd = 3f; // -2200 -> -4747 까지 가는 데 걸리는 시간


    [SerializeField] private float waitingTime = 1.5f; // 잠시 대기하는 시간
    [SerializeField] private GameObject cloudObject;
    

    // ================================================================

    private bool wasImpactOn = false;

    private readonly Dictionary<string, string> speakerKeyMap = new Dictionary<string, string>()
    {
        { "근원", "Ending_Name_Nidus" },
        { "수도승", "Ending_Name_Monk" },
        { "X", "" }
    };

    private List<EndingAction> endingSequence = new List<EndingAction>();
    private List<EndingAction> ending_Up_Sequence = new List<EndingAction>();


    private void Start()
    {
        SettingClearForStart();
        ClearAllTexts();
        LoadEndingSequenceData("Localization/EndingTable");
        PlayEndingSequence().Forget();
    }

    private void ClearAllTexts()
    {
        // SetActive(false)로 끄지 않고 무조건 켜둡니다. 알파(투명도)로만 제어합니다.
        name.gameObject.SetActive(true);
        content_Text.gameObject.SetActive(true);
        Impact_Name.gameObject.SetActive(true);
        impact_Content_Text.gameObject.SetActive(true);

        name.text = "";
        content_Text.text = "";
        Impact_Name.text = "";
        impact_Content_Text.text = "";

        SetAlpha(name, 0f);
        SetAlpha(content_Text, 0f);
        SetAlpha(Impact_Name, 0f);
        SetAlpha(impact_Content_Text, 0f);
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    private void LoadEndingSequenceData(string resourcePath)
    {
        // [수정된 부분] 두 리스트 모두 초기화
        endingSequence.Clear();
        ending_Up_Sequence.Clear();

        TextAsset csvAsset = Resources.Load<TextAsset>(resourcePath);
        if (csvAsset == null) return;

        string raw = csvAsset.text.Replace("\uFEFF", "");
        using (StringReader reader = new StringReader(raw))
        {
            string headerLine = reader.ReadLine();
            var headers = SplitCsv(headerLine);

            int idIdx = headers.FindIndex(x => x.Trim().ToLower() == "id" || x.Trim().ToLower() == "key");
            int speakerIdx = headers.FindIndex(x => x.Trim().ToLower() == "speaker");
            int curveIdx = headers.FindIndex(x => x.Trim().ToLower() == "curve");
            int durationIdx = headers.FindIndex(x => x.Trim().ToLower() == "duration");
            int conversionIdx = headers.FindIndex(x => x.Trim().ToLower() == "conversion");
            int nameColorIdx = headers.FindIndex(x => x.Trim().ToLower() == "namecolor");
            int textColorIdx = headers.FindIndex(x => x.Trim().ToLower() == "textcolor");
            int highlightIdx = headers.FindIndex(x => x.Trim().ToLower() == "middlehighlight");

            if (idIdx == -1) return;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var row = SplitCsv(line);

                if (idIdx < 0 || row.Count <= idIdx || string.IsNullOrWhiteSpace(row[idIdx])) continue;

                string id = row[idIdx];

                // [수정된 부분] 어떤 연출 파트의 데이터인지 확인
                bool isPart1 = id.StartsWith("Ending_Talk_");
                bool isPart2 = id.StartsWith("Ending_Up_Talk_"); // 후반부 대사용 ID 접두사

                // 둘 다 아니라면 스킵
                if (!isPart1 && !isPart2) continue;

                EndingAction action = new EndingAction();
                action.id = id;

                // 인덱스 파싱
                string indexPart = isPart1 ? id.Replace("Ending_Talk_", "").Trim() : id.Replace("Ending_Up_Talk_", "").Trim();
                if (int.TryParse(indexPart, out int idx)) action.index = idx;

                string rawSpeaker = (speakerIdx >= 0 && speakerIdx < row.Count) ? row[speakerIdx].Trim() : "X";
                action.rawSpeaker = rawSpeaker;
                action.speakerKey = speakerKeyMap.ContainsKey(rawSpeaker) ? speakerKeyMap[rawSpeaker] : "";

                action.duration = (durationIdx >= 0 && durationIdx < row.Count) ? ParseTime(row[durationIdx]) : 0f;
                action.conversion = (conversionIdx >= 0 && conversionIdx < row.Count) ? ParseTime(row[conversionIdx]) : 0f;
                action.nameColor = (nameColorIdx >= 0 && nameColorIdx < row.Count) ? ParseColor(row[nameColorIdx]) : Color.white;
                action.textColor = (textColorIdx >= 0 && textColorIdx < row.Count) ? ParseColor(row[textColorIdx]) : Color.white;

                if (highlightIdx >= 0 && highlightIdx < row.Count)
                {
                    string rawValue = row[highlightIdx].Trim().ToUpper();
                    action.isMiddleHighlight = (rawValue == "TRUE" || rawValue == "1");
                }
                else
                {
                    action.isMiddleHighlight = false;
                }

                string curveStr = (curveIdx >= 0 && curveIdx < row.Count) ? row[curveIdx] : "";
                action.easeType = ParseEase(curveStr);

                // [수정된 부분] ID에 맞게 알맞은 리스트에 추가
                if (isPart1)
                {
                    endingSequence.Add(action);
                }
                else if (isPart2)
                {
                    ending_Up_Sequence.Add(action);
                }
            }
        }
    }

    private async UniTaskVoid PlayEndingSequence()
    {
        Managers.Sound.Play("BGM/EndingTheme1", Define.Sound.BGM);

        foreach (var action in endingSequence)
        {

            SpecialAction(action.index, action);

            string localizedName = string.IsNullOrEmpty(action.speakerKey) ? "" : LocalizationManager.Get(action.speakerKey);
            string localizedContent = LocalizationManager.Get(action.id);



            if (localizedContent == "X" || localizedContent == "~") localizedContent = "";

            // [수정된 부분] SetActive(false)를 완전히 제거하고, 대상 UI 포인터만 지정합니다.
            Text activeName;
            Text activeContent;
            Text inactiveName;
            Text inactiveContent;

            if (action.isMiddleHighlight)
            {
                activeName = Impact_Name;
                activeContent = impact_Content_Text;
                inactiveName = name;
                inactiveContent = content_Text;
            }
            else
            {
                activeName = name;
                activeContent = content_Text;
                inactiveName = Impact_Name;
                inactiveContent = impact_Content_Text;
            }

            if (!string.IsNullOrEmpty(localizedContent))
            {
                // [추가된 부분] 텍스트가 바뀔 때 이름이 계속 유지 중이었다면 깜빡이지 않게 방지
                float prevNameAlpha = activeName.color.a;
                string prevNameText = activeName.text;

                // [새로운 텍스트 출력 페이즈]
                activeName.text = localizedName;
                activeContent.text = localizedContent;

                // 혹시 모를 겹침을 방지하기 위해, 안 쓰는 UI는 알파를 0으로 맞춤
                SetAlpha(inactiveName, 0f);
                SetAlpha(inactiveContent, 0f);

                Color startNameCol = action.nameColor; startNameCol.a = 0f;

                // 이전 대사와 이름이 똑같고 이미 화면에 떠있다면 알파값을 0으로 덮어쓰지 않고 유지합니다.
                startNameCol.a = (prevNameText == localizedName && prevNameAlpha > 0f) ? prevNameAlpha : 0f;

                Color startTextCol = action.textColor; startTextCol.a = 0f;
                activeName.color = startNameCol;
                activeContent.color = startTextCol;

                if (action.conversion > 0f)
                {
                    var t1 = activeName.DOColor(action.nameColor, action.conversion).SetEase(action.easeType);
                    var t2 = activeContent.DOColor(action.textColor, action.conversion).SetEase(action.easeType);

       

                    await UniTask.WhenAll(t1.ToUniTask(), t2.ToUniTask());
                }
                else
                {
                    activeName.color = action.nameColor;
                    activeContent.color = action.textColor;
                }

              
               
            }
            else
            {
                // [수정된 부분] 화자가 "~"인 쉬어가는 구간인지 확인
                bool isResting = (action.rawSpeaker == "~");

                // [수정된 부분] 빈 줄(X, ~)이 들어와서 사라져야 할 때!
                // 현재 화면에 알파값이 있어서 눈에 보이는 "모든" 텍스트를 찾아 부드럽게 지워버립니다. (구글 시트의 True/False 값과 무관하게 완벽 작동)

                if (action.conversion > 0f)
                {
                    var fadeTasks = new List<UniTask>();

                    //keepname이 false 일때만 페이드시킵니다
                    if (!isResting&&name.color.a > 0) fadeTasks.Add(name.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());
                   
                    if (content_Text.color.a > 0) fadeTasks.Add(content_Text.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());
                    if (!isResting && Impact_Name.color.a > 0) fadeTasks.Add(Impact_Name.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());
                    if (impact_Content_Text.color.a > 0) fadeTasks.Add(impact_Content_Text.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());
                    if (wasImpactOn&&!action.isMiddleHighlight) fadeTasks.Add(backGround.DOColor(new Color(180f / 255f, 180f / 255f, 180f / 255f, 1),action.conversion).SetEase(action.easeType).ToUniTask());// 화면이 바로전 변화된적이 있다면
                    if (wasImpactOn && !action.isMiddleHighlight) fadeTasks.Add(lineImage.DOColor(new Color(180f / 255f, 180f / 255f, 180f / 255f, 1), action.conversion).SetEase(action.easeType).ToUniTask());
                    // middleHighLight가 ture라면원래대로 복구하지 않아야함.
                    // color와 관련된것은 default값을 따르고있음.

                    if (fadeTasks.Count > 0)
                    {
                        await UniTask.WhenAll(fadeTasks);
                    }
                }
                else
                {
                    SetAlpha(name, 0f); SetAlpha(content_Text, 0f);
                    SetAlpha(Impact_Name, 0f); SetAlpha(impact_Content_Text, 0f);
                }




                // 모두 투명해졌으면 텍스트 내용 비우기
                if (!isResting)
                {
                    name.text = "";
                    Impact_Name.text = "";
                }
                content_Text.text = "";
                impact_Content_Text.text = "";
                wasImpactOn = false;
            }

            if (action.isMiddleHighlight || action.index == 30)   // 별개로 isMiddleHighLight가 존재하면 화면 암전 효과
            {
                backGround.DOColor(new Color(100f / 255f, 100f / 255f, 100f / 255f, 255f / 255f), action.conversion);
                lineImage.DOColor(new Color(100f / 255f, 100f / 255f, 100f / 255f, 255f / 255f), action.conversion);
                wasImpactOn = true;
            }   

            if (action.duration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(action.duration));
            }
        }
        Debug.Log("엔딩 시퀀스 1 종료!");

        PlayEndingSequence_Part2().Forget();
    }

    private async UniTask PlayEndingSequence_Part2()
    {
        Debug.Log("엔딩 시퀀스 2 시작!");

        // 영화 위 아래에서 검은색 내려오는 액션
        var tasks = new List<UniTask>();

        tasks.Add(upDark.DOSizeDelta(new Vector2(upDark.sizeDelta.x, 400f), 1.7f)
                        .SetEase(Ease.OutQuad) // 부드러운 효과 추가
                        .ToUniTask());

        // (기존 코드의 upDark 오타를 downDark로 수정했습니다)
        tasks.Add(downDark.DOSizeDelta(new Vector2(downDark.sizeDelta.x, 400f), 1.7f)
                     .SetEase(Ease.OutQuad) // 부드러운 효과 추가
                     .ToUniTask());

        tasks.Add(backGround.DOFade(0, 2.5f)
            .SetEase(Ease.OutQuad)
            .ToUniTask());

        await UniTask.WhenAll(tasks);
        tasks.Clear();

        // 끝

        // === 상승하기 로직 시작 ===
        if (scrollTarget != null)
        {
            // 시작 PosY 4747 설정
            scrollTarget.anchoredPosition = new Vector2(scrollTarget.anchoredPosition.x, startPosY);

            // DOTween Sequence를 이용해 각 구간별로 이동 (빠르게 가다가 목적지 부근에서 느려짐)
            // Ease.InOutQuad 속성은 시작할때 느림 -> 중간 빠름 -> 끝날때 느림 을 적용하여 주석의 움직임을 완벽히 모방합니다.
            Sequence scrollSequence = DOTween.Sequence();

            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY1, duration1).SetEase(scrollEase));
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY2, duration2).SetEase(scrollEase));
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY3, duration3).SetEase(scrollEase));
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY4, duration4).SetEase(scrollEase));
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY5, duration5).SetEase(scrollEase));
            scrollSequence.Append(scrollTarget.DOAnchorPosY(endPosY, durationEnd).SetEase(scrollEase));

            // 시퀀스가 끝날 때까지 대기
            await scrollSequence.ToUniTask();
        }
        else
        {
            Debug.LogWarning("인스펙터 창에서 Scroll Target이 비어있어 상승 연출을 재생할 수 없습니다.");
        }

        Debug.Log("끝까지 도착했습니다!");
        // 잠시대기
        await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));

        // 영화 효과 다시 사라지기
        var tasks2 = new List<UniTask>();

        // [수정된 부분] tasks -> tasks2 로 변경하여 새 리스트에 담습니다.
        tasks2.Add(upDark.DOSizeDelta(new Vector2(upDark.sizeDelta.x, 0), 1.7f)
                        .SetEase(Ease.OutQuad) // 부드러운 효과 추가
                        .ToUniTask());

        tasks2.Add(downDark.DOSizeDelta(new Vector2(downDark.sizeDelta.x, 0), 1.7f)
                     .SetEase(Ease.OutQuad) // 부드러운 효과 추가
                     .ToUniTask());

        // === 구름 올라오기 ===
        // cloudObject가 GameObject로 선언되어 있으므로, 위치와 투명도를 제어할 컴포넌트를 가져옵니다.
        RectTransform cloudRect = cloudObject.GetComponent<RectTransform>();
        Image cloudImage = cloudObject.GetComponent<Image>();

        if (cloudRect != null && cloudImage != null)
        {
            // 1. PosY -1200으로 살며시 올라오기 (시간은 2.5초로 임의 설정)
            tasks2.Add(cloudRect.DOAnchorPosY(-1200f, 2.5f)
                .SetEase(Ease.OutQuad) // 도착할 때 부드럽게 감속
                .ToUniTask());

            // 2. 구름 오브젝트 Alpha(투명도) 1로 변환하면서 나타나기
            tasks2.Add(cloudImage.DOFade(1f, 2.5f)
                .SetEase(Ease.OutQuad)
                .ToUniTask());
        }
        else
        {
            Debug.LogWarning("cloudObject에 RectTransform 또는 Image 컴포넌트가 없습니다.");
        }

        // 영화 레터박스가 사라지는 효과와 구름이 올라오는 효과를 동시에 실행하고 대기
        await UniTask.WhenAll(tasks2);

        Debug.Log("모든 엔딩 연출 종료!");

        // 잠시대기
        await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));


        // 대화시작! 

        // 대화시작! 
        foreach (var action in ending_Up_Sequence)
        {
            string localizedName = string.IsNullOrEmpty(action.speakerKey) ? "" : LocalizationManager.Get(action.speakerKey);
            string localizedContent = LocalizationManager.Get(action.id);

            if (localizedContent == "X" || localizedContent == "~") localizedContent = "";
            SpecialAction_Up(action.index, action);

            if (!string.IsNullOrEmpty(localizedContent))
            {
                // [이름 깜빡임 방지 로직] 이전 대사와 화자가 같으면 이름의 알파값을 유지
                float prevNameAlpha = name.color.a;
                string prevNameText = name.text;

                name.text = localizedName;
                content_Text.text = localizedContent;

                Color startNameCol = action.nameColor;
                startNameCol.a = (prevNameText == localizedName && prevNameAlpha > 0f) ? prevNameAlpha : 0f;
                name.color = startNameCol;

                Color startTextCol = action.textColor;
                startTextCol.a = 0f;
                content_Text.color = startTextCol;

                // 페이드 인 진행
                if (action.conversion > 0f)
                {
                    var t1 = name.DOColor(action.nameColor, action.conversion).SetEase(action.easeType);
                    var t2 = content_Text.DOColor(action.textColor, action.conversion).SetEase(action.easeType);

                    await UniTask.WhenAll(t1.ToUniTask(), t2.ToUniTask());
                }
                else
                {
                    name.color = action.nameColor;
                    content_Text.color = action.textColor;
                }
            }
            else
            {
                // 빈 줄(X, ~)이 들어왔을 때 화면의 텍스트를 지우는 로직
                bool isResting = (action.rawSpeaker == "~");

                if (action.conversion > 0f)
                {
                    var fadeTasks = new List<UniTask>();

                    // 쉬는 구간(~ 즉, isResting이 true)이 아닐 때만 이름을 페이드 아웃
                    if (!isResting && name.color.a > 0)
                        fadeTasks.Add(name.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());

                    if (content_Text.color.a > 0)
                        fadeTasks.Add(content_Text.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());

                    if (fadeTasks.Count > 0)
                    {
                        await UniTask.WhenAll(fadeTasks);
                    }
                }
                else
                {
                    SetAlpha(name, 0f);
                    SetAlpha(content_Text, 0f);
                }

                // 모두 투명해졌으면 텍스트 비우기
                if (!isResting)
                {
                    name.text = "";
                }
                content_Text.text = "";
            }

            // 대사 유지(대기) 시간
            if (action.duration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(action.duration));
            }
        }



    }

    private async UniTask Last_Ending_Animation()
    {

    }
        private float ParseTime(string timeStr)
    {
        if (string.IsNullOrEmpty(timeStr)) return 0f;
        string cleanStr = timeStr.ToLower().Replace("ms", "").Trim();
        if (float.TryParse(cleanStr, out float ms)) return ms / 1000f;
        return 0f;
    }

    private Color ParseColor(string hexCode)
    {
        if (string.IsNullOrEmpty(hexCode)) return Color.white;
        if (!hexCode.StartsWith("#")) hexCode = "#" + hexCode;
        if (ColorUtility.TryParseHtmlString(hexCode, out Color color)) return color;
        return Color.white;
    }

    private Ease ParseEase(string curveStr)
    {
        if (string.IsNullOrWhiteSpace(curveStr)) return Ease.OutQuad;
        string cleanStr = curveStr.Trim().ToLower().Replace(" ", "");
        switch (cleanStr)
        {
            case "easein": return Ease.InQuad;
            case "easeout": return Ease.OutQuad;
            case "easeinout": return Ease.InOutQuad;
            case "linear": return Ease.Linear;
            default:
                if (Enum.TryParse(curveStr, true, out Ease result)) return result;
                return Ease.OutQuad;
        }
    }

    private List<string> SplitCsv(string line)
    {
        List<string> result = new List<string>();
        if (line == null) return result;
        bool inQuotes = false;
        StringBuilder cur = new StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(cur.ToString());
                cur.Clear();
            }
            else
            {
                cur.Append(c);
            }
        }   
        result.Add(cur.ToString());
        return result;
    }

    private void SpecialAction(int index,EndingAction action)
    {
        if (index == 0)
        {
            backGround.DOColor(new Color(180f / 255f, 180f / 255f, 180f / 255f, 1), 1.5f);
        }
        if(index==28)
        {
            action.speakerKey = "   ";
        }
    }

    private void SpecialAction_Up(int index,EndingAction action)
    {
        
    }

    private void SettingClearForStart()
    {
        scrollTarget.DOAnchorPosY(startPosY, 0);
        backGround.DOColor(new Color(0, 0, 0, 1), 0);
    }
}