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
    public Ending_Start ending_Start;

    [Header("Image Connects")]
    [SerializeField] Image backGround;
    [SerializeField] Image lineImage;
    [SerializeField] RectTransform upDark;
    [SerializeField] RectTransform downDark;

    [Header("Text Connect")]
    [SerializeField] TMP_Text name;
    [SerializeField] TMP_Text Impact_Name;
    [SerializeField] TMP_Text content_Text;
    [SerializeField] TMP_Text impact_Content_Text;

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


    [Header("Ending Part 3 Settings")]
    [SerializeField] private Image image_UpEye;
    [SerializeField] private Image image_DownEye;
    [SerializeField] private Image image_UpDarkBackGround;

    [SerializeField] private Image image_DukdoLogo;

    [SerializeField] private List<ParticleSystem> particle_Flowers;

    [Header("조정 구역")]
    [SerializeField] private float backGroundFadeOutTime = 2.3f;
    [SerializeField] private float cloudUpTime = 2f;
    [SerializeField] private float textPosY = -500f;
    [Header("그외")]

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

    private List<EndingAction> ending_Normal_Sequence = new List<EndingAction>();
    private List<EndingAction> ending_Hidden_Sequence = new List<EndingAction>();



    [Header("Normal Ending Part Settings")]
    // =============== [추가된 부분] 수도승 애니메이션 제어용 변수 ===============
    private float currentSeatAnimSpeed = 1f;
    private bool isSeatAnimPlaying = false;

    [SerializeField] private Image image_backGroundBright;
    [SerializeField] private CanvasGroup canvasGroup_NormalEnding;
    [SerializeField] private Image image_sudoSeat;
    [SerializeField] private List<Sprite> sprites_sudoSeat;
    [SerializeField] private Image image_BackGlow2;
    [SerializeField] private Image image_HighLightLogo;
    [SerializeField] private Image image_BelowPetal;
    [SerializeField] private Image image_BackGlow;
    [SerializeField] private Image image_BelowPetal2;
    [SerializeField] private Image image_Flower;
    [SerializeField] public Image image_AllBlackPanel;

    [SerializeField] private CanvasGroup canvasGroup_Sun;


    // ▼▼▼ 2. "준비 완료" 신호를 보내는 코루틴 추가 ▼▼▼
    private IEnumerator NotifyManagerWhenReady()
    {
        // 씬의 모든 Start 함수가 실행되고 첫 프레임을 그릴 시간을 안전하게 확보합니다.
        yield return null;

        // SceneLoadingManager에게 "이제 문 열어도 돼!" 라고 신호를 보냅니다.
        if (SceneLoadingManager.Instance != null)
        {
            SceneLoadingManager.Instance.NotifySceneReady();
        }
    }
    private void Start()
    {
        // 씬의 모든 준비가 끝났다고 LoadingManager에게 알립니다.
        StartCoroutine(NotifyManagerWhenReady());
        StartInit().Forget();
    }
    private async UniTask StartInit()
    {
        foreach (var particle in particle_Flowers)
        {
            particle.Stop();
        }

        // LocalizationManager.LoadAll();
        SettingClearForStart();
        ClearAllTexts();
        LoadEndingSequenceData("Localization/EndingTable");
        LocalizationManager.LoadAll();

        if (SceneLoadingManager.Instance != null)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            SceneLoadingManager.Instance.NotifySceneReady();
        }

        ending_Start.ConnectWithController(this);
        // PlayEndingSequence().Forget(); // 처음부터 시작

        // PlayEndingSequence_Part2().Forget(); // 영화 액션부터 시작, 하늘로 올라가기

        //PlayEndingSequence_Part2_TextAction().Forget(); // 하늘로 올라가서 대화 시작 UpTalk

        //NormalEnding_Sequence().Forget(); // 노말엔딩시작 // NormalEnding Talk




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
        ending_Normal_Sequence.Clear();
        ending_Hidden_Sequence.Clear();

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

                string id = row[idIdx].Trim();

                int caseType = 0;

              
                if(id.StartsWith("Ending_Talk_"))
                {
                    caseType = 1;
                }
                else if(id.StartsWith("Ending_UpTalk_"))
                {
                    caseType = 2;
                }
                else if(id.StartsWith("Ending_Normal_"))
                {
                    caseType = 3;
                }
                else if(id.StartsWith("Ending_Hidden_"))
                {
                    caseType = 4;
                }


             
                EndingAction action = new EndingAction();
                action.id = id;

                // 인덱스 파싱
                string indexPart= null;

                switch(caseType)
                {
                    case 1:
                        indexPart = id.Replace("Ending_Talk_", "").Trim();
                        break;
                    case 2:
                        indexPart = id.Replace("Ending_UpTalk_", "").Trim();
                        break;
                    case 3:
                        indexPart = id.Replace("Ending_Normal_", "").Trim();
                        break;

                    case 4:
                        indexPart = id.Replace("Ending_Hidden_", "").Trim();
                        break;
                }



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

              

                switch (caseType)
                {
                    case 1:
                        endingSequence.Add(action);
                        break;
                    case 2:
                        ending_Up_Sequence.Add(action);
                        break;
                    case 3:
                        ending_Normal_Sequence.Add(action);
                        break;

                    case 4:
                        ending_Hidden_Sequence.Add(action);
                        break;
                }

            }
        }

     
    }

    public async UniTaskVoid PlayEndingSequence()
    {
        Managers.Sound.Play("BGM/EndingTheme1", Define.Sound.BGM,1,1,false);

        foreach (var action in endingSequence)
        {

            SpecialAction(action.index, action);

            string localizedName = string.IsNullOrEmpty(action.speakerKey) ? "" : LocalizationManager.Get(action.speakerKey);
            string localizedContent = LocalizationManager.Get(action.id);



            if (localizedContent == "X" || localizedContent == "~") localizedContent = "";

            // [수정된 부분] SetActive(false)를 완전히 제거하고, 대상 UI 포인터만 지정합니다.
            TMP_Text activeName;
            TMP_Text activeContent;
            TMP_Text inactiveName;
            TMP_Text inactiveContent;

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

                    if (content_Text.color.a > 0)
                    {
                        if (action.index == 33)
                        {
                            fadeTasks.Add(content_Text.DOFade(0f, 3f).SetEase(action.easeType).ToUniTask());
                        }

                        else
                        {
                            fadeTasks.Add(content_Text.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());
                        }
                            
                    }
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
                if (action.index != 33)
                { content_Text.text = ""; }
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

        // [수정] 마지막에 남아있는 텍스트들을 부드럽게 지워주는 연출 추가
        var finalFadeTasks = new List<UniTask>();
        float fadeTime = 1.0f; // 페이드 아웃 시간 설정

        if (name.color.a > 0) finalFadeTasks.Add(name.DOFade(0f, fadeTime).ToUniTask());
        if (Impact_Name.color.a > 0) finalFadeTasks.Add(Impact_Name.DOFade(0f, fadeTime).ToUniTask());
        if (impact_Content_Text.color.a > 0) finalFadeTasks.Add(impact_Content_Text.DOFade(0f, fadeTime).ToUniTask());


        name.text = "";
        Impact_Name.text = "";
       
        impact_Content_Text.text = "";
        wasImpactOn = false;

        
        name.DOFade(1f, 0);


        PlayEndingSequence_Part2().Forget();
    }

    private async UniTask PlayEndingSequence_Part2()
    {
        Debug.Log("엔딩 시퀀스 2 시작!");
        content_Text.GetComponent<RectTransform>().DOAnchorPosY(textPosY, 0);

       
        // 영화 위 아래에서 검은색 내려오는 액션
        var tasks = new List<UniTask>();

        tasks.Add(upDark.DOSizeDelta(new Vector2(upDark.sizeDelta.x, 400f), 1.5f)
                        .SetEase(Ease.OutQuad) // 부드러운 효과 추가
                        .ToUniTask());

        // (기존 코드의 upDark 오타를 downDark로 수정했습니다)
        tasks.Add(downDark.DOSizeDelta(new Vector2(downDark.sizeDelta.x, 400f), 1.5f)
                     .SetEase(Ease.OutQuad) // 부드러운 효과 추가
                     .ToUniTask());

        tasks.Add(backGround.DOFade(0, backGroundFadeOutTime)
            .SetEase(Ease.OutQuad)
            .ToUniTask());

        tasks.Add(lineImage.DOFade(0, backGroundFadeOutTime).ToUniTask());

        await UniTask.WhenAll(tasks);
        tasks.Clear();

        // 끝

        // === 상승하기 로직 시작 ===
        if (scrollTarget != null)
        {
            Managers.Sound.Play("SFX/Ending/Cloud"); // 바람올라가는소리
            // 시작 PosY 4747 설정
            scrollTarget.anchoredPosition = new Vector2(scrollTarget.anchoredPosition.x, startPosY);

            // DOTween Sequence를 이용해 각 구간별로 이동 (빠르게 가다가 목적지 부근에서 느려짐)
            // Ease.InOutQuad 속성은 시작할때 느림 -> 중간 빠름 -> 끝날때 느림 을 적용하여 주석의 움직임을 완벽히 모방합니다.
            Sequence scrollSequence = DOTween.Sequence();

            // 1구간
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY1, duration1).SetEase(scrollEase));
            scrollSequence.AppendCallback(() => Managers.Sound.Play("SFX/Ending/CreditName"));

            // 2구간
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY2, duration2).SetEase(scrollEase));
            scrollSequence.AppendCallback(() => Managers.Sound.Play("SFX/Ending/CreditName"));

            // 3구간
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY3, duration3).SetEase(scrollEase));
            scrollSequence.AppendCallback(() => Managers.Sound.Play("SFX/Ending/CreditName"));

            // 4구간
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY4, duration4).SetEase(scrollEase));
            scrollSequence.AppendCallback(() => Managers.Sound.Play("SFX/Ending/CreditName"));

            // 5구간 및 엔딩 (사운드 없음)
            scrollSequence.Append(scrollTarget.DOAnchorPosY(targetPosY5, duration5).SetEase(scrollEase));
            scrollSequence.Append(scrollTarget.DOAnchorPosY(endPosY, durationEnd).SetEase(scrollEase));

            // 암전 해제 (Join은 이전 Append와 동시에 실행됨)
            scrollSequence.Join(upDark.DOSizeDelta(new Vector2(upDark.sizeDelta.x, 0), 1.7f).SetEase(Ease.OutQuad));
            scrollSequence.Join(downDark.DOSizeDelta(new Vector2(downDark.sizeDelta.x, 0), 1.7f).SetEase(Ease.OutQuad));


            // 시퀀스가 끝날 때까지 대기
            await scrollSequence.ToUniTask();
        }
        else
        {
            Debug.LogWarning("인스펙터 창에서 Scroll Target이 비어있어 상승 연출을 재생할 수 없습니다.");
        }

        Debug.Log("끝까지 도착했습니다!");
  
        // 영화 효과 다시 사라지기
        var tasks2 = new List<UniTask>();


        // === 구름 올라오기 ===
        // cloudObject가 GameObject로 선언되어 있으므로, 위치와 투명도를 제어할 컴포넌트를 가져옵니다.
        RectTransform cloudRect = cloudObject.GetComponent<RectTransform>();
        Image cloudImage = cloudObject.GetComponent<Image>();

        if (cloudRect != null && cloudImage != null)
        {
            // 1. PosY -1200으로 살며시 올라오기 (시간은 2.5초로 임의 설정)
            tasks2.Add(cloudRect.DOAnchorPosY(-1100f, 2f)
                .SetEase(Ease.OutQuad) // 도착할 때 부드럽게 감속
                .ToUniTask());

        
        }
        else
        {
            Debug.LogWarning("cloudObject에 RectTransform 또는 Image 컴포넌트가 없습니다.");
        }

        name.text = "";
        SetAlpha(name, 1f);
        content_Text.text = "";
        SetAlpha(content_Text, 1f);


        // 영화 레터박스가 사라지는 효과와 구름이 올라오는 효과를 동시에 실행하고 대기
        await UniTask.WhenAll(tasks2);

        Debug.Log("대화 연출 시작!");

        

        // 대화시작! 
        Debug.Log(ending_Up_Sequence.Count);
        PlayEndingSequence_Part2_TextAction().Forget();

    }

    private async UniTask PlayEndingSequence_Part2_TextAction()
    {
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


        Debug.Log("Part2 모든 액션 종료");

        NormalEnding_Sequence().Forget();
        /*
        if (Bool_CheckHiddenEndingEnter())
        {
            HiddenEnding_Sequence().Forget();
            return;
        }
        else
        {
            NormalEnding_Sequence().Forget();
            return;
        }
        */
    }


    private async UniTask NormalEnding_Sequence()
    {
        // 대화시작! 
        Managers.Sound.Play("BGM/Exhaust", Define.Sound.BGM,1,1,false);
        Debug.Log("노말엔딩 시퀀스 시작");
        foreach (var action in ending_Normal_Sequence)
        {

            SpecialAction_NormalEnding(action.index, action);

            string localizedName = string.IsNullOrEmpty(action.speakerKey) ? "" : LocalizationManager.Get(action.speakerKey);
            string localizedContent = LocalizationManager.Get(action.id);

            if (localizedContent == "X" || localizedContent == "~") localizedContent = "";
           

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

    private async UniTask HiddenEnding_Sequence()
    {
        // 대화시작! 
        foreach (var action in ending_Hidden_Sequence)
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

    private void SpecialAction_NormalEnding(int index, EndingAction action)
    {
        Debug.Log($"{index} 노말 엔딩 액션 시작");
        switch (index)
        {
            case 16:
                // 음악 재생 (프로젝트 내 SoundManager 등의 호출부 필요)
                Managers.Sound.Play("BGM/EndingTheme2",Define.Sound.SFX,1,1,false);

                content_Text.alignment = TextAlignmentOptions.Midline;
                // 수도승 애니메이션 시작 (기본 속도 1f부터)
                SeatAnimation(0.5f).Forget();
                // 화면 페이드인 Linear로
                // action.conversion 시간(데이터에 정의된 시간)만큼 캔버스 그룹 알파를 1로 올림
                float fadeTime = action.conversion > 0f ? action.conversion : 2f;
                canvasGroup_NormalEnding.DOFade(1f, fadeTime*1f).SetEase(Ease.Linear);
                canvasGroup_Sun.DOFade(1f, fadeTime * 1.5f).SetEase(Ease.Linear );

                break;

            case 17:
                image_backGroundBright.DOFade(1f, 14f).SetEase(Ease.Linear);
                
               
                break;

            case 19:    
                // 연꽃 애니메이션 시작
                // 26번 케이스에서 이어지기 위해 기초값 세팅 및 등장 처리
                image_Flower.gameObject.SetActive(true);
                image_Flower.rectTransform.anchoredPosition = new Vector2(1400f, 1400f);
                image_Flower.rectTransform.localRotation = Quaternion.Euler(0, 0, -38f);
                image_Flower.DOFade(1f, 1.5f);
                particle_Flowers[0].Play();
                particle_Flowers[1].Play();

                break;

            case 20:
                // 텍스트 위치 및 여러가지 조정
                // 예: 텍스트의 부모나 자신의 앵커 위치를 DOTween으로 부드럽게 이동
                // content_Text.rectTransform.DOAnchorPosY(-200f, 2f).SetEase(Ease.OutQuad);
                

                break;

            case 21:
                // Petal2와 BackGlow alpha값 1로 천천히 올리기
                float duration21 = action.conversion > 0f ? action.conversion : 3f;

                image_BelowPetal2.DOFade(1f, duration21);
                image_BackGlow.DOFade(1f, duration21).OnComplete(() =>
                {
                    image_BackGlow2.DOFade(1f, 2f);
                    // 완전히 끝나면 BackGlow의 Rotation Z값 돌아가기 (계속 무한반복)
                    image_BackGlow.rectTransform.DORotate(new Vector3(0, 0, 360f),70f, RotateMode.FastBeyond360)
                        .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Restart); // -1은 무한 반복
                });
                SeatAnimation(1f).Forget();
                
             
                break;

            case 22:
                particle_Flowers[2].Play();

                break;
            case 23:
                particle_Flowers[3].Play();
                particle_Flowers[0].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                particle_Flowers[1].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                break;
            case 26:
                // 26번 케이스는 복잡한 대기 시간과 순차적 애니메이션이 필요하므로 별도의 비동기 함수로 분리하여 실행
                SeatAnimation(1.5f).Forget();
                PlayCase26Sequence().Forget();
                break;

            default:
                break;
        }
    }

    // =============== [추가된 부분] Case 26번 전용 연출 시퀀스 ===============
    private async UniTaskVoid PlayCase26Sequence()
    {
        // 시작 시점: Flower가 1400, 1400 / rotate z -38 (Case 19에서 이미 세팅됨)
        foreach(var particle in particle_Flowers)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // 9초 대기
        await UniTask.Delay(TimeSpan.FromSeconds(3f));

        // 9초 뒤 이 연꽃(Flower)이 움직여서 Pos X, Pos Y에 위치함 (임의로 화면 중앙 0, 0으로 설정)
        // 이때 canvasGroup_Sun 의 alpha값 0으로 만들어서 안보이게
        image_Flower.rectTransform.DORotate(new Vector3(0, 0, -35f), 0.6f).SetEase(Ease.Linear);

        canvasGroup_Sun.DOFade(0, 4.4f);

        image_Flower.rectTransform.DOAnchorPos(Vector2.zero, 0.6f).SetEase(Ease.Linear).OnComplete(
        ()=>
        {
            image_sudoSeat.color = new Color(0, 0, 0, 0);
            // 그리고 다시 -1400, -1400 으로 사라짐, 이때까지 rotateZ는 -12까지 변화함
            image_Flower.rectTransform.DOAnchorPos(new Vector2(-1400f, -1400f), 0.6f).SetEase(Ease.Linear);
            image_Flower.rectTransform.DORotate(new Vector3(0, 0, -50f), 0.6f).SetEase(Ease.Linear);

        }
        );


       

        // 사라지는데 걸리는 3초 + 추가 6초 대기 = 총 9초 대기
        // (만약 애니메이션 시작 직후부터 6초 대기라면 6초만 적절히 조절하세요)

        await UniTask.Delay(TimeSpan.FromSeconds(4.4f));
        Managers.Sound.Play("SFX/Ending/Deukdo");
        await UniTask.Delay(TimeSpan.FromSeconds(2f));

        // HighLightLogo alpha값 1로 바꾸기 
        
        image_HighLightLogo.DOFade(1f, 0f);

        // 잠시 대기 (예: 2초)
        await UniTask.Delay(TimeSpan.FromSeconds(7f));

        // AllBlackPanel alpha값 1로 바꾸기
        image_AllBlackPanel.DOFade(1f, 0f);

        CheckFirstClearSteamAchievement();

        await UniTask.Delay(TimeSpan.FromSeconds(7f));
        SceneLoadingManager.Instance.LoadScene("MainTitle");
        // 씬이동
    }
    // =========================================================================

    private async UniTask SeatAnimation(float speed)
    {
        // 1. 타 함수에서 이 함수를 다른 스피드로 호출하면 속도 변수만 업데이트되게 처리
        currentSeatAnimSpeed = speed;

        // 2. 만약 이미 애니메이션이 루프 중이라면 새로 실행하지 않고 종료 (속도만 바뀐 채로 기존 루프가 돌아감)
        if (isSeatAnimPlaying) return;

        if (sprites_sudoSeat == null || sprites_sudoSeat.Count == 0 || image_sudoSeat == null)
        {
            Debug.LogWarning("수도승 이미지가 세팅되지 않았습니다.");
            return;
        }

        isSeatAnimPlaying = true;
        int spriteIndex = 0;

        // 3. 계속 수도승의 애니메이션 진행 (리스트 내의 이미지 순환)
        while (isSeatAnimPlaying)
        {
            image_sudoSeat.sprite = sprites_sudoSeat[spriteIndex];
            spriteIndex = (spriteIndex + 1) % sprites_sudoSeat.Count; // 끝에 도달하면 0으로 롤백

            // 4. 스피드가 높아질수록 딜레이 시간이 짧아져서 애니메이션이 빨라짐 (1f일 때 프레임당 0.1초 기준 예시)
            float delayTime = 0.1f / currentSeatAnimSpeed;
            await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
        }
    }
    private void SpecialAction(int index, EndingAction action)
    {

        if (index == 0)

        {

            // 1. 텍스트들은 빠르게 페이드아웃 (0.3초)

            ending_Start.textUp.DOFade(0f, 0.3f);

            ending_Start.textDown.DOFade(0f, 0.3f);





            // 2. image_stamina의 Sprite를 1.5초동안 List 0부터 끝까지 변환하도록

            int spriteCount = ending_Start.stamina_effects.Count;

            if (spriteCount > 0)

            {

                // DOVirtual을 사용해 0부터 마지막 인덱스까지 1.5초 동안 선형(Linear)으로 변화

                DOVirtual.Float(0f, spriteCount - 0.01f, 2.5f, (v) =>

                {
                    int currentFrame = Mathf.FloorToInt(v);

                    ending_Start.image_stamina.sprite = ending_Start.stamina_effects[currentFrame];

                }).SetEase(Ease.Linear).OnComplete(()=> { ending_Start.image_stamina.DOFade(0, 0.4f); });

            }



            // 3. 이러면서 blackPanel 값도 밝아지게끔 설정 (알파값을 0으로 만들어서 암전 해제)
            ending_Start.blackPanelBack.DOFade(0f, 3f);

        }

        if (index == 28)
        {
            action.speakerKey = "   ";
        }
    }

    private async UniTask SpecialAction_Up(int index, EndingAction action)
    {
        if(index ==0)
        {
            // 2. 구름 오브젝트 Alpha(투명도) 1로 변환하면서 나타나기
            cloudObject.GetComponent<Image>().DOFade(1f, cloudUpTime)
                 .SetEase(Ease.OutQuad);
            content_Text.GetComponent<RectTransform>().DOAnchorPosY(-470, 0);
        }

        if (index == 7)
        {
            // [구름 내려감] 
            // 이전에 -1200까지 올라왔던 구름을 다시 화면 아래로 내립니다.
            RectTransform cloudRect = cloudObject.GetComponent<RectTransform>();
            if (cloudRect != null)
            {
                // action.conversion 시간을 활용하거나, 적절한 고정 시간(예: 2.5s)을 사용합니다.
                image_UpDarkBackGround.DOFade(1, 2f).SetEase(Ease.OutQuad);
                cloudRect.DOAnchorPosY(-2500f, 2f).SetEase(Ease.InQuad);
                cloudObject.GetComponent<Image>()?.DOFade(0f, 2f);
            }
        }
        else if (index == 8)
        {
            if (image_UpDarkBackGround != null)
            {
                // 1. Pivot이 정중앙(0.5, 0.5)인지 확인하세요.
                // 2. Scale을 1.3f보다 조금 더 크게 잡아 여백을 방지합니다.
                var rt = image_UpDarkBackGround.rectTransform;

                // 시퀀스를 사용하여 부드럽게 연결
                var sequence = DOTween.Sequence();

                // 동시에 실행
                sequence.Join(rt.DOScale(1.4f, 0.2f).SetEase(Ease.OutQuad));
                sequence.Join(rt.DORotate(new Vector3(0, 0, -6.7f), 0.2f).SetEase(Ease.OutBack));

                await sequence.AsyncWaitForCompletion();
            }
        }
        else if (index == 9)
        {
            // [최종 연출: 확대 + 눈 감기]

            await UniTask.WaitForSeconds(0.3f);
            // 1. 16.3초 동안 화면이 점점 Linear 확대되는 애니메이션
            if (image_UpDarkBackGround != null)
            {
                image_UpDarkBackGround.rectTransform.DOScale(1.8f, 16.3f).SetEase(Ease.Linear);
            }

            // 2. 12초 대기 후 남은 4.3초 동안 눈 감는 연출 시작
            PlayEyeClosingSequence(12f, 4.3f).Forget();
        }
    }

   
    private async UniTaskVoid PlayEyeClosingSequence(float delay, float duration)
    {
        // 12초 대기
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        image_UpEye.gameObject.SetActive(true);
        image_DownEye.gameObject.SetActive(true);
        image_UpEye.DOFade(1, 0);
        image_DownEye.DOFade(1, 0);


        // 눈꺼풀 RectTransform 가져오기
        RectTransform upEye = image_UpEye.rectTransform;
        RectTransform downEye = image_DownEye.rectTransform;

        // 눈 감기 시퀀스 (총 4.3초 구성)
        Sequence eyeSeq = DOTween.Sequence();

        // 1. 살짝 700 / -700 까지 감김 (깜빡임 시작)
        eyeSeq.Append(upEye.DOAnchorPosY(500f, 0.5f).SetEase(Ease.OutQuad));
        eyeSeq.Join(downEye.DOAnchorPosY(-500, 0.5f).SetEase(Ease.OutQuad));

        // 2. 800 / -800 으로 살짝 다시 떠짐 (정신을 차리려는 시도)
        eyeSeq.Append(upEye.DOAnchorPosY(600f, 0.3f).SetEase(Ease.InOutQuad));
        eyeSeq.Join(downEye.DOAnchorPosY(-600f, 0.3f).SetEase(Ease.InOutQuad));

        // 3. 600 / -600 까지 더 깊게 감김
        eyeSeq.Append(upEye.DOAnchorPosY(400f, 0.6f).SetEase(Ease.OutQuad));
        eyeSeq.Join(downEye.DOAnchorPosY(-400f, 0.6f).SetEase(Ease.OutQuad));

        // 4. 잠시 떨림 효과 (재생 시간 약 0.4초)
        eyeSeq.Append(upEye.DOShakeAnchorPos(0.4f, new Vector2(0, 10f), 10, 90, false, true));
        eyeSeq.Join(downEye.DOShakeAnchorPos(0.4f, new Vector2(0, 10f), 10, 90, false, true));

        // 5. 슬며시 완전히 감김 (목표치 350 / -350)
        // 남은 시간 동안 천천히 닫힙니다.
        eyeSeq.Append(upEye.DOAnchorPosY(210f, 2.5f).SetEase(Ease.InOutSine));
        eyeSeq.Join(downEye.DOAnchorPosY(-210f, 2.5f).SetEase(Ease.InOutSine));

        // 6. 완전히 감긴 후 암전 처리 (선택 사항)
        eyeSeq.OnComplete(() => {

            Debug.Log("눈을 완전히 감았습니다.");
            // 필요 시 추가적인 엔딩 크레딧이나 페이드 아웃 처리
        });
    }



    private void SettingClearForStart()
    {
        scrollTarget.DOAnchorPosY(startPosY, 0);
 
    }

    /// <summary>
    /// Noraml 엔딩 fale, Hidden 엔딩 true
    /// </summary>
    private bool Bool_CheckHiddenEndingEnter()
    {
        // 모든 챕터의 랭크가 최상일때 
        for(int i=0;i<IngameData.TOTAL_STORY_CHAPTERS;i++)//모든 스토리 챕터에 대해
        {
            if(IngameData._bestChapterRanks[i]!=Define.Rank.Perfect) // 최고 랭크가 아니라면
            {

                Debug.Log("노말엔딩 진입");
                return false; // 노말 엔딩으로 진입
            }
        }
        Debug.Log("히든엔딩 진입");
        return true; // 모든것을 통과했다면 히든엔딩으로 진입할 수 있음.
    }

    // Steam 업적 
    private void CheckFirstClearSteamAchievement()
    {
        IngameData._isStoryCompleteClear = true;
        Managers.Steam.UnlockAchievement($"ACH_ENDING_WATCH");
    }

}