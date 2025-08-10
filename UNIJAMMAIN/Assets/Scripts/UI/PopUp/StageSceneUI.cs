using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class StageSceneUI : UI_Popup
{
    private Button _selectedButton = null;

    //맵 이동관련
    public RectTransform mapImage;
    private float moveDistance = 1100f;  // 이동 거리
    private float moveDuration = 0.8f;   // 애니메이션 지속 시간


    enum Buttons
    {
        UpButton,
        DownButton,
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
    }


    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.UpButton).gameObject.AddUIEvent(UpButtonClicked);
        GetButton((int)Buttons.DownButton).gameObject.AddUIEvent(DownButtonClicked);
        GetButton((int)Buttons.StageButton_1).gameObject.AddUIEvent(StageButton_1_Clicked);
        GetButton((int)Buttons.StageButton_2).gameObject.AddUIEvent(StageButton_2_Clicked);
        GetButton((int)Buttons.StageButton_3).gameObject.AddUIEvent(StageButton_3_Clicked);
        GetButton((int)Buttons.StageButton_4).gameObject.AddUIEvent(StageButton_4_Clicked);
        /*
        GetButton((int)Buttons.StageButton_5).gameObject.AddUIEvent(StageButton_5_Clicked);
        GetButton((int)Buttons.StageButton_6).gameObject.AddUIEvent(StageButton_6_Clicked);
        GetButton((int)Buttons.StageButton_7).gameObject.AddUIEvent(StageButton_7_Clicked);
        GetButton((int)Buttons.StageButton_8).gameObject.AddUIEvent(StageButton_8_Clicked);
        */
    }


    public void UpButtonClicked(PointerEventData eventData)
    {
        mapImage.DOKill(); // 기존 애니메이션 중단
        Vector2 targetPos = mapImage.anchoredPosition - new Vector2(0, moveDistance);
        mapImage.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutCubic);
    }

    public void DownButtonClicked(PointerEventData eventData)
    {
        mapImage.DOKill(); // 기존 애니메이션 중단
        Vector2 targetPos = mapImage.anchoredPosition + new Vector2(0, moveDistance);
        mapImage.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutCubic);
    }


    public void StageButton_1_Clicked(PointerEventData eventData)
    {
        var btn = GetButton((int)Buttons.StageButton_1);
        ResetAllButtonStates();
        HighlightButton(btn);
    }
    public void StageButton_2_Clicked(PointerEventData eventData)
    {
        var btn = GetButton((int)Buttons.StageButton_1);
        ResetAllButtonStates();
        HighlightButton(btn);
    }
    public void StageButton_3_Clicked(PointerEventData eventData)
    {
        var btn = GetButton((int)Buttons.StageButton_1);
        ResetAllButtonStates();
        HighlightButton(btn);
    }
    public void StageButton_4_Clicked(PointerEventData eventData)
    {

    }
    public void StageButton_5_Clicked(PointerEventData eventData)
    {

    }
    public void StageButton_6_Clicked(PointerEventData eventData)
    {

    }
    public void StageButton_7_Clicked(PointerEventData eventData)
    {

    }
    public void StageButton_8_Clicked(PointerEventData eventData)
    {

    }



    private void ResetAllButtonStates()
    {
        /*
        for (int i = 0; i <= (int)Buttons.StageButton_8; i++)
        {
            var btn = GetButton(i);
            var image = btn.GetComponent<Image>();
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();

            if (image != null)
                image.color = Color.white; // 기본 배경색

            if (text != null)
                text.color = Color.black; // 기본 텍스트 색
        }
        */
    }

    private void HighlightButton(Button button)
    {
        var image = button.GetComponent<Image>();
        var text = button.GetComponentInChildren<TextMeshProUGUI>();

        if (image != null)
            image.color = new Color(0.3f, 0.6f, 1f); // 예: 파란색 계열 강조

        if (text != null)
            text.color = Color.white;
    }

}
