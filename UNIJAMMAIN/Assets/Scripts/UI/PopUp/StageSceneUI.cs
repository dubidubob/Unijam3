using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StageSceneUI : UI_Popup
{
    private Button _selectedButton = null;
    private Button _hoveredButton = null;

    // 이미지
    [SerializeField]
    public Sprite deActive;
    public Sprite clickActive;
    public Sprite nonClickActive;

    // 텍스트 머티리얼
    [SerializeField]
    private Material normalTextMaterial; // 이 변수만 인스펙터에서 할당하면 됩니다.
    private Material glowingTextMaterial; // 코드에서 자동으로 생성할 머티리얼

    // 맵 이동관련
    private int currentPageLevel = 0;
    private bool isAnimating = false;
    public Ease moveEase = Ease.OutCubic; // 이동 애니메이션의 Ease 효과
    public float rotateDuration = 1f; // 회전에 걸리는 시간
    public RectTransform mapImage;
    private float moveDistance = 1100f;
    private float moveDuration = 0.8f;

    private int currentStageIndex = 2;
    private List<Button> stageButtons = new List<Button>();

    public TMP_Text startButtonText;
   

    [Header("Stage Data Inputs")]
    public List<StageUIData> stageDataList;
    public Tmp_StageSceneResultUI stageSceneResultUI;
    [Header("Text Objects")]
    public TMP_Text stageMainText;
    public TMP_Text stageMainSubText;
    public TMP_Text stageLevelText;

    [SerializeField] GameObject completedObject;
    

    


    enum ButtonState
    {
        DeActive,
        ClickActive,
        NonClickActive,
        Hover
    }

    enum Buttons
    {
        UpButton,
        DownButton,
        StartButton,
        ToMain,
        StageButton_1,
        StageButton_2,
        StageButton_3,
        StageButton_4,
        StageButton_5,
        StageButton_6,
        StageButton_7,
        StageButton_8,
        StageButton_9
    }

    private void Update()
    {
        // ESC버튼
        // ▼▼▼ 로딩 중이 아닐 때만 ESC 키를 받도록 조건 추가 ▼▼▼
        if (Input.GetKeyDown(KeyCode.Escape) && !SceneLoadingManager.IsLoading)
        {
            //ToMain으로의 버튼
            ToMainButtonClicked(null);
        }
    }


        private void Awake()
    {
        // normalTextMaterial이 있다면, 이를 기반으로 빛나는 머티리얼을 생성합니다.
        if (normalTextMaterial != null)
        {
            // Material(Material source) 생성자를 사용해 복사본을 만듭니다.
            glowingTextMaterial = new Material(normalTextMaterial);
            SetupGlowMaterial(glowingTextMaterial);
        }
        currentStageIndex = Managers.Game.GameStage+1;
    }

    private void OnDestroy()
    {
        // 씬이 전환되거나 오브젝트가 파괴될 때 동적으로 생성한 머티리얼을 정리합니다.
        if (glowingTextMaterial != null)
        {
            Destroy(glowingTextMaterial);
        }
    }

    // ✨ 빛나는 머티리얼의 속성을 설정하는 함수
    private void SetupGlowMaterial(Material material)
    {
        // TextMesh Pro의 언더레이(Underlay) 기능을 고 속성을 설정합니다.
        material.EnableKeyword("UNDERLAY_ON");
        material.SetColor("_UnderlayColor", new Color(1f, 1f, 0.8f, 1f)); // 노란빛이 도는 흰색
        material.SetFloat("_UnderlaySoftness", 0.5f);
        material.SetFloat("_UnderlayDilate", 0.3f);
    }

    private void Start()
    {
        Init();
        UpdateStageButtons();

        var startButton = GetButton((int)Buttons.StartButton);
        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
        }

        // 두 번 호출되므로, (StageScene에서 한 번 이미 호출함) 주석처리함.
        //Managers.Sound.Play("BGM/MainScene_V2", Define.Sound.BGM);

        // 서울게임타운용
        Managers.Game.GameStage = 7;
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));

        // Up, Down, Start 버튼의 참조 가져오기
        var upButton = GetButton((int)Buttons.UpButton);
        var downButton = GetButton((int)Buttons.DownButton);
        var startButton = GetButton((int)Buttons.StartButton);
        GetButton((int)Buttons.ToMain).gameObject.AddUIEvent(ToMainButtonClicked);

        // 각 버튼의 클릭 이벤트 및 마우스 오버/이탈 이벤트 연결
        if (upButton != null)
        {
            upButton.gameObject.AddUIEvent(UpButtonClicked);
            AddPointerEvent(upButton, (eventData) => OnPointerEnter(upButton), EventTriggerType.PointerEnter);
            AddPointerEvent(upButton, (eventData) => OnPointerExit(upButton), EventTriggerType.PointerExit);
        }
  

        if (downButton != null)
        {
            downButton.gameObject.AddUIEvent(DownButtonClicked);
            AddPointerEvent(downButton, (eventData) => OnPointerEnter(downButton), EventTriggerType.PointerEnter);
            AddPointerEvent(downButton, (eventData) => OnPointerExit(downButton), EventTriggerType.PointerExit);
        }
   

        if (startButton != null)
        {
            startButton.gameObject.AddUIEvent(StartButtonClicked);
            AddPointerEvent(startButton, (eventData) => OnPointerEnter(startButton), EventTriggerType.PointerEnter);
            AddPointerEvent(startButton, (eventData) => OnPointerExit(startButton), EventTriggerType.PointerExit);
        }

        for (int i = (int)Buttons.StageButton_1; i <= (int)Buttons.StageButton_9; i++)
        {
            var button = GetButton(i);
            if (button != null)
            {
                stageButtons.Add(button);
                int stageIndex = i - (int)Buttons.StageButton_1 + 1;

                button.gameObject.AddUIEvent((eventData) => StageButtonClicked(button, stageIndex));
                AddPointerEvent(button, (eventData) => OnPointerEnter(button), EventTriggerType.PointerEnter);
                AddPointerEvent(button, (eventData) => OnPointerExit(button), EventTriggerType.PointerExit);

                //Completed 설정
                IngameData.ChapterIdx = stageIndex-1;


                if (IngameData.ChapterRank!=Define.Rank.Unknown)
                {
                    // 1. Instantiate 시 부모를 바로 지정해주는 것이 더 안정적입니다.
                    GameObject obj = Instantiate(completedObject, button.gameObject.transform);
                    RectTransform rect = obj.GetComponent<RectTransform>();

                    // 2. [핵심] .position 대신 .anchoredPosition을 사용합니다.
                    rect.anchoredPosition = new Vector2(120, 0);

                    // 3. 부모의 스케일 값에 영향을 받지 않도록 1로 초기화합니다.
                    rect.localScale = Vector3.one;
                }
            }
        }
    }

    public void OnPointerEnter(Button button)
    {
        if (_selectedButton == button || !button.interactable) return;
        _hoveredButton = button;
        SetButtonState(button, ButtonState.Hover);
    }

    public void OnPointerExit(Button button)
    {
        if (_hoveredButton == button)
        {
            _hoveredButton = null;
            UpdateStageButtons();
        }
    }

    private void AddPointerEvent(Button button, System.Action<PointerEventData> action, EventTriggerType eventType)
    {
        var trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }
        var entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((eventData) => action((PointerEventData)eventData));
        trigger.triggers.Add(entry);
    }

    public void UpButtonClicked(PointerEventData eventData)
    {

        if (isAnimating) return; // 애니메이션 중에는 입력을 무시

        switch (currentPageLevel)
        {
            case 0:
                // [Level 0 -> 1] : y좌표 -295으로 이동
                MoveTo(yPos: -295f);
                currentPageLevel = 1;
                Managers.Sound.Play("SFX/UI/GoTo456Stage_V1", Define.Sound.SFX);
                break;

            case 1:
                // [Level 1 -> 2] : z축 180도 회전, y좌표 892으로 이동
                RotateAndMoveTo(zRot: 180f, yPos: 892f);
                currentPageLevel = 2;
                Managers.Sound.Play("SFX/UI/GoToFinalStage_V1",Define.Sound.SFX, 1f, 3f);
                break;

            case 2:
                // Level 2가 마지막 레벨이므로 아무 동작 안 함
                Debug.Log("Already at the top level.");
                Managers.Sound.Play("SFX/UI/GoToNowhere_V1", Define.Sound.SFX);
                break;
        }
    }

    public void DownButtonClicked(PointerEventData eventData)
    {
        if (isAnimating) return; // 애니메이션 중에는 입력을 무시

        switch (currentPageLevel)
        {
            case 0:
                // Level 0이 최하단 레벨이므로 아무 동작 안 함
                Debug.Log("Already at the bottom level.");
                Managers.Sound.Play("SFX/UI/GoToNowhere_V1", Define.Sound.SFX);
                break;

            case 1:
                // [Level 1 -> 0] : y좌표 892으로 이동
                MoveTo(yPos: 892f);
                currentPageLevel = 0;
                Managers.Sound.Play("SFX/UI/GoTo123Stage_V1", Define.Sound.SFX, 1f, 2f);
                break;

            case 2:
                // [Level 2 -> 1] : z축 0도로 복귀, y좌표 -295으로 이동
                RotateAndMoveTo(zRot: 0f, yPos: -295f);
                currentPageLevel = 1;
                Managers.Sound.Play("SFX/UI/GoTo456Stage_V1", Define.Sound.SFX, 1f, 2f);
                break;
        }
    }
    public void ToMainButtonClicked(PointerEventData eventData)
    {
        // 여기서도 한 번 더 확인하여 중복 호출을 완벽하게 막습니다.
        if (SceneLoadingManager.IsLoading) return;

        SceneLoadingManager.Instance.LoadScene("MainTitle");
    }

    public void StartButtonClicked(PointerEventData eventData)
    {
        if (_selectedButton != null)
        {
            SceneLoadingManager.Instance.LoadScene("StoryScene");
        }
        else
        {
            Debug.Log("Please select a stage.");
        }
    }

    public void StageButtonClicked(Button button, int stageIndex)
    {
        if (stageIndex > currentStageIndex)
        {
            return;
        }

        var startButton = GetButton((int)Buttons.StartButton);
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
        }

        StartButtonAnimation();
        Managers.Sound.Play("SFX/UI/StageClick_V1", Define.Sound.SFX);

        IngameData.ChapterIdx = stageIndex - 1;
        _selectedButton = button;
        _hoveredButton = null;
        UpdateStageButtons();
        TextSetting(IngameData.ChapterIdx);
        stageSceneResultUI.LoadClickedStageData(IngameData.ChapterIdx);
    }

    private void UpdateStageButtons()
    {
        if (_selectedButton == null)
        {
            if (currentStageIndex > 0 && currentStageIndex <= stageButtons.Count)
            {
                _selectedButton = stageButtons[currentStageIndex - 1];
            }
        }

        for (int i = 0; i < stageButtons.Count; i++)
        {
            var button = stageButtons[i];
            int stageIndex = i + 1;

            if (stageIndex < currentStageIndex)
            {
                SetButtonState(button, ButtonState.NonClickActive);
            }
            else if (stageIndex == currentStageIndex)
            {
                SetButtonState(button, ButtonState.NonClickActive);
            }
            else
            {
                SetButtonState(button, ButtonState.DeActive);
            }

            if (_selectedButton != null && _selectedButton == button)
            {
                SetButtonState(button, ButtonState.ClickActive);
            }
        }
    }

    private void SetButtonState(Button button, ButtonState state)
    {
        Image buttonImage = button.GetComponent<Image>();
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        if (buttonImage == null || buttonText == null) return;

        switch (state)
        {
            case ButtonState.DeActive:
                buttonImage.sprite = deActive;
                button.interactable = false;
                buttonText.color = new Color32(194, 194, 194, 255);
                buttonText.fontSharedMaterial = normalTextMaterial;
                break;
            case ButtonState.ClickActive:
                buttonImage.sprite = clickActive;
                button.interactable = true;
                buttonText.color = Color.white;
                // ✨ glowingTextMaterial이 생성되었다면 적용합니다.
                //if (glowingTextMaterial != null)
                //{
                //    buttonText.fontSharedMaterial = glowingTextMaterial;
                //}
                //else
                //{
                //    Debug.LogWarning("Glowing material is not set up. Make sure normalTextMaterial is assigned in the Inspector.");
                //}
                // Glow 효과 너무 세서 우선 아예 뻈음 - 예준
                break;
            case ButtonState.NonClickActive:
                buttonImage.sprite = nonClickActive;
                button.interactable = true;
                buttonText.color = new Color32(194, 194, 194, 255);
                buttonText.fontSharedMaterial = normalTextMaterial;
                break;
            case ButtonState.Hover:
                if (button != _selectedButton)
                {
                    buttonText.color = Color.white;
                }
                break;
        }
    }

    private void StartButtonAnimation()
    {
        // 버튼 찾기
        var startButton = GetButton((int)Buttons.StartButton);
        if (startButton == null) return;

        var startButtonImage = startButton.GetComponent<Image>();
        if (startButtonImage == null) return;

        var startButtonText = startButton.GetComponentInChildren<TMP_Text>();
        if (startButtonText == null) return;

        // Set initial state
        startButtonImage.type = Image.Type.Filled;
        startButtonImage.fillAmount = 0f;
        startButtonImage.fillOrigin = (int)Image.Origin360.Top; // Or any origin you prefer

        startButtonText.alpha = 0f;

        // Animate the fill amount from 0 to 1
        startButtonImage.DOFillAmount(1f, 0.3f) // 애니메이션 지속시간 0.5초
            .SetEase(Ease.OutCubic);


        startButtonText.DOFade(1.0f, 0.5f); // Fade in the text over 0.5 seconds
    }

    private void TextSetting(int index)
    {
        stageMainText.text = stageDataList[index].stageMainText;
        stageMainSubText.text = stageDataList[index].stageMainSubText;
        stageLevelText.text = stageDataList[index].levelText;

    }
    #region Tool

    private void MoveTo(float yPos)
    {
        isAnimating = true;
        mapImage.DOKill(); // 진행 중인 모든 애니메이션을 즉시 중지

        Vector2 targetPos = new Vector2(mapImage.anchoredPosition.x, yPos);
        mapImage.DOAnchorPos(targetPos, moveDuration)
                .SetEase(moveEase)
                .OnComplete(() => isAnimating = false); // 애니메이션 완료 시 플래그 해제
    }

    private void RotateAndMoveTo(float zRot, float yPos)
    {
        isAnimating = true;
        mapImage.DOKill();

        // 위치 이동과 회전을 동시에 실행
        Vector2 targetPos = new Vector2(mapImage.anchoredPosition.x, yPos);
        mapImage.DOAnchorPos(targetPos, moveDuration).SetEase(moveEase);

        mapImage.DORotate(new Vector3(0, 0, zRot), rotateDuration)
                .SetEase(moveEase)
                .OnComplete(() => isAnimating = false); // 애니메이션 완료 시 플래그 해제
    }
    #endregion
}