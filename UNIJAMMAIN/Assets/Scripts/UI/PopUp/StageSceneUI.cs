using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class StageSceneUI : UI_Popup
{
    private Button _selectedButton = null;

    // 이미지
    [SerializeField]
    public Sprite deActive;
    public Sprite clickActive;
    public Sprite nonClickActive;

    // 맵 이동관련
    public RectTransform mapImage;
    private float moveDistance = 1100f;  // 이동 거리
    private float moveDuration = 0.8f;   // 애니메이션 지속 시간

    private int currentStageIndex = 2; // 현재 스테이지 인덱스 (이 값은 게임 상태에 따라 변경되어야 함)
    private List<Button> stageButtons = new List<Button>();

    enum ButtonState
    {
        DeActive,
        ClickActive,
        NonClickActive
    }

    enum Buttons
    {
        UpButton,
        DownButton,
        StartButton,
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

    private void Start()
    {
        Init();
        UpdateStageButtons();
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.UpButton).gameObject.AddUIEvent(UpButtonClicked);
        GetButton((int)Buttons.DownButton).gameObject.AddUIEvent(DownButtonClicked);
        GetButton((int)Buttons.StartButton).gameObject.AddUIEvent(StartButtonClicked);

        // 스테이지 버튼들을 리스트에 추가하고 이벤트 등록
        for (int i = (int)Buttons.StageButton_1; i <= (int)Buttons.StageButton_9; i++)
        {
            var button = GetButton(i);
            if (button != null)
            {
                stageButtons.Add(button);
                int stageIndex = i - (int)Buttons.StageButton_1 + 1;
                button.gameObject.AddUIEvent((eventData) => StageButtonClicked(button, stageIndex));
            }
        }
    }

    public void UpButtonClicked(PointerEventData eventData)
    {
        mapImage.DOKill();
        Vector2 targetPos = mapImage.anchoredPosition - new Vector2(0, moveDistance);
        mapImage.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutCubic);
    }

    public void DownButtonClicked(PointerEventData eventData)
    {
        mapImage.DOKill();
        Vector2 targetPos = mapImage.anchoredPosition + new Vector2(0, moveDistance);
        mapImage.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutCubic);
    }

    public void StartButtonClicked(PointerEventData eventData)
    {
        //인덱스에 따라 스테이지 이동하기 추가 요망
        Managers.Scene.LoadScene(Define.Scene.GamePlayScene);
    }
    public void StageButtonClicked(Button button, int stageIndex)
    {
        if (stageIndex > currentStageIndex)
        {
            // 아직 도달하지 않은 스테이지는 클릭 무시
            return;
        }

        // 선택된 버튼 업데이트 및 하이라이트
        _selectedButton = button;
        UpdateStageButtons();

        // 게임 씬 로드 (예시)
        if (stageIndex <= currentStageIndex)
        {
            Debug.Log($"Loading Stage {stageIndex}...");
        }
    }

    private void UpdateStageButtons()
    {
        for (int i = 0; i < stageButtons.Count; i++)
        {
            var button = stageButtons[i];
            int stageIndex = i + 1;

            // 현재 스테이지 인덱스를 기준으로 모든 버튼 상태 초기화
            if (stageIndex < currentStageIndex)
            {
                // 클리어한 스테이지
                SetButtonState(button, ButtonState.NonClickActive);
            }
            else if (stageIndex == currentStageIndex)
            {
                // 현재 도달한 스테이지
                SetButtonState(button, ButtonState.NonClickActive);
            }
            else
            {
                // 아직 도달하지 않은 스테이지
                SetButtonState(button, ButtonState.DeActive);
            }

            // 만약 현재 선택된 버튼이 있다면, 해당 버튼만 ClickActive 상태로 오버라이드
            if (_selectedButton != null && _selectedButton == button)
            {
                SetButtonState(button, ButtonState.ClickActive);
            }
        }
    }

    private void SetButtonState(Button button, ButtonState state)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage == null) return;

        switch (state)
        {
            case ButtonState.DeActive:
                buttonImage.sprite = deActive;
                button.interactable = false;
                break;
            case ButtonState.ClickActive:
                buttonImage.sprite = clickActive;
                button.interactable = true;
                break;
            case ButtonState.NonClickActive:
                buttonImage.sprite = nonClickActive;
                button.interactable = true;
                break;
        }
    }
}