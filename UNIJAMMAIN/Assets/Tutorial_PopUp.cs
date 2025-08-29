using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tutorial_PopUp : UI_Popup
{
    public TMP_Text text;
    public string[] textInContent; // ���� �ؽ�Ʈ�� ������ �迭
    public GameObject contents;

    public float appearSpeed = 2f;
    public float disappearDelay = 3f;
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

    private void Start()
    {
        // �ؽ�Ʈ �迭�� ��� ������ ���������� ǥ���ϴ� �ڷ�ƾ ����
        StartCoroutine(ShowSequenceOfPopups());
    }

    private IEnumerator ShowSequenceOfPopups()
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