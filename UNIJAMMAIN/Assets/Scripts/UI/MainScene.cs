using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class MainScene : UI_Popup
{
    // ... (기존 변수들은 동일)
    public Image brush;
    public Image brushText;
    public float brushFillDuration = 2f;
    public float brushTextFillDuration = 3f;
    public float comeTime = 1;

    public Transform leftImage;
    public Transform rightImage;

    public RectTransform[] buttonsTransform;
    public TMP_Text[] tmpText;

    private Material originalMaterial;
    private Vector2[] originalPositions;

    public CanvasGroup optionPanel;
    public CanvasGroup memberPanel;
    private bool isOpen = false;

    public TMP_Text toStartText;
    private Sequence toStartSequence;

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

        Init();
        PlayBrushFillAnimation();
        AnimateToStartText();
    }

    public class DebugUIEvent : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[DEBUG] {gameObject.name} 클릭됨");
        }
    }

    private void AnimateToStartText()
    {
        toStartText.fontMaterial = new Material(toStartText.fontSharedMaterial);
        toStartText.fontMaterial.EnableKeyword("UNDERLAY_ON");
        toStartText.fontMaterial.SetColor("_UnderlayColor", new Color(1f, 0.8f, 0f, 1f));
        toStartText.fontMaterial.SetFloat("_UnderlaySoftness", 0.5f);
        toStartText.fontMaterial.SetFloat("_UnderlayDilate", 0.3f);
        toStartText.fontMaterial.SetFloat("_UnderlayOffsetX", 0f);
        toStartText.fontMaterial.SetFloat("_UnderlayOffsetY", 0f);

        if (toStartSequence != null)
        {
            toStartSequence.Kill();
        }

        toStartSequence = DOTween.Sequence();
        float originalScale = toStartText.transform.localScale.x;

        toStartSequence.Append(toStartText.DOFade(0.2f, 0.6f));
        toStartSequence.Append(toStartText.DOFade(1f, 0.6f));

        toStartSequence.Join(toStartText.transform.DOScale(originalScale * 1.05f, 1.2f).SetEase(Ease.InOutSine));
        toStartSequence.Join(toStartText.transform.DOScale(originalScale, 1.2f).SetEase(Ease.InOutSine).SetDelay(1.2f));

        toStartSequence.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void PlayComingAnimation()
    {
        Managers.Sound.Play("SFX/UI/SettingCredit_V2", Define.Sound.SFX);
        DOTween.To(() => -10f, x => leftImage.transform.position = new Vector3(x, 0f, 0f), 0f, comeTime);
        DOTween.To(() => 10f, x => rightImage.transform.position = new Vector3(x, 0f, 0f), 0f, comeTime);
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
        if (toStartSequence != null)
        {
            toStartSequence.Kill();
        }

        toStartText.fontMaterial = originalMaterial;
        toStartText.transform.localScale = Vector3.one;
        toStartText.color = Color.white;

        GameObject clickedObj = eventData.pointerPress;
        if (clickedObj != null)
        {
            Destroy(clickedObj);
        }
        Debug.Log("시작, 입장");
        PlayComingAnimation();
    }

    private void PlayBrushFillAnimation()
    {
        brush.fillAmount = 0f;
        brush.DOFillAmount(1f, brushFillDuration).SetEase(Ease.OutCubic);
        brushText.DOFillAmount(1f, brushTextFillDuration * 1.5f).SetEase(Ease.OutCubic);
    }

    private void StoryModeClicked(PointerEventData eventData)
    {
        Managers.Sound.Play("SFX/UI/StorySelect_V1", Define.Sound.SFX);
        Managers.Scene.LoadScene(Define.Scene.StageScene);
    }

    private void EndClicked(PointerEventData eventData)
    {
        Application.Quit();
    }

    private void ButtonClicked(PointerEventData eventData, CanClcikState targetState, int index)
    {
        if (currentState == targetState)
        {
            SetButtonClose(index);
        }
        else if (currentState != CanClcikState.Nothing)
        {
            StartCoroutine(ChangePanelWithReset(targetState, index));
        }
        else
        {
            currentState = targetState;
            SetButtonOpen(index);
        }
    }

    private IEnumerator ChangePanelWithReset(CanClcikState targetState, int index)
    {
        int oldIndex = (currentState == CanClcikState.isOptionClick) ? 1 : 2;

        TogglePanel(false);
        yield return new WaitForSeconds(0.5f);

        ResetButtons();
        ResetHighlight(oldIndex);
        yield return new WaitForSeconds(1.0f);

        currentState = targetState;
        SetButtonOpen(index);
    }

    private void SetButtonOpen(int index)
    {
        Managers.Sound.Play("SFX/UI/SettingCredit_V2", Define.Sound.SFX);

        //buttonsTransform 위로
        for (int i = 0; i < index + 1; i++)
        {
            buttonsTransform[i].DOAnchorPosY(buttonsTransform[i].anchoredPosition.y + 300, 1f) // 1f = 이동 시간
                 .SetEase(Ease.OutCubic);
        }

        //buttonsTransform 아래로
        for (int i = index + 1; i < buttonsTransform.Length; i++)
        {
            buttonsTransform[i].DOAnchorPosY(buttonsTransform[i].anchoredPosition.y - 300, 1f) // 1f = 이동 시간
                 .SetEase(Ease.OutCubic);
        }

        // 패널 페이드 인
        TogglePanel(true);

        // 버튼 하이라이트
        TextGlow(index);
        isOpen = true;
    }

    private void SetButtonClose(int index)
    {
        ResetButtons();
        TogglePanel(false);
        ResetHighlight(index);
        isOpen = false;
        currentState = CanClcikState.Nothing;
    }

    private void TextGlow(int index)
    {
        tmpText[index].fontMaterial = new Material(tmpText[index].fontSharedMaterial);
        tmpText[index].fontMaterial.EnableKeyword("UNDERLAY_ON");
        tmpText[index].fontMaterial.SetColor("_UnderlayColor", new Color(1f, 1f, 0f, 0.8f));
        tmpText[index].fontMaterial.SetFloat("_UnderlaySoftness", 0.8f);
        tmpText[index].fontMaterial.SetFloat("_UnderlayDilate", 0.5f);
        tmpText[index].fontMaterial.SetFloat("_UnderlayOffsetX", 0f);
        tmpText[index].fontMaterial.SetFloat("_UnderlayOffsetY", 0f);

        tmpText[index].fontMaterial.SetFloat("_OutlineWidth", 0.2f);
        tmpText[index].fontMaterial.SetColor("_OutlineColor", Color.yellow);
    }

    private void ResetHighlight(int index)
    {
        if (index >= 0 && index < tmpText.Length)
        {
            tmpText[index].fontMaterial = originalMaterial;
        }
    }

    public void ResetButtons()
    {
        DOTween.KillAll(true);

        for (int i = 0; i < buttonsTransform.Length; i++)
        {
            buttonsTransform[i].DOAnchorPos(originalPositions[i], 1f)
                .SetEase(Ease.OutCubic);
        }
    }

    private void TogglePanel(bool isFadeIn)
    {
        CanvasGroup panel = null;
        if (currentState == CanClcikState.isOptionClick)
        {
            panel = optionPanel;
        }
        else if (currentState == CanClcikState.isMemberClick)
        {
            panel = memberPanel;
        }

        if (panel == null) return;

        if (isFadeIn)
        {
            Debug.Log("FadeIn");
            panel.DOFade(1f, 0.5f).OnStart(() =>
            {
                panel.interactable = true;
                panel.blocksRaycasts = true;
            });
        }
        else
        {
            Debug.Log("FadeOut");
            panel.DOFade(0f, 0.5f).OnComplete(() =>
            {
                panel.interactable = false;
                panel.blocksRaycasts = false;
            });
        }
    }
}