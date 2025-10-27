using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening; // DOTween ���ӽ����̽� �߰�

public class StageLevelSceneUI : MonoBehaviour
{
    private Image backGroundImage;
    private CanvasGroup canvasGroup; // CanvasGroup ���� �߰�
    public TMP_Text tmpText;
    public TMP_Text extraText;
    public List<string> extraTextString;

    public bool isMoving = false;

    // Start ��� Awake ��� ���� (GetComponent�� Awake����)
    private void Awake()
    {
        backGroundImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>(); // CanvasGroup ������Ʈ ��������

        // ������Ʈ�� ���� ��� ���� ����
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup ������Ʈ�� �����ϴ�. GameObject�� �߰����ּ���.");
            canvasGroup = gameObject.AddComponent<CanvasGroup>(); // �ӽ÷� �߰�
        }

        // ���� �� �����ϰ� ����
        canvasGroup.alpha = 0f;
    }

    

    /// <summary>
    /// �������� ���� UI�� ǥ���ϴ� �ڷ�ƾ (1-based index)
    /// </summary>
    /// <param name="nowStageLevel">���� �������� ���� (1, 2, 3...)</param>
    public IEnumerator SetStageLevelSceneUI(int nowStageLevel)
    {
        canvasGroup.blocksRaycasts = true;
        isMoving = true;
        // 0. �ؽ�Ʈ �ʱ�ȭ
        tmpText.text = "";
        tmpText.transform.localScale = Vector3.zero; // ũ�� �ʱ�ȭ

        // 1. ��Ʈ������ CanvasGroup õõ�� -> ��� ���� (Fade In)
        yield return canvasGroup.DOFade(1f, 0.7f).WaitForCompletion();


        // 2. tmpText ������Ʈ�� ����ó�� ������ �ִϸ��̼�
        // 3. tmpText.text = "��{nowStageLevel}��" ; ���� ����
        tmpText.text = $"��{nowStageLevel+1}��";

        tmpText.transform.localScale = new Vector2(3f, 3f);
        yield return new WaitForSeconds(0.4f);
        // Ease.OutBack�� ����ó�� ���� Ƣ�� ������ �ݴϴ�.
        yield return tmpText.transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();

        yield return new WaitForSeconds(0.4f);
        tmpText.rectTransform.DOAnchorPosX(-240f, 0.7f); // (O) �ν������� Pos X ����
        // 4. ��� ��ٸ�
        yield return new WaitForSeconds(1.0f);

        // 5. tmpText.text �� ���� ���ڿ� extraText[nowStageLevel] �� �����ִ� ���ڸ� ���ʷ� ��

        // nowStageLevel�� 1���� �����ϴ� ���� ���̹Ƿ�, ����Ʈ �ε����δ� -1�� ���ݴϴ�.
        int textIndex = nowStageLevel;

        // ����Ʈ ���� üũ
        if (textIndex >= 0 && textIndex < extraTextString.Count)
        {
            string extra = extraTextString[textIndex];

            // DOText�� ����� Ÿ���� ȿ��
            // (����: �ּ��� '��, ��, �ܤ�' ���(�ڼ� �и�)�� �ƴ� '��', '��' ���(����)���� Ÿ���ε˴ϴ�)
            float typingDuration = extra.Length * 0.15f; // �� ���ڴ� 0.15��
            yield return extraText.DOText(extra, typingDuration)
                                .SetEase(Ease.Linear) // ������ �ӵ��� Ÿ����
                                .WaitForCompletion();
        }
        else
        {
            Debug.LogError($"extraText ����Ʈ�� ��ȿ�� �ε���({textIndex})�� �����ϴ�. nowStageLevel: {nowStageLevel}");
        }


        // 6. ��ô���ϰ�
        yield return new WaitForSeconds(1.5f);

        // 7. SetOffStageLevelSceneUI ����
        StartCoroutine(SetOffStageLevelSceneUI());
    }

    IEnumerator SetOffStageLevelSceneUI()
    {
        // 1. CanvasGroup -> ������� (Fade Out)
        yield return canvasGroup.DOFade(0f, 0.5f).WaitForCompletion();

        // 2. ������ ��ο����� �ؽ�Ʈ clear
        tmpText.text = "";
        extraText.text = "";
        canvasGroup.blocksRaycasts = false;

        tmpText.rectTransform.anchoredPosition = new Vector2(0, 0);
        tmpText.transform.localScale = Vector3.zero; // ������ ���� ũ�⵵ ����
        isMoving = false;
    }
}