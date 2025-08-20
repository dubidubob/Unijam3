using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MainScene : UI_Popup
{
    public Image brush; // Image ������Ʈ (Fill ������ �귯�� �̹���)
    public Image brushText; // �浵��Ʈ!
    public float brushFillDuration = 2f; // �ִϸ��̼� �ð�
    public float brushTextFillDuration = 3f; // �ִϸ��̼� �ð�
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
            Debug.Log($"[DEBUG] {gameObject.name} Ŭ����");
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

        // ��ư Bind
        GetButton((int)Buttons.StoryMode).gameObject.AddUIEvent(StoryModeClicked);
        GetButton((int)Buttons.Option).gameObject.AddUIEvent(OptionClicked);
        GetButton((int)Buttons.Members).gameObject.AddUIEvent(MembersClicked);
        GetButton((int)Buttons.End).gameObject.AddUIEvent(EndClicked);
        GetButton((int)Buttons.StartToClick).gameObject.AddUIEvent(EnterToStartClicked);
    }

    public void EnterToStartClicked(PointerEventData eventData)
    {
        // ������ ��ư�� �ı�
        GameObject clickedObj = eventData.pointerPress;

        if(clickedObj!=null)
        {
            Destroy(clickedObj);
        }
        Debug.Log("����, ����");
        PlayComingAnimation();


    }
    private void PlayBrushFillAnimation()
    {
        brush.fillAmount = 0f; // ������ ����
        brush.DOFillAmount(1f, brushFillDuration)
             .SetEase(Ease.OutCubic); // �ڿ������� �巯������
        brushText.DOFillAmount(1f, brushTextFillDuration*1.5f)
             .SetEase(Ease.OutCubic); // �ڿ������� �巯������
    }


    private void StoryModeClicked(PointerEventData eventData)
    {
        // ���丮��� ���� ����
        Managers.Scene.LoadScene(Define.Scene.StageScene);
    }

    private void OptionClicked(PointerEventData eventData)
    {
        // ���� ���� ����
    }

    private void MembersClicked(PointerEventData eventData)
    {
        // ������ Ȯ�� ����
    }

    private void EndClicked(PointerEventData eventData)
    {
        // ���� ���� ����
    }
}

