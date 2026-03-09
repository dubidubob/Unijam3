using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.IO;
using System;

// 프롤로그/텍스트 데이터 한 줄을 담을 클래스
public class PrologueAction
{
    public string key;                 // 텍스트 키 (ID)
    public int index;
    public string id;

    // 오브젝트/배경 관련
    public bool isNextBackground;      // 다음 배경 활성화 여부
    public bool isNextObject;          // 다음 오브젝트 활성화 여부
    public Vector2 startPos;           // 오브젝트 시작 위치
    public Vector2 endPos;             // 오브젝트 목표 위치

    // 텍스트 관련
    public string speaker;             // 화자 (원시 문자열, 예: "수도승")
    public Color nameColor;            // 화자 이름 색상
    public Color textColor;            // 대사 텍스트 색상
    public float conversion;           // 텍스트 페이드인/아웃 시간

    // 공통
    public float duration;             // 연출 또는 유지(대기) 시간
    public Ease easeType;              // DOTween 커브 타입
}

public class PrologueController : MonoBehaviour
{
    [Header("UI Connects")]
    [SerializeField] private Text speakerName_Text; // 화자 이름 텍스트
    [SerializeField] private Text content_Text;     // 프롤로그 대사 텍스트
    [SerializeField] private Image dimmenel_Panel;

    [Header("Sequences (Inspector에서 할당)")]
    [SerializeField] private List<Image> backgrounds = new List<Image>();
    [SerializeField] private List<RectTransform> objects = new List<RectTransform>();
    

    // 두 개의 시퀀스를 병렬로 처리하기 위해 리스트 분리
    private List<PrologueAction> prologueSequence = new List<PrologueAction>();
    private List<PrologueAction> textSequence = new List<PrologueAction>();

    // 화자 매핑 딕셔너리
    private readonly Dictionary<string, string> speakerKeyMap = new Dictionary<string, string>()
    {
        { "수도승", "Speaker_Name_Sudo" },
        { "스승님", "Speaker_Name_Master" },
        // 필요에 따라 화자를 계속 추가하세요.
    };

    // 현재 활성화된 인덱스 추적용
    private int currentBgIndex = -1;
    private int currentObjIndex = -1;
    private Image target_Image;

    [Header("Special Action 0 Settings")]
    [SerializeField] private float action0_waitDeleayStart = 0f;
    [SerializeField] private float action0_StartPosY = -1550f;      // 시작 Y 위치
    [SerializeField] private float action0_TargetScale = 0.4f;      // 목표 스케일
    [SerializeField] private float action0_TargetPosY_Step1 = -600f;// 1단계 목표 Y 위치
    [SerializeField] private float action0_Duration_Step1 = 2f;     // 1단계 연출 시간
    [SerializeField] private float action0_TargetPosY_Step2 = 634f; // 2단계 목표 Y 위치
    [SerializeField] private float action0_Duration_Step2 = 1.5f;   // 2단계 연출 시간

    [Header("Special Action 1 Settings")]
    [SerializeField] private float action1_waitDeleayStart = 0f;
    [SerializeField] private float action1_StartPosX = 200f;        // 시작 X 위치
    [SerializeField] private float action1_TargetPosX = -200f;      // 목표 X 위치
    [SerializeField] private float action1_Duration = 3f;           // 연출 시간

    [Header("Special Action 2 Settings")]
    [SerializeField] private float action2_waitDeleayStart = 0f;
    [SerializeField] private float action2_StartScale = 1f;         // 시작 스케일
    [SerializeField] private float action2_TargetScale = 0.5f;      // 목표 스케일
    [SerializeField] private float action2_Duration = 2f;           // 연출 시간
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
        InitializeUI();
        //LocalizationManager.LoadAll();
        LoadPrologueSequenceData("Localization/PrologueTable"); // 경로에 맞게 수정

        // 두 시퀀스를 병렬(Parallel)로 실행
        if (SceneLoadingManager.Instance != null)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

            SceneLoadingManager.Instance.NotifySceneReady();
        }

        PlayPrologueSequence().Forget();
        PlayTextSequence().Forget();
    }

    private void InitializeUI()
    {
        // 텍스트 초기화
        speakerName_Text.text = "";
        content_Text.text = "";
        SetAlpha(speakerName_Text, 0f);
        SetAlpha(content_Text, 0f);
        dimmenel_Panel.DOFade(0, 0);

        // 모든 배경과 오브젝트를 투명하게 또는 비활성화 처리
        foreach (var bg in backgrounds)
        {
            SetAlpha(bg, 0f);
        }

        foreach (var obj in objects)
        {
            SetAlpha(obj.GetComponent<Image>(), 0f);
            obj.gameObject.SetActive(false);
        }
    }

    private void LoadPrologueSequenceData(string resourcePath)
    {
        prologueSequence.Clear();
        textSequence.Clear();

        TextAsset csvAsset = Resources.Load<TextAsset>(resourcePath);
        if (csvAsset == null)
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {resourcePath}");
            return;
        }

        string raw = csvAsset.text.Replace("\uFEFF", ""); // BOM 제거

        // 여기서 커스텀 CSV 파서 호출
        List<List<string>> csvData = ParseCSV(raw);
        if (csvData.Count == 0) return;

        // 첫 번째 행을 헤더로 사용
        List<string> headers = csvData[0];
        for (int i = 0; i < headers.Count; i++) headers[i] = headers[i].ToLower(); // 소문자로 통일

        int keyIdx = headers.IndexOf("key");
        int isNextBgIdx = headers.IndexOf("isnextbackground");
        int isNextObjIdx = headers.IndexOf("isnextobject");
        int startPosIdx = headers.IndexOf("startpos");
        int endPosIdx = headers.IndexOf("endpos");
        int curveIdx = headers.IndexOf("curve");
        int durationIdx = headers.IndexOf("duration");
        int conversionIdx = headers.IndexOf("conversion");
        int textColorIdx = headers.IndexOf("textcolor");
        int nameColorIdx = headers.IndexOf("namecolor");
        int speakerIdx = headers.IndexOf("speaker");

        // 1번 인덱스(두 번째 줄)부터 데이터 읽기 시작
        for (int i = 1; i < csvData.Count; i++)
        {
            var row = csvData[i];

            // 빈 줄이거나 Key가 없으면 패스
            if (row.Count == 0 || keyIdx < 0 || row.Count <= keyIdx || string.IsNullOrWhiteSpace(row[keyIdx])) continue;

            PrologueAction action = new PrologueAction();
            action.key = row[keyIdx];

            // 오브젝트 및 배경 처리 파싱
            action.isNextBackground = (isNextBgIdx >= 0 && row.Count > isNextBgIdx) && ParseBool(row[isNextBgIdx]);
            action.isNextObject = (isNextObjIdx >= 0 && row.Count > isNextObjIdx) && ParseBool(row[isNextObjIdx]);
            action.startPos = (startPosIdx >= 0 && row.Count > startPosIdx) ? ParseVector2(row[startPosIdx]) : Vector2.zero;
            action.endPos = (endPosIdx >= 0 && row.Count > endPosIdx) ? ParseVector2(row[endPosIdx]) : Vector2.zero;

            string id = action.key;
            action.id = id;
            string indexPart = action.id.Replace("Prologue_Frame_", "").Trim();
            if (int.TryParse(indexPart, out int idx)) action.index = idx;

            // 시간 및 커브 파싱 (이제 Duration이 정상적으로 가져와짐!)
            action.duration = (durationIdx >= 0 && durationIdx < row.Count) ? ParseTime(row[durationIdx]) : 0f;
            action.conversion = (conversionIdx >= 0 && conversionIdx < row.Count) ? ParseTime(row[conversionIdx]) : 0f;
            action.easeType = (curveIdx >= 0 && curveIdx < row.Count) ? ParseEase(row[curveIdx]) : Ease.Linear;

            // 텍스트 및 화자 파싱
            action.textColor = (textColorIdx >= 0 && textColorIdx < row.Count) ? ParseColor(row[textColorIdx]) : Color.white;
            action.nameColor = (nameColorIdx >= 0 && nameColorIdx < row.Count) ? ParseColor(row[nameColorIdx]) : Color.white;
            action.speaker = (speakerIdx >= 0 && speakerIdx < row.Count) ? row[speakerIdx] : "";

            // Key 값에 따라 실행될 시퀀스 분리
            if (action.key.StartsWith("Prologue_Frame"))
            {
                prologueSequence.Add(action);
            }
            else if (action.key.StartsWith("Text_Frame"))
            {
                textSequence.Add(action);
            }
        }

        // 주의: 더 이상 SplitCsv 도우미 함수는 필요하지 않으므로 삭제하셔도 됩니다.
    }

    // ==========================================
    // 1. 프롤로그 연출 시퀀스 (배경 및 오브젝트 병렬)
    // ==========================================
    private async UniTaskVoid PlayPrologueSequence()
    {
        Managers.Sound.Play("BGM/Prolog", Define.Sound.BGM, 1, 1, false);
        foreach (var action in prologueSequence)
        {
            List<UniTask> tasks = new List<UniTask>();

            Special_Action(action.index);
            Debug.Log(action.duration);
            if (action.isNextBackground)
            {
                if (currentBgIndex >= 0 && currentBgIndex < backgrounds.Count)
                {
                    tasks.Add(backgrounds[currentBgIndex].DOFade(0f, action.duration).SetEase(action.easeType).ToUniTask());
                }

                currentBgIndex++;
                if (currentBgIndex < backgrounds.Count)
                {
                    tasks.Add(backgrounds[currentBgIndex].DOFade(1f, action.duration).SetEase(action.easeType).ToUniTask());
                }

                target_Image = backgrounds[currentBgIndex];
            }

            if (action.isNextObject)
            {
                currentObjIndex++;
                if (currentObjIndex < objects.Count)
                {
                    RectTransform objRect = objects[currentObjIndex];
                    Image objImage = objRect.GetComponent<Image>();

                    objRect.gameObject.SetActive(true);
                    objRect.anchoredPosition = action.startPos;

                    tasks.Add(objRect.DOAnchorPos(action.endPos, action.duration).SetEase(action.easeType).ToUniTask());

                    if (objImage != null)
                    {
                        SetAlpha(objImage, 0f);
                        tasks.Add(objImage.DOFade(1f, action.duration).SetEase(action.easeType).ToUniTask());
                    }
                }
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }
            else if (action.duration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(action.duration));
            }
        }
        IngameData._isPrologueWatched = true;
        SceneLoadingManager.Instance.LoadScene("StageScene");
        Debug.Log("프롤로그 배경/오브젝트 시퀀스 종료!");
    }

    // ==========================================
    // 2. 텍스트 연출 시퀀스 (텍스트 병렬)
    // ==========================================
    private async UniTaskVoid PlayTextSequence()
    {
        foreach (var action in textSequence)
        {
            string localizedContent = string.IsNullOrEmpty(action.key) ? "" : LocalizationManager.Get(action.key);

            // 화자 매핑
            string locSpeakerKey = speakerKeyMap.ContainsKey(action.speaker) ? speakerKeyMap[action.speaker] : "";
            string localizedName = string.IsNullOrEmpty(locSpeakerKey) ? "" : LocalizationManager.Get(locSpeakerKey);

            bool isTextEmpty = string.IsNullOrWhiteSpace(localizedContent) || localizedContent == "X" || localizedContent == "~";

            List<UniTask> fadeTasks = new List<UniTask>();

            if (isTextEmpty)
            {
                // [텍스트가 없어지는 경우] - Conversion 시간 동안 Fade Out
                if (content_Text.color.a > 0f)
                {
                    fadeTasks.Add(speakerName_Text.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());
                    fadeTasks.Add(content_Text.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());
                    fadeTasks.Add(dimmenel_Panel.DOFade(0f, action.conversion).SetEase(action.easeType).ToUniTask());
                }
            }
            else
            {
                // [텍스트가 생기는 경우]
                speakerName_Text.text = localizedName;
                content_Text.text = localizedContent;

                if (content_Text.color.a <= 0.05f)
                {
                    // 아예 꺼져있었다면 완전히 투명한 상태에서 Fade In 시작
                    Color startNameColor = action.nameColor; startNameColor.a = 0f;
                    Color startTextColor = action.textColor; startTextColor.a = 0f;

                    speakerName_Text.color = startNameColor;
                    content_Text.color = startTextColor;
                }

                // Conversion 시간 동안 컬러/알파값 Fade In (또는 변경)
                fadeTasks.Add(speakerName_Text.DOColor(action.nameColor, action.conversion).SetEase(action.easeType).ToUniTask());
                fadeTasks.Add(content_Text.DOColor(action.textColor, action.conversion).SetEase(action.easeType).ToUniTask());
                fadeTasks.Add(dimmenel_Panel.DOFade(1,action.conversion).SetEase(action.easeType).ToUniTask());
            }

            // 페이드 인/아웃(Conversion) 대기
            if (fadeTasks.Count > 0)
            {
                await UniTask.WhenAll(fadeTasks);
            }

            // Duration 시간 동안 해당 텍스트 상태 유지 (대기)
            if (action.duration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(action.duration));
            }
        }
        Debug.Log("프롤로그 텍스트 시퀀스 종료!");
    }


    #region Helpers (파싱 및 유틸)
    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    private bool ParseBool(string val)
    {
        val = val.Trim().ToUpper();
        return val == "TRUE" || val == "1" || val == "O";
    }

    private Vector2 ParseVector2(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return Vector2.zero;

        // 쉼표(,) 대신 슬래시(/)를 기준으로 분리하도록 수정
        string[] split = val.Split('/');
        if (split.Length >= 2)
        {
            float.TryParse(split[0], out float x);
            float.TryParse(split[1], out float y);
            return new Vector2(x, y);
        }
        return Vector2.zero;
    }
    private float ParseTime(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return 0f;
        string numStr = val.ToLower().Replace("ms", "").Replace("s", "").Trim();
        if (float.TryParse(numStr, out float ms))
        {
            return val.ToLower().Contains("ms") ? ms / 1000f : ms;
        }
        return 0f;
    }

    private Ease ParseEase(string easeStr)
    {
        if (string.IsNullOrWhiteSpace(easeStr)) return Ease.Linear;
        string normalized = easeStr.Replace(" ", "").ToLower();

        switch (normalized)
        {
            case "easeout": return Ease.OutQuad;
            case "easein": return Ease.InQuad;
            case "easeinout": return Ease.InOutQuad;
            default:
                if (Enum.TryParse<Ease>(easeStr, true, out Ease ease)) return ease;
                return Ease.Linear;
        }
    }

    private Color ParseColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.white;
        if (!hex.StartsWith("#")) hex = "#" + hex;
        if (ColorUtility.TryParseHtmlString(hex, out Color color)) return color;
        return Color.white;
    }

    // 큰따옴표 안의 쉼표와 줄바꿈을 무시하고 정확히 셀을 나누는 커스텀 CSV 파서
    private List<List<string>> ParseCSV(string text)
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> currentRow = new List<string>();
        bool inQuotes = false;
        string currentValue = "";

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '\"')
                {
                    // 이스케이프된 큰따옴표("") 처리
                    if (i + 1 < text.Length && text[i + 1] == '\"')
                    {
                        currentValue += '\"';
                        i++;
                    }
                    else
                    {
                        inQuotes = false; // 따옴표 닫힘
                    }
                }
                else
                {
                    currentValue += c; // 따옴표 안의 쉼표나 줄바꿈은 그냥 문자로 추가
                }
            }
            else
            {
                if (c == '\"')
                {
                    inQuotes = true; // 따옴표 열림
                }
                else if (c == ',')
                {
                    // 셀 종료
                    currentRow.Add(currentValue.Trim());
                    currentValue = "";
                }
                else if (c == '\r')
                {
                    continue; // 캐리지 리턴 무시
                }
                else if (c == '\n')
                {
                    // 행 종료
                    currentRow.Add(currentValue.Trim());
                    rows.Add(currentRow);
                    currentRow = new List<string>();
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }
        }

        // 마지막 데이터 처리
        if (!string.IsNullOrEmpty(currentValue) || text.EndsWith(","))
        {
            currentRow.Add(currentValue.Trim());
        }
        if (currentRow.Count > 0)
        {
            rows.Add(currentRow);
        }

        return rows;
    }


    private List<string> SplitCsv(string line)
    {
        List<string> result = new List<string>();
        string[] values = line.Split(',');
        foreach (var v in values) result.Add(v.Trim());
        return result;
    }

    private void Special_Action(int index)
    {
        Debug.Log($"{index} 스페셜 액션 시작");
        Image t = target_Image;
        switch (index)
        {
            case 1:
                Action_StartMapMove_0(t);
                break;
            case 7:
                Action_StartMapMove_1(t);
                break;
            case 25:
                Action_StartMapMove_2(t);
                break;
            default:
                break;


        }

    }
    #region Special Actions
    private async UniTaskVoid Action_StartMapMove_0(Image target_Image)
    {
        await UniTask.WaitForSeconds(action0_waitDeleayStart);

        if (target_Image == null) return;
        RectTransform rect = target_Image.rectTransform;

        // 1. 초기 세팅 (Scale 1, PosY -1550)
        rect.localScale = Vector3.one;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, action0_StartPosY);

        // 2. 스케일 축소(0.4)와 Y축 이동(-600)을 동시에 실행 (병렬)
        var scaleTask = rect.DOScale(action0_TargetScale, action0_Duration_Step1).SetEase(Ease.InOutQuad).ToUniTask();
        var moveTask = rect.DOAnchorPosY(action0_TargetPosY_Step1, action0_Duration_Step1).SetEase(Ease.InOutQuad).ToUniTask();

        await UniTask.WhenAll(scaleTask, moveTask); // 두 애니메이션이 끝날 때까지 대기

        // 3. 그리고 내려가서 Y축 634로 이동 (순차 실행)
        await rect.DOAnchorPosY(action0_TargetPosY_Step2, action0_Duration_Step2).SetEase(Ease.InOutQuad).ToUniTask();

        Debug.Log("Action_StartMapMove_0 연출 완료");
    }

    private async UniTaskVoid Action_StartMapMove_1(Image target_Image)
    {
        await UniTask.WaitForSeconds(action1_waitDeleayStart);

        if (target_Image == null) return;
        RectTransform rect = target_Image.rectTransform;

        // 1. 초기 세팅 (PosX 200)
        rect.anchoredPosition = new Vector2(action1_StartPosX, rect.anchoredPosition.y);

        // 2. X축 천천히 이동 (-200까지)
        await rect.DOAnchorPosX(action1_TargetPosX, action1_Duration).SetEase(Ease.Linear).ToUniTask();

        Debug.Log("Action_StartMapMove_1 연출 완료");
    }

    private async UniTaskVoid Action_StartMapMove_2(Image target_Image)
    {
        await UniTask.WaitForSeconds(action2_waitDeleayStart);

        if (target_Image == null) return;
        RectTransform rect = target_Image.rectTransform;

        // 1. 초기 세팅 (Scale 1)
        rect.localScale = Vector3.one * action2_StartScale;

        // 2. 스케일 천천히 축소 (0.5까지)
        await rect.DOScale(action2_TargetScale, action2_Duration).SetEase(Ease.InOutExpo).ToUniTask();

        Debug.Log("Action_StartMapMove_2 연출 완료");
    }
    #endregion
    #endregion
}
