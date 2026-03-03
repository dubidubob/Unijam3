using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;

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
    [SerializeField] private Image Monster1;
    [SerializeField] private Image Monster2;
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

    private void Start()
    {
        originalMaterial = tmpText[0].fontSharedMaterial;
        originalPositions = new Vector2[buttonsTransform.Length];
        for (int i = 0; i < buttonsTransform.Length; i++)
        {
            originalPositions[i] = buttonsTransform[i].anchoredPosition;
        }
        ActionGamesLogo().Forget();

       
    }

    private async UniTask ActionGamesLogo()
    {
        // 0. 초기 설정
        CanvasGroup gamesLogoCanvasGroup = Canvas_GamesLogo;

        await UniTask.Delay(System.TimeSpan.FromSeconds(1f));

        gamesLogoCanvasGroup.alpha = 1;

        // 시작 스케일을 5f -> 1.8f로 줄여서 너무 멀리서 날아오지 않게 설정
        float startScale = 1.8f;
        Image_GamesLogoUp.transform.localScale = Vector3.one * startScale;
        Image_GamesLogoDown.transform.localScale = Vector3.one * startScale;

        Image_GamesLogoUp.color = new Color(1, 1, 1, 0);
        Image_GamesLogoDown.color = new Color(1, 1, 1, 0);

        // 1. Up 로고 박힘 (강도 1.2 - "탕!")
        Image_GamesLogoUp.DOFade(1f, 0.05f); // 페이드는 아주 빠르게
                                             // InExpo 대신 OutQuad를 사용하고 시간을 줄여 타격감을 높임
        await Image_GamesLogoUp.transform.DOScale(1f, 0.12f).SetEase(Ease.OutQuad).ToUniTask();
        Image_GamesLogoUp.transform.DOShakePosition(0.2f, 3f, 20); // 절도 있는 짧은 흔들림

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));

        // 2. Down 로고 박힘 (강도 3.6 - "쾅!")
        Image_GamesLogoDown.DOFade(1f, 0.05f);
        await Image_GamesLogoDown.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad).ToUniTask();
        // 강도 3.6을 위해 진동 세기를 유지하면서 타격 시간을 짧게 가져감
        Image_GamesLogoDown.transform.DOShakePosition(0.3f, 9f, 30);

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));

        // 3. 심장 박동하듯이 2번 쿵쿵!
        for (int i = 0; i < 2; i++)
        {
            Sequence beatSeq = DOTween.Sequence();
            // 박동은 0.1초 내외로 짧아야 쫄깃한 느낌이 납니다.
            beatSeq.Join(Image_GamesLogoUp.transform.DOScale(1.1f, 0.08f).SetEase(Ease.OutSine));
            beatSeq.Join(Image_GamesLogoDown.transform.DOScale(1.1f, 0.08f).SetEase(Ease.OutSine));
            beatSeq.Append(Image_GamesLogoUp.transform.DOScale(1f, 0.12f).SetEase(Ease.InSine));
            beatSeq.Join(Image_GamesLogoDown.transform.DOScale(1f, 0.12f).SetEase(Ease.InSine));

            await beatSeq.ToUniTask();
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.05f));
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(1.2f));

        // 4. 사라지기
        await gamesLogoCanvasGroup.DOFade(0f, 1f).ToUniTask();

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.3f));

        // 5. 다음 로고 액션 진행
        await ActionLogo();
    }

    private async UniTask ActionLogo()
    {
        Managers.Sound.Play("BGM/MainTitle_V3", Define.Sound.BGM);

        Managers.Sound.Play("SFX/UI/SettingCredit_V2", Define.Sound.SFX);

        // 0. 초기 설정 (이미지들이 안 보이는 상태로 시작)
        Image_LogoUp.fillAmount = 0;

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

        StartCoroutine(NotifyManagerWhenReady());
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
            RectTransform m1Rect = Monster1.rectTransform;
            RectTransform m2Rect = Monster2.rectTransform;

            Vector2 m1TargetPos = m1Rect.anchoredPosition;
            Vector2 m2TargetPos = m2Rect.anchoredPosition;

            // 시작 위치 설정: 현재 위치에서 Y축으로 -100만큼 내리고, 투명하게 설정
            m1Rect.anchoredPosition = new Vector2(m1TargetPos.x, m1TargetPos.y - 100f);
            m2Rect.anchoredPosition = new Vector2(m2TargetPos.x, m2TargetPos.y - 100f);

            Monster1.color = new Color(1f, 1f, 1f, 0f); // 투명하게 시작
            Monster2.color = new Color(1f, 1f, 1f, 0f);

            // 2. 새로운 시퀀스로 몬스터 등장 연출
            Sequence monsterSeq = DOTween.Sequence();

            // Monster 1 등장 (위로 올라오면서 Fade In)
            monsterSeq.Join(m1Rect.DOAnchorPos(m1TargetPos, 0.5f).SetEase(Ease.OutQuad));
            monsterSeq.Join(Monster1.DOFade(1f, 0.5f));

            // Monster 2 등장 (동시에 실행)
            monsterSeq.Join(m2Rect.DOAnchorPos(m2TargetPos, 0.5f).SetEase(Ease.OutQuad));
            monsterSeq.Join(Monster2.DOFade(1f, 0.5f));
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
        SceneLoadingManager.Instance.LoadScene("StageScene");
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
            buttonsTransform[i].DOAnchorPosY(originalPositions[i].y - 150, ANIMATION_DURATION).SetEase(Ease.OutCubic);
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
}