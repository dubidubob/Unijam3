using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
public class MainScene : UI_Popup
{
    public Image brush; // Image 컴포넌트 (Fill 설정된 브러쉬 이미지)
    public Image brushText; // 득도비트!
    public float brushFillDuration = 2f; // 애니메이션 시간
    public float brushTextFillDuration = 3f; // 애니메이션 시간
    public float comeTime = 1;

    public Transform leftImage;
    public Transform rightImage;

    public RectTransform[] buttonsTransform;
    public TMP_Text[] tmpText;

    private Material originalMaterial; // 초기값
    private Vector2[] originalPositions; // 원래 위치 저장용

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
        // 시작할 때 원래 머티리얼 저장
        originalMaterial = tmpText[0].fontSharedMaterial;

        // 원래 위치 저장
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
        // 제작진 확인 로직
    }

    private void EndClicked(PointerEventData eventData)
    {
        Application.Quit();// 게임 종료 로직
    }

    private void SetButtonOpen(int index)
    {
        Debug.Log($"현재 상태 열려있는 상태{isOpen} : 현재 상태 : {currentState}");
        if (isOpen) // 오픈된 상태면 
        {
            ResetButtons();
            ResetHighlight(index);

            if (currentState == CanClcikState.isOptionClick) //옵션창이 열려있는 상태라면
            {
                TogglePanel(); // 이미지 페이드인, 여기에서 isOpen조절
            }
            else if(currentState ==CanClcikState.isMemberClick)
            {
                TogglePanel();
            }

            return;
        }
        TogglePanel(); // 이미지 페이드인, 여기에서 isOpen조절



        //buttonsTransform 위로
        for (int i = 0; i < index+1; i++)
        {
            buttonsTransform[i].DOAnchorPosY(buttonsTransform[i].anchoredPosition.y + 300, 1f) // 1f = 이동 시간
               .SetEase(Ease.OutCubic);
        }

        //buttonsTransform 아래로
        for (int i = index+1; i < 4; i++)
        {
            buttonsTransform[i].DOAnchorPosY(buttonsTransform[i].anchoredPosition.y - 300, 1f) // 1f = 이동 시간
               .SetEase(Ease.OutCubic);
        }

        //buttonsTransform 하이라이트
        TextGlow(index);

        
    }

    private void TextGlow(int index)
    {
        tmpText[index].fontMaterial = new Material(tmpText[index].fontSharedMaterial);

        // Glow (Underlay)
        tmpText[index].fontMaterial.EnableKeyword("UNDERLAY_ON");
        tmpText[index].fontMaterial.SetColor("_UnderlayColor", new Color(1f, 1f, 0f, 0.8f)); // 노란빛
        tmpText[index].fontMaterial.SetFloat("_UnderlaySoftness", 0.8f);
        tmpText[index].fontMaterial.SetFloat("_UnderlayDilate", 0.5f);

        // Outline
        tmpText[index].fontMaterial.SetFloat("_OutlineWidth", 0.2f);
        tmpText[index].fontMaterial.SetColor("_OutlineColor", Color.yellow);
    }

    private void ResetHighlight(int index)
    {
        // 원래 머티리얼로 복원
        tmpText[index].fontMaterial = originalMaterial;
    }

    public void ResetButtons()
    {
        // 원래 자리로 되돌리기
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

