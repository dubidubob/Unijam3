using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class StageSceneUI : UI_Popup
{
    private Button _selectedButton = null;

    // �̹���
    [SerializeField]
    public Sprite deActive;
    public Sprite clickActive;
    public Sprite nonClickActive;

    // �� �̵�����
    public RectTransform mapImage;
    private float moveDistance = 1100f;  // �̵� �Ÿ�
    private float moveDuration = 0.8f;   // �ִϸ��̼� ���� �ð�

    private int currentStageIndex = 2; // ���� �������� �ε��� (�� ���� ���� ���¿� ���� ����Ǿ�� ��)
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

        // �������� ��ư���� ����Ʈ�� �߰��ϰ� �̺�Ʈ ���
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
        //�ε����� ���� �������� �̵��ϱ� �߰� ���
        Managers.Scene.LoadScene(Define.Scene.GamePlayScene);
    }
    public void StageButtonClicked(Button button, int stageIndex)
    {
        if (stageIndex > currentStageIndex)
        {
            // ���� �������� ���� ���������� Ŭ�� ����
            return;
        }

        // ���õ� ��ư ������Ʈ �� ���̶���Ʈ
        _selectedButton = button;
        UpdateStageButtons();

        // ���� �� �ε� (����)
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

            // ���� �������� �ε����� �������� ��� ��ư ���� �ʱ�ȭ
            if (stageIndex < currentStageIndex)
            {
                // Ŭ������ ��������
                SetButtonState(button, ButtonState.NonClickActive);
            }
            else if (stageIndex == currentStageIndex)
            {
                // ���� ������ ��������
                SetButtonState(button, ButtonState.NonClickActive);
            }
            else
            {
                // ���� �������� ���� ��������
                SetButtonState(button, ButtonState.DeActive);
            }

            // ���� ���� ���õ� ��ư�� �ִٸ�, �ش� ��ư�� ClickActive ���·� �������̵�
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