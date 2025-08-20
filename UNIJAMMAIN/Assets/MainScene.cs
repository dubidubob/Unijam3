using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MainScene : UI_Popup
{
    public Image brush; // Image 컴포넌트 (Fill 설정된 브러쉬 이미지)
    public Image brushText; // 득도비트!
    public float brushFillDuration = 2f; // 애니메이션 시간
    public float brushTextFillDuration = 3f; // 애니메이션 시간
    public float comeTime = 1;

    public Transform leftImage;
    public Transform rightImage;
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
        Init();
        PlayBrushFillAnimation();
    }
    public class DebugUIEvent : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[DEBUG] {gameObject.name} 클릭됨");
        }
    }


    private void PlayComingAnimation()
    {
        DOTween.To(() => -10f, x => leftImage.transform.position = new Vector3(x, 0f, 0f), 0f, comeTime);
        DOTween.To(() => 10f, x => rightImage.transform.position = new Vector3(x, 0f, 0f), 0f, comeTime);
    }
    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));

        // 버튼 Bind
        GetButton((int)Buttons.StoryMode).gameObject.AddUIEvent(StoryModeClicked);
        GetButton((int)Buttons.Option).gameObject.AddUIEvent(OptionClicked);
        GetButton((int)Buttons.Members).gameObject.AddUIEvent(MembersClicked);
        GetButton((int)Buttons.End).gameObject.AddUIEvent(EndClicked);
        GetButton((int)Buttons.StartToClick).gameObject.AddUIEvent(EnterToStartClicked);
    }

    public void EnterToStartClicked(PointerEventData eventData)
    {
        // 누르면 버튼은 파괴
        GameObject clickedObj = eventData.pointerPress;

        if(clickedObj!=null)
        {
            Destroy(clickedObj);
        }
        Debug.Log("시작, 입장");
        PlayComingAnimation();


    }
    private void PlayBrushFillAnimation()
    {
        brush.fillAmount = 0f; // 시작은 투명
        brush.DOFillAmount(1f, brushFillDuration)
             .SetEase(Ease.OutCubic); // 자연스럽게 드러나도록
        brushText.DOFillAmount(1f, brushTextFillDuration*1.5f)
             .SetEase(Ease.OutCubic); // 자연스럽게 드러나도록
    }


    private void StoryModeClicked(PointerEventData eventData)
    {
        // 스토리모드 진입 로직
        Managers.Scene.LoadScene(Define.Scene.StageScene);
    }

    private void OptionClicked(PointerEventData eventData)
    {
        // 설정 진입 로직
    }

    private void MembersClicked(PointerEventData eventData)
    {
        // 제작진 확인 로직
    }

    private void EndClicked(PointerEventData eventData)
    {
        // 게임 종료 로직
    }
}

