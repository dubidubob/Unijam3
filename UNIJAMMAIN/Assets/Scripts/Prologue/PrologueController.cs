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

    private void Start()
    {
        InitializeUI();
        LocalizationManager.LoadAll();
        LoadPrologueSequenceData("Localization/PrologueTable"); // 경로에 맞게 수정

        // 두 시퀀스를 병렬(Parallel)로 실행
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
        using (StringReader reader = new StringReader(raw))
        {
            string headerLine = reader.ReadLine();
            var headers = SplitCsv(headerLine);

            int keyIdx = headers.FindIndex(x => x.Trim().ToLower() == "key");
            int isNextBgIdx = headers.FindIndex(x => x.Trim().ToLower() == "isnextbackground");
            int isNextObjIdx = headers.FindIndex(x => x.Trim().ToLower() == "isnextobject");
            int startPosIdx = headers.FindIndex(x => x.Trim().ToLower() == "startpos");
            int endPosIdx = headers.FindIndex(x => x.Trim().ToLower() == "endpos");
            int curveIdx = headers.FindIndex(x => x.Trim().ToLower() == "curve");
            int durationIdx = headers.FindIndex(x => x.Trim().ToLower() == "duration");
            int conversionIdx = headers.FindIndex(x => x.Trim().ToLower() == "conversion");
            int textColorIdx = headers.FindIndex(x => x.Trim().ToLower() == "textcolor");
            int nameColorIdx = headers.FindIndex(x => x.Trim().ToLower() == "namecolor");
            int speakerIdx = headers.FindIndex(x => x.Trim().ToLower() == "speaker");

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var row = SplitCsv(line);

                if (keyIdx < 0 || row.Count <= keyIdx) continue;

                PrologueAction action = new PrologueAction();
                action.key = row[keyIdx].Trim();

                // 오브젝트 및 배경 처리 파싱
                action.isNextBackground = (isNextBgIdx >= 0 && row.Count > isNextBgIdx) && ParseBool(row[isNextBgIdx]);
                action.isNextObject = (isNextObjIdx >= 0 && row.Count > isNextObjIdx) && ParseBool(row[isNextObjIdx]);
                action.startPos = (startPosIdx >= 0 && row.Count > startPosIdx) ? ParseVector2(row[startPosIdx]) : Vector2.zero;
                action.endPos = (endPosIdx >= 0 && row.Count > endPosIdx) ? ParseVector2(row[endPosIdx]) : Vector2.zero;

                // 시간 및 커브 파싱
                action.duration = (durationIdx >= 0 && durationIdx < row.Count) ? ParseTime(row[durationIdx]) : 0f;
                action.conversion = (conversionIdx >= 0 && conversionIdx < row.Count) ? ParseTime(row[conversionIdx]) : 0f;
                action.easeType = (curveIdx >= 0 && curveIdx < row.Count) ? ParseEase(row[curveIdx]) : Ease.Linear;

                // 텍스트 및 화자 파싱
                action.textColor = (textColorIdx >= 0 && textColorIdx < row.Count) ? ParseColor(row[textColorIdx]) : Color.white;
                action.nameColor = (nameColorIdx >= 0 && nameColorIdx < row.Count) ? ParseColor(row[nameColorIdx]) : Color.white;
                action.speaker = (speakerIdx >= 0 && speakerIdx < row.Count) ? row[speakerIdx].Trim() : "";

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
        }
    }

    // ==========================================
    // 1. 프롤로그 연출 시퀀스 (배경 및 오브젝트 병렬)
    // ==========================================
    private async UniTaskVoid PlayPrologueSequence()
    {
        foreach (var action in prologueSequence)
        {
            List<UniTask> tasks = new List<UniTask>();

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

        string[] split = val.Split(',');
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

    private List<string> SplitCsv(string line)
    {
        List<string> result = new List<string>();
        string[] values = line.Split(',');
        foreach (var v in values) result.Add(v.Trim());
        return result;
    }
    #endregion
}
