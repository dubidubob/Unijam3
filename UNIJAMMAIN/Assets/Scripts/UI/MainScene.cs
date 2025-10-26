using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class MainScene : UI_Popup
{
    // --- UI ��� �� �ִϸ��̼� ���� ���� ---
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

    // --- ���� ���� ���� ���� ---
    private Material originalMaterial;
    private Vector2[] originalPositions;
    private Sequence toStartSequence;
    private bool isAnimating = false; // UI �ִϸ��̼� ��� ���� �ٽ� �÷���
     
    private const float ANIMATION_DURATION = 1.0f; // �ִϸ��̼� �ð� (���)
    private const float INPUT_UNLOCK_TIME = 0.7f;  // �Է� ��� ���� �ð� (�� ������, ��뼺�� ���� �־���.)

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

        StartCoroutine(NotifyManagerWhenReady());
    }

    private IEnumerator NotifyManagerWhenReady()
    {
        // ���� ��� Start �Լ��� ����ǰ� ù �������� �׸� �ð��� �����ϰ� Ȯ���մϴ�.
        //yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        // SceneLoadingManager���� "���� �� ��� ��!" ��� ��ȣ�� �����ϴ�.
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
        toStartText.fontMaterial.SetFloat("_UnderlayDilate", 0.3f);
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
        toStartSequence?.Kill();
        toStartText.fontMaterial = originalMaterial;
        toStartText.transform.localScale = Vector3.one;
        toStartText.color = Color.white;
        if (eventData.pointerPress != null)
        {
            Destroy(eventData.pointerPress);
        }
        Debug.Log("����, ����");
        PlayComingAnimation();
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
        tmpText[index].fontMaterial = new Material(toStartText.fontSharedMaterial);
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

    // --- �ٽ� ����: ��ư Ŭ�� �̺�Ʈ ���� ���� ---
    private void ButtonClicked(PointerEventData eventData, CanClcikState targetState, int index)
    {
        if (isAnimating) return;

        if (currentState == targetState)
        {
            SetButtonClose(index);
        }
        else if (currentState != CanClcikState.Nothing)
        {
            // [������] �ڷ�ƾ�� �ݵ�� StartCoroutine���� ȣ���ؾ� �մϴ�.
            StartCoroutine(ChangePanelWithReset(targetState, index));
        }
        else
        {
            currentState = targetState;
            SetButtonOpen(index);
        }
    }

    // --- �ٽ� ����: �г� ��ȯ �ִϸ��̼� (�ڷ�ƾ) ---
    private IEnumerator ChangePanelWithReset(CanClcikState targetState, int index)
    {
        isAnimating = true; // ��ü ��ȯ ���� ���� UI ���

        int oldIndex = (currentState == CanClcikState.isOptionClick) ? 1 : 2;

        TogglePanel(false);
        ResetHighlight(oldIndex);
        ResetButtons(); // 1��¥�� �ð��� �ݱ� �ִϸ��̼� ����

        // �ݱ� �ִϸ��̼�(1��)�� ������ ���� ������ ��ٸ�
        yield return new WaitForSeconds(ANIMATION_DURATION);

        // �ݱⰡ ���� ��, �� �г��� ���� ���� ����
        currentState = targetState;
        SetButtonOpen(index); // SetButtonOpen�� �ڽ��� Ÿ�̸ӷ� isAnimating�� �ٽ� ����
    }

    // --- �ٽ� ����: �г� ���� ---
    private void SetButtonOpen(int index)
    {
        isAnimating = true; // �ִϸ��̼� ���� -> ��� ���
        Managers.Sound.Play("SFX/UI/SettingCredit_V2", Define.Sound.SFX);

        // [������] 0.8�� �Ŀ� UI ����� �����ϴ� 'Ÿ�̸�' ����
        DOVirtual.DelayedCall(INPUT_UNLOCK_TIME, () => { isAnimating = false; });

        // 1��¥�� '�ð���' �ִϸ��̼� ���
        for (int i = 0; i < index + 1; i++)
        {
            buttonsTransform[i].DOAnchorPosY(originalPositions[i].y + 250, ANIMATION_DURATION).SetEase(Ease.OutCubic);
        }
        for (int i = index + 1; i < buttonsTransform.Length; i++)
        {
            buttonsTransform[i].DOAnchorPosY(originalPositions[i].y - 250, ANIMATION_DURATION).SetEase(Ease.OutCubic);
        }

        TogglePanel(true);
        TextGlow(index);
    }

    // --- �ٽ� ����: �г� �ݱ� ---
    private void SetButtonClose(int index)
    {
        isAnimating = true; // �ִϸ��̼� ���� -> ��� ���

        // [������] 0.8�� �Ŀ� '��� ����'�� '���� �ʱ�ȭ'�� '���ÿ�' �����ϴ� Ÿ�̸� ����
        DOVirtual.DelayedCall(INPUT_UNLOCK_TIME, () =>
        {
            isAnimating = false;
            currentState = CanClcikState.Nothing;
        });

        // 1��¥�� '�ð���' �ִϸ��̼Ǹ� ȣ��
        ResetButtons();
        TogglePanel(false);
        ResetHighlight(index);
    }

    private void ResetHighlight(int index)
    {
        if (index < 0 || index >= tmpText.Length || tmpText[index] == null) return;
        tmpText[index].fontMaterial = originalMaterial;
    }

    // --- �ٽ� ����: ��ư ��ġ �ʱ�ȭ ---
    public void ResetButtons()
    {
        for (int i = 0; i < buttonsTransform.Length; i++)
        {
            buttonsTransform[i].DOAnchorPos(originalPositions[i], ANIMATION_DURATION).SetEase(Ease.OutCubic);
        }
    }

    // --- �ٽ� ����: �г� ���̵� ȿ�� ---
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