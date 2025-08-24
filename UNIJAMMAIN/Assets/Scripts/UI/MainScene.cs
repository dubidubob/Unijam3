using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
public class MainScene : UI_Popup
{
    public Image brush; // Image ������Ʈ (Fill ������ �귯�� �̹���)
    public Image brushText; // �浵��Ʈ!
    public float brushFillDuration = 2f; // �ִϸ��̼� �ð�
    public float brushTextFillDuration = 3f; // �ִϸ��̼� �ð�
    public float comeTime = 1;

    public Transform leftImage;
    public Transform rightImage;

    public RectTransform[] buttonsTransform;
    public TMP_Text[] tmpText;

    private Material originalMaterial; // �ʱⰪ
    private Vector2[] originalPositions; // ���� ��ġ �����

    public CanvasGroup optionPanel;
    public CanvasGroup memberPanel;
    private bool isOpen = false;

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
        // ������ �� ���� ��Ƽ���� ����
        originalMaterial = tmpText[0].fontSharedMaterial;

        // ���� ��ġ ����
        originalPositions = new Vector2[buttonsTransform.Length];
        for (int i = 0; i < buttonsTransform.Length; i++)
        {
            originalPositions[i] = buttonsTransform[i].anchoredPosition;
        }

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
        int index = 1;
        if (currentState == CanClcikState.isMemberClick)
        {
            ResetButtons();
            ResetHighlight(index);
            TogglePanel();
        }
        currentState = CanClcikState.isOptionClick;
        SetButtonOpen(1);
    }

    private void MembersClicked(PointerEventData eventData)
    {
        int index = 2;
        if(currentState==CanClcikState.isOptionClick)
        {
            ResetButtons();
            ResetHighlight(index);
            TogglePanel();
        }
        currentState = CanClcikState.isMemberClick;
        SetButtonOpen(index);
        // ������ Ȯ�� ����
    }

    private void EndClicked(PointerEventData eventData)
    {
        Application.Quit();// ���� ���� ����
    }

    private void SetButtonOpen(int index)
    {
        Debug.Log($"���� ���� �����ִ� ����{isOpen} : ���� ���� : {currentState}");
        if (isOpen) // ���µ� ���¸� 
        {
            ResetButtons();
            ResetHighlight(index);

            if (currentState == CanClcikState.isOptionClick) //�ɼ�â�� �����ִ� ���¶��
            {
                TogglePanel(); // �̹��� ���̵���, ���⿡�� isOpen����
            }
            else if(currentState ==CanClcikState.isMemberClick)
            {
                TogglePanel();
            }

            return;
        }
        TogglePanel(); // �̹��� ���̵���, ���⿡�� isOpen����



        //buttonsTransform ����
        for (int i = 0; i < index+1; i++)
        {
            buttonsTransform[i].DOAnchorPosY(buttonsTransform[i].anchoredPosition.y + 300, 1f) // 1f = �̵� �ð�
               .SetEase(Ease.OutCubic);
        }

        //buttonsTransform �Ʒ���
        for (int i = index+1; i < 4; i++)
        {
            buttonsTransform[i].DOAnchorPosY(buttonsTransform[i].anchoredPosition.y - 300, 1f) // 1f = �̵� �ð�
               .SetEase(Ease.OutCubic);
        }

        //buttonsTransform ���̶���Ʈ
        TextGlow(index);

        
    }

    private void TextGlow(int index)
    {
        tmpText[index].fontMaterial = new Material(tmpText[index].fontSharedMaterial);

        // Glow (Underlay)
        tmpText[index].fontMaterial.EnableKeyword("UNDERLAY_ON");
        tmpText[index].fontMaterial.SetColor("_UnderlayColor", new Color(1f, 1f, 0f, 0.8f)); // �����
        tmpText[index].fontMaterial.SetFloat("_UnderlaySoftness", 0.8f);
        tmpText[index].fontMaterial.SetFloat("_UnderlayDilate", 0.5f);

        // Outline
        tmpText[index].fontMaterial.SetFloat("_OutlineWidth", 0.2f);
        tmpText[index].fontMaterial.SetColor("_OutlineColor", Color.yellow);
    }

    private void ResetHighlight(int index)
    {
        // ���� ��Ƽ����� ����
        tmpText[index].fontMaterial = originalMaterial;
    }

    public void ResetButtons()
    {
        // ���� �ڸ��� �ǵ�����
        for (int i = 0; i < buttonsTransform.Length; i++)
        {
            buttonsTransform[i].DOAnchorPos(originalPositions[i], 1f)
                .SetEase(Ease.OutCubic);
        }
    }

    void TogglePanel()
    {
        CanvasGroup panel;

        if(currentState==CanClcikState.isOptionClick)
        {
            panel = optionPanel;
        }
        else if(currentState == CanClcikState.isMemberClick)
        {
            panel = memberPanel;
        }
        else
        {
            panel = null;
        }

        if (isOpen)
        {
            // Fade Out
            Debug.Log("FadeOut");
            panel.DOFade(0f, 0.5f).OnComplete(() =>
            {
                panel.interactable = false;
                panel.blocksRaycasts = false;
            });
        }
        else
        {
            // Fade In
            Debug.Log("FadeIn");
            panel.DOFade(1f, 0.5f).OnStart(() =>
            {
                panel.interactable = true;
                panel.blocksRaycasts = true;
            });
        }

        isOpen = !isOpen;
        if(!isOpen)
        {
            currentState = CanClcikState.Nothing;
        }
    }

}

