using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Settings;
public class MainScene : UI_Popup
{
    // --- UI 요소 및 애니메이션 설정 변수 ---
    public Image brush;
    public Image brushText;
    public float brushFillDuration = 2f;
    public float brushTextFillDuration = 3f;
    public float comeTime = 1;
    public Transform leftImage;
    public Transform rightImage;
    public RectTransform[] buttonsTransform;
    public TMP_Text[] tmpText;
    public CanvasGroup optionPanel;
    public CanvasGroup memberPanel;
    public TMP_Text toStartText;

    // --- 내부 상태 관리 변수 ---
    private Material originalMaterial;
    private Vector2[] originalPositions;
    private Sequence toStartSequence;
    private bool isAnimating = false; // UI 애니메이션 제어를 위한 핵심 플래그
     
    private const float ANIMATION_DURATION = 1.0f; // 애니메이션 시간 (상수)
    private const float INPUT_UNLOCK_TIME = 0.7f;  // 입력 잠금 해제 시간 (더 빠르게, 사용성을 위해 넣었음.)


    [Header("NewScene Setting")]
    [SerializeField] private CanvasGroup Canvas_GamesLogo;
    [SerializeField] private Image Image_GamesLogoUp;
    [SerializeField] private Image Image_GamesLogoDown;

    [SerializeField] private Image Image_LogoUp;
    [SerializeField] private Image Image_LogoDown;


    [SerializeField] private Image drawing_Image;
    [SerializeField] private List<Image> buttons_Image;
    [SerializeField] private Image patternBackGround_Image;
    [SerializeField] private Image image_Monster1;
    [SerializeField] private Image image_Monster2;

    [Header("엔딩 시청 후 수정해야할 것을")]

    /*
     * 엔딩에 다다르면 -> 패턴바꾸기
     * 수도승 바꾸기
     * brush 바꾸기
     * YinYang Color 활성화
     * YinYang 돌리기
     */

    [SerializeField] private Image image_Monk;
    [SerializeField] private Image image_Brush;
    [SerializeField] private Image image_YinYang;
    

    [SerializeField] private Sprite sprite_Ending_Pattern;
    [SerializeField] private Sprite sprite_Ending_Monk;
    [SerializeField] private Sprite sprite_Ending_Brush;
    [SerializeField] private Sprite sprite_Ending_YinYang;
    [SerializeField] private RectTransform rect_YinYangContainer;
    [SerializeField] private Sprite sprite_Ending_Monster1;
    [SerializeField] private Sprite sprite_Ending_Monster2;
    enum CanClcikState
    {
        isOptionClick,
        isMemberClick,
        Nothing
    }
    private CanClcikState currentState = CanClcikState.Nothing;

    enum Buttons
    {
        StoryMode,
        Option,
        Members,
        End,
        StartToClick
    }

   
    // 씬 로딩이 완료된 직후에 호출해 줍니다.
    public void ForceRefreshLocalization()
    {
        if (LocalizationSettings.SelectedLocale != null)
        {
            // 현재 언어를 자기 자신으로 다시 덮어씌움 (이러면 모든 Localize Event가 강제로 새로고침됨)
            LocalizationSettings.SelectedLocale = LocalizationSettings.SelectedLocale;
        }
    }
    private void Start()
    {
        // 기본 초기화 세팅
        Time.timeScale = 1f;
        originalMaterial = tmpText[0].fontSharedMaterial;
        originalPositions = new Vector2[buttonsTransform.Length];
        Canvas_GamesLogo.alpha = 1;
        for (int i = 0; i < buttonsTransform.Length; i++)
        {
            originalPositions[i] = buttonsTransform[i].anchoredPosition;
        }
        image_Monster1.DOFade(0, 0); //투명
        image_Monster2.DOFade(0, 0); //투명

        if (IngameData._isStoryCompleteClear)
        {
            image_Monk.sprite = sprite_Ending_Monk;
            image_Brush.sprite = sprite_Ending_Brush;
            patternBackGround_Image.sprite = sprite_Ending_Pattern;
            image_Monster1.sprite = sprite_Ending_Monster1;
   
            image_Monster2.sprite = sprite_Ending_Monster2;
            image_Monster1.SetNativeSize();
            image_Monster2.SetNativeSize();
        }

        //  로컬라이제이션 완료 대기 후 로고 액션 시작
        StartGameSequenceAsync().Forget();
    }

    private async UniTask StartGameSequenceAsync()
    {
        //  로컬라이제이션 시스템이 완전히 로드될 때까지 대기
        await LocalizationSettings.InitializationOperation;

        // 이제 로컬라이제이션 준비가 끝났으므로 강제 새로고침 실행
        ForceRefreshLocalization();

        // 기존의 로고 분기 처리
        if (IngameData._wastSceneName == "StageScene" || IngameData._wastSceneName == "EndingScene")
        {
            Debug.Log("로고스킵");
            Canvas_GamesLogo.alpha = 0;
            await ActionLogo();
        }
        else
        {
            await ActionGamesLogo();
        }
    }

    private async UniTask ActionGamesLogo()
    {
        // 0. 초기 설정
        CanvasGroup gamesLogoCanvasGroup = Canvas_GamesLogo;
        Image[] logoParts = { Image_GamesLogoUp, Image_GamesLogoDown };

        // 초기 상태: 모두 투명하게 시작
        foreach (var v in logoParts)
        {
            v.color = new Color(1, 1, 1, 0);
        }


        // 1. 모든 로고 동시 페이드 인
        Sequence fadeInSeq = DOTween.Sequence();
        foreach (var v in logoParts)
        {
            // Join을 써야 모든 이미지가 '동시에' 페이드 됩니다.
            fadeInSeq.Join(v.DOFade(1f, 0.7f));
        }
        await fadeInSeq.ToUniTask();

        // 2. 로고 보여주는 대기 시간
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.5f));

        // 3. 모든 로고 동시 페이드 아웃
        Sequence fadeOutSeq = DOTween.Sequence();
        foreach (var v in logoParts)
        {
            fadeOutSeq.Join(v.DOFade(0f, 0.7f));
        }
        await fadeOutSeq.ToUniTask();

        fadeInSeq.Join(gamesLogoCanvasGroup.DOFade(0, 1f));


        // 4. 다음 단계로 넘어가기 전 짧은 대기
        //Managers.Sound.Play("BGM/MainTitle_V3", Define.Sound.BGM);
        Managers.Sound.Play("BGM/MainTitleogg", Define.Sound.BGM);
        await UniTask.Delay(System.TimeSpan.FromSeconds(1f));

        // 5. 다음 로고 액션 진행
        await ActionLogo();
    }

    private async UniTask ActionLogo()
    {
        // ★ 핵심 추가: UI 캔버스가 레이아웃을 계산할 수 있도록 딱 한 프레임만 대기!
        await UniTask.Yield(PlayerLoopTiming.Update);

        StartCoroutine(NotifyManagerWhenReady());
      


        Managers.Sound.Play("SFX/UI/SettingCredit_V2", Define.Sound.SFX);

        // 0. 초기 설정 (이미지들이 안 보이는 상태로 시작)
        Image_LogoUp.fillAmount = 0;
        Image_LogoUp.SetNativeSize();
        Image_LogoDown.SetNativeSize();

        // LogoDown은 투명하고 약간 크게 설정 (찍히기 전 상태)
        Color downColor = Image_LogoDown.color;
        downColor.a = 0;
        Image_LogoDown.color = downColor;
        Image_LogoDown.transform.localScale = Vector3.one * 1.2f;

        // 1. LogoUpImage Filled 1로 채우기 (붓으로 쓰는 느낌)
        // .ToUniTask()를 붙여서 애니메이션이 끝날 때까지 비동기로 대기합니다.
        await Image_LogoUp.DOFillAmount(1f, 0.5f)
            .SetEase(Ease.InSine)
            .ToUniTask();

        // 2. 약간의 시간차 (여운)
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));

        // 3. LogoDownImage 도장 찍기 애니메이션
        // 알파값 조절과 크기 조절을 동시에 진행
        Sequence stampSeq = DOTween.Sequence();

        stampSeq.Join(Image_LogoDown.DOFade(1f, 0f)); // 순식간에 나타남
        stampSeq.Join(Image_LogoDown.transform.DOScale(1f, 0.3f).SetEase(Ease.InBack)); // 쿵! 찍히는 느낌

        // 도장이 찍히는 순간 화면(또는 로고 전체)을 살짝 흔들어줌
        stampSeq.OnComplete(() => {
            // 카메라나 로고 부모 객체를 흔들어도 좋지만, 여기서는 로고 자체를 살짝 흔듭니다.
            Image_LogoDown.transform.DOShakePosition(0.2f, 10f, 20);
            // 여기서 도장 찍히는 소리(SFX)를 재생하면 최고입니다.
            // Managers.Sound.Play("SFX/Stamp", Define.Sound.SFX); 
        });

        await stampSeq.ToUniTask();

        // 4. 모든 로고 연출 끝난 뒤 메인 액션 시작
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));

        StartMainSceneAction();
    }

    private void StartMainSceneAction()
    {
        Init();


        // PlayBrushFillAnimation();
        AnimateToStartText();

      
    }

    private IEnumerator NotifyManagerWhenReady()
    {
        // 씬의 모든 Start 함수가 실행되고 첫 프레임을 그릴 시간을 안전하게 확보합니다.
        //yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        // SceneLoadingManager에게 "이제 문 열어도 돼!" 라고 신호를 보냅니다.
        if (SceneLoadingManager.Instance != null)
        {
            SceneLoadingManager.Instance.NotifySceneReady();
        }
    }

    private void AnimateToStartText()
    {
        toStartText.fontMaterial = new Material(toStartText.fontSharedMaterial);
        toStartText.fontMaterial.EnableKeyword("UNDERLAY_ON");
        toStartText.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 1f));
        toStartText.fontMaterial.SetFloat("_UnderlaySoftness", 0.5f);
        toStartText.fontMaterial.SetFloat("_UnderlayOffsetX", 0f);
        toStartText.fontMaterial.SetFloat("_UnderlayOffsetY", 0f);

        toStartSequence?.Kill();
        toStartSequence = DOTween.Sequence();
        float originalScale = toStartText.transform.localScale.x;

        toStartSequence.Append(toStartText.DOFade(0.2f, 0.6f));
        toStartSequence.Append(toStartText.DOFade(1f, 0.6f));
        toStartSequence.Join(toStartText.transform.DOScale(originalScale * 1.05f, 1.2f).SetEase(Ease.InOutSine));
        toStartSequence.Join(toStartText.transform.DOScale(originalScale, 1.2f).SetEase(Ease.InOutSine).SetDelay(1.2f));
        toStartSequence.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private async UniTask PlayComingAnimation()
    {
        // 시퀀스 생성 (애니메이션 그룹화)
        Sequence seq = DOTween.Sequence();

        // 2. 좌우 이미지 이동 (동시에 실행)
        // float 변수 제어 대신 UI이므로 DOAnchorPos(RectTransform)를 권장하지만, 
        // 기존 코드 스타일(Position)을 유지하여 작성합니다.
        seq.Append(leftImage.transform.DOMoveX(0f, comeTime).From(new Vector3(-10, 0f, 0f)).SetEase(Ease.OutQuad));

        // 3. 이동이 완료된 후 실행될 애니메이션들 (Append 사용)
        float fillDuration = 0.5f; // 채워지는 시간 설정

        // Drawing과 Muck 이미지를 1로 채움
        seq.Join(drawing_Image.DOFillAmount(1f, comeTime).SetEase(Ease.OutQuad));

        seq.Append(rightImage.transform.DOMoveX(0f, comeTime).From(new Vector3(10f, 0f, 0f)).SetEase(Ease.OutQuad));

        /*
        // 버튼 리스트들을 순차적으로 혹은 동시에 채움
        foreach (var btnImg in buttons_Image)
        {
            // Join을 쓰면 동시에, Append를 쓰면 하나씩 차례대로 실행됩니다.
            seq.Join(btnImg.DOFillAmount(1f, fillDuration).SetEase(Ease.OutQuad));
        }
        */
        // 시퀀스 맨 마지막에 실행될 함수 등록
        seq.OnComplete(() =>
        {
            // 1. Monster들의 현재(기준) 위치 기억 및 초기화 (아래로 100만큼 내림)
            // RectTransform을 사용하므로 anchoredPosition을 활용하는 것이 정확합니다.
            RectTransform m1Rect = image_Monster1.rectTransform;
            RectTransform m2Rect = image_Monster2.rectTransform;

            Vector2 m1TargetPos = m1Rect.anchoredPosition;
            Vector2 m2TargetPos = m2Rect.anchoredPosition;

            // 시작 위치 설정: 현재 위치에서 Y축으로 -100만큼 내리고, 투명하게 설정
            m1Rect.anchoredPosition = new Vector2(m1TargetPos.x, m1TargetPos.y - 100f);
            m2Rect.anchoredPosition = new Vector2(m2TargetPos.x, m2TargetPos.y - 100f);

         
            // 2. 새로운 시퀀스로 몬스터 등장 연출
            Sequence monsterSeq = DOTween.Sequence();

            // Monster 1 등장 (위로 올라오면서 Fade In)
            monsterSeq.Join(m1Rect.DOAnchorPos(m1TargetPos, 0.5f).SetEase(Ease.OutQuad));
            monsterSeq.Join(image_Monster1.DOFade(1f, 0.5f));

            // Monster 2 등장 (동시에 실행)
            monsterSeq.Join(m2Rect.DOAnchorPos(m2TargetPos, 0.5f).SetEase(Ease.OutQuad));
            monsterSeq.Join(image_Monster2.DOFade(1f, 0.5f));

            // 몬스터 등장까지 끝난 후, 모든 캐릭터 유영 시작
            monsterSeq.OnComplete(() =>
            {
                StartFloatingAnimation(image_Monk.rectTransform, 10f, 2.5f);     // 수도승
                StartFloatingAnimation(image_Monster1.rectTransform, 25f, 3.0f); // 몬스터1 (좀 더 느리고 크게)
                StartFloatingAnimation(image_Monster2.rectTransform, 20f, 2.2f); // 몬스터2 (좀 더 빠르고 작게)
            });

        });



    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.StoryMode).gameObject.AddUIEvent(StoryModeClicked);
        GetButton((int)Buttons.Option).gameObject.AddUIEvent(eventData => ButtonClicked(eventData, CanClcikState.isOptionClick, 1));
        GetButton((int)Buttons.Members).gameObject.AddUIEvent(eventData => ButtonClicked(eventData, CanClcikState.isMemberClick, 2));
        GetButton((int)Buttons.End).gameObject.AddUIEvent(EndClicked);
        GetButton((int)Buttons.StartToClick).gameObject.AddUIEvent(EnterToStartClicked);
        Image_LogoUp.SetNativeSize();
        Image_LogoDown.SetNativeSize();

    }

    public void EnterToStartClicked(PointerEventData eventData)
    {
        Managers.Sound.Play("SFX/UI/PressToStart_V1",Define.Sound.SFX);

        toStartSequence?.Kill();
        toStartText.fontMaterial = toStartText.font.material;
        toStartText.transform.localScale = Vector3.one;
        toStartText.color = Color.white;
        if (eventData.pointerPress != null)
        {
            Destroy(eventData.pointerPress);
        }
        Debug.Log("시작, 입장");

        // 1. 배경 밝아지는 애니메이션 먼저 실행
        // patternBackGround_Image의 컬러를 흰색(기본밝기)으로 1초 동안 변경
        patternBackGround_Image.DOColor(Color.white, 1.0f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
            // 완전히 다 밝아지면 실행
            PlayComingAnimation().Forget();
            });
    }

    private void PlayBrushFillAnimation()
    {
        brush.fillAmount = 0f;
        brush.DOFillAmount(1f, brushFillDuration).SetEase(Ease.OutCubic);
        brushText.DOFillAmount(1f, brushTextFillDuration * 1.5f).SetEase(Ease.OutCubic);
    }

    private void TextGlow(int index)
    {
        if (index < 0 || index >= tmpText.Length || tmpText[index] == null) return;
        tmpText[index].fontMaterial = new Material(tmpText[index].fontSharedMaterial);
        tmpText[index].fontMaterial.EnableKeyword("UNDERLAY_ON");
        tmpText[index].fontMaterial.SetColor("_UnderlayColor", Color.white);
        tmpText[index].fontMaterial.SetFloat("_UnderlaySoftness", 0.4f);
        tmpText[index].fontMaterial.SetFloat("_UnderlayDilate", 0.15f);
        tmpText[index].fontMaterial.SetFloat("_UnderlayOffsetX", 0f);
        tmpText[index].fontMaterial.SetFloat("_UnderlayOffsetY", 0f);
        tmpText[index].fontMaterial.SetFloat("_OutlineWidth", 0.05f);
        tmpText[index].fontMaterial.SetColor("_OutlineColor", Color.white);
    }

    private void StoryModeClicked(PointerEventData eventData)
    {
        if (isAnimating) return;
        TextGlow((int)Buttons.StoryMode);
        Managers.Sound.Play("SFX/UI/StorySelect_V1", Define.Sound.SFX);
        if(IngameData._isPrologueWatched)
        {
            SceneLoadingManager.Instance.LoadScene("StageScene");
        }
        else
        {
            SceneLoadingManager.Instance.LoadScene("PrologueScene");
        }
    }

    private void EndClicked(PointerEventData eventData)
    {
        if (isAnimating) return;
        TextGlow((int)Buttons.End);
        Application.Quit();
    }

    // --- 핵심 로직: 버튼 클릭 이벤트 통합 관리 ---
    private void ButtonClicked(PointerEventData eventData, CanClcikState targetState, int index)
    {
        if (isAnimating) return;

        if (currentState == targetState)
        {
            SetButtonClose(index);
        }
        else if (currentState != CanClcikState.Nothing)
        {
            // [수정됨] 코루틴은 반드시 StartCoroutine으로 호출해야 합니다.
            StartCoroutine(ChangePanelWithReset(targetState, index));
        }
        else
        {
            currentState = targetState;
            SetButtonOpen(index);
        }
    }

    // --- 핵심 로직: 패널 전환 애니메이션 (코루틴) ---
    private IEnumerator ChangePanelWithReset(CanClcikState targetState, int index)
    {
        isAnimating = true; // 전체 전환 과정 동안 UI 잠금

        int oldIndex = (currentState == CanClcikState.isOptionClick) ? 1 : 2;

        TogglePanel(false);
        ResetHighlight(oldIndex);
        ResetButtons(); // 1초짜리 시각적 닫기 애니메이션 시작

        // 닫기 애니메이션(1초)이 완전히 끝날 때까지 기다림
        yield return new WaitForSeconds(ANIMATION_DURATION);

        // 닫기가 끝난 후, 새 패널을 여는 동작 시작
        currentState = targetState;
        SetButtonOpen(index); // SetButtonOpen이 자신의 타이머로 isAnimating을 다시 관리
    }

    // --- 핵심 로직: 패널 열기 ---
    private void SetButtonOpen(int index)
    {
        isAnimating = true; // 애니메이션 시작 -> 즉시 잠금
        Managers.Sound.Play("SFX/UI/SettingCredit_V2", Define.Sound.SFX);

        // [수정됨] 0.8초 후에 UI 잠금을 해제하는 '타이머' 설정
        DOVirtual.DelayedCall(INPUT_UNLOCK_TIME, () => { isAnimating = false; });

        // 1초짜리 '시각적' 애니메이션 재생
        for (int i = 0; i < index + 1; i++)
        {
            buttonsTransform[i].DOAnchorPosY(originalPositions[i].y + 150, ANIMATION_DURATION).SetEase(Ease.OutCubic);
        }
        for (int i = index + 1; i < buttonsTransform.Length; i++)
        {
            buttonsTransform[i].DOAnchorPosY(originalPositions[i].y - 320, ANIMATION_DURATION).SetEase(Ease.OutCubic);
        }

        TogglePanel(true);
        TextGlow(index);
    }

    // --- 핵심 로직: 패널 닫기 ---
    private void SetButtonClose(int index)
    {
        isAnimating = true; // 애니메이션 시작 -> 즉시 잠금

        // [수정됨] 0.8초 후에 '잠금 해제'와 '상태 초기화'를 '동시에' 실행하는 타이머 설정
        DOVirtual.DelayedCall(INPUT_UNLOCK_TIME, () =>
        {
            isAnimating = false;
            currentState = CanClcikState.Nothing;
        });

        // 1초짜리 '시각적' 애니메이션만 호출
        ResetButtons();
        TogglePanel(false);
        ResetHighlight(index);
    }

    private void ResetHighlight(int index)
    {
        if (index < 0 || index >= tmpText.Length || tmpText[index] == null) return;
        tmpText[index].fontMaterial = tmpText[index].font.material;
    }

    // --- 핵심 로직: 버튼 위치 초기화 ---
    public void ResetButtons()
    {
        for (int i = 0; i < buttonsTransform.Length; i++)
        {
            buttonsTransform[i].DOAnchorPos(originalPositions[i], ANIMATION_DURATION).SetEase(Ease.OutCubic);
        }
    }

    // --- 핵심 로직: 패널 페이드 효과 ---
    private void TogglePanel(bool isFadeIn)
    {
        CanvasGroup panel = null;
        if (currentState == CanClcikState.isOptionClick) panel = optionPanel;
        else if (currentState == CanClcikState.isMemberClick) panel = memberPanel;

        if (panel == null) return;

        if (isFadeIn)
        {
            panel.DOFade(1f, 0.5f).OnStart(() =>
            {
                panel.interactable = true;
                panel.blocksRaycasts = true;
            });
        }
        else
        {
            panel.DOFade(0f, 0.5f).OnComplete(() =>
            {
                panel.interactable = false;
                panel.blocksRaycasts = false;
            });
        }
    }
    /// <summary>
    /// UI 요소를 위아래로 부드럽게 유영(Floating)시키는 공용 메서드
    /// </summary>
    /// <param name="target">움직일 RectTransform</param>
    /// <param name="amplitude">움직임 범위 (픽셀)</param>
    /// <param name="duration">한 번 왕복하는 시간</param>
    private void StartFloatingAnimation(RectTransform target, float amplitude, float duration)
    {
        if (target == null) return;

        // 기존에 혹시 돌아가고 있을지 모를 트윈 제거 (안전성)
        target.DOKill();

        // 현재 위치를 기준으로 위아래 반복
        // SetRelative(true)를 써서 현재 anchoredPosition 기준 상대적으로 움직이게 설정
        target.DOAnchorPosY(amplitude, duration)
            .SetRelative(true)
            .SetEase(Ease.InOutSine) // 부드러운 가속/감속
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 (왔다갔다)
    }

}