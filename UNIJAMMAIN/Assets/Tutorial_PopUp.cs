using System.Collections;
using UnityEngine;
using TMPro;

public class Tutorial_PopUp : UI_Popup
{
    public TMP_Text text;
    public GameObject contents;

    public float appearSpeed = 2f;
    public float disappearDelay = 2f;
    public float disappearSpeed = 2f;
    public float startOffset = -100f;

    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;
   
    public override void Init()
    {
        base.Init();
    }

    private void Awake()
    {
        originalPosition = contents.transform.localPosition;
        canvasGroup = contents.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = contents.AddComponent<CanvasGroup>();
        }
    }

    public void StartTutorial(string[] textInContent)
    {
        StartCoroutine(ShowSequenceOfPopups(textInContent));
    }

    private IEnumerator ShowSequenceOfPopups(string[] textInContent)
    {
        // textInContent �迭�� �� �ؽ�Ʈ�� ���� �ݺ�
        for (int i = 0; i < textInContent.Length; i++)
        {
            // ���� �˾��� �ؽ�Ʈ ����
            text.text = textInContent[i];

            // �˾��� ��Ÿ���� �ڷ�ƾ ����
            yield return StartCoroutine(SmoothyPopUp(true));

            // ������ �ð���ŭ ���
            yield return new WaitForSeconds(disappearDelay);

            // �˾��� ������� �ڷ�ƾ ����
            yield return StartCoroutine(SmoothyPopUp(false));
        }

        // ��� �˾� ǥ�ð� ������ GameObject ��Ȱ��ȭ
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// �˾��� �ε巴�� ��Ÿ���ų� ������� �ϴ� �ڷ�ƾ
    /// </summary>
    /// <param name="isAppearing">true�� ��Ÿ����, false�� ������ϴ�.</param>
    private IEnumerator SmoothyPopUp(bool isAppearing)
    {
        float timer = 0f;

        if (isAppearing)
        {
            // ��Ÿ���� �ִϸ��̼�
            contents.transform.localPosition = originalPosition + new Vector3(0, startOffset, 0);
            canvasGroup.alpha = 0f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * appearSpeed;
                contents.transform.localPosition = Vector3.Lerp(originalPosition + new Vector3(0, startOffset, 0), originalPosition, timer);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer);
                yield return null;
            }
            contents.transform.localPosition = originalPosition;
            canvasGroup.alpha = 1f;
        }
        else // isAppearing == false
        {
            // ������� �ִϸ��̼�
            while (timer < 1f)
            {
                timer += Time.deltaTime * disappearSpeed;
                contents.transform.localPosition = Vector3.Lerp(originalPosition, originalPosition + new Vector3(0, startOffset, 0), timer);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer);
                yield return null;
            }
            contents.transform.localPosition = originalPosition + new Vector3(0, startOffset, 0);
            canvasGroup.alpha = 0f;
        }
    }
}