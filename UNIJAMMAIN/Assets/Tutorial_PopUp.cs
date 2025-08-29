using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tutorial_PopUp : UI_Popup
{
    public TMP_Text text;
    public string[] textInContent; // 여러 텍스트를 저장할 배열
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
        // 텍스트 배열의 모든 내용을 순차적으로 표시하는 코루틴 시작
        StartCoroutine(ShowSequenceOfPopups());
    }

    private IEnumerator ShowSequenceOfPopups()
    {
        // textInContent 배열의 각 텍스트에 대해 반복
        for (int i = 0; i < textInContent.Length; i++)
        {
            // 현재 팝업의 텍스트 설정
            text.text = textInContent[i];

            // 팝업이 나타나는 코루틴 실행
            yield return StartCoroutine(SmoothyPopUp(true));

            // 지정된 시간만큼 대기
            yield return new WaitForSeconds(disappearDelay);

            // 팝업이 사라지는 코루틴 실행
            yield return StartCoroutine(SmoothyPopUp(false));
        }

        // 모든 팝업 표시가 끝나면 GameObject 비활성화
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 팝업을 부드럽게 나타내거나 사라지게 하는 코루틴
    /// </summary>
    /// <param name="isAppearing">true면 나타나고, false면 사라집니다.</param>
    private IEnumerator SmoothyPopUp(bool isAppearing)
    {
        float timer = 0f;

        if (isAppearing)
        {
            // 나타나는 애니메이션
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
            // 사라지는 애니메이션
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