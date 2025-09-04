using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Tutorial_PopUp : UI_Popup
{
    public TMP_Text text;
    public GameObject contents;

    public float appearSpeed = 2f;
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
        Managers.UI.SetCanvas(this.gameObject);
    }

    public void StartTutorial(IReadOnlyList<TextInfo> textInfo, int? lastMonsterHitCnt)
    {
        StartCoroutine(ShowSequenceOfPopups(textInfo, lastMonsterHitCnt));
    }

    private IEnumerator ShowSequenceOfPopups(IReadOnlyList<TextInfo> textInfo, int? lastMonsterHitCnt=0)
    {
        // textInContent 배열의 각 텍스트에 대해 반복
        for (int i = 0; i < textInfo.Count; i++)
        {
            var textInContent = textInfo[i].textContents;            

            float durationSec = (float)IngameData.BeatInterval * (textInfo[i].delayBeat-1);
            appearSpeed = (float)IngameData.BeatInterval * 0.5f;
            disappearSpeed = (float)IngameData.BeatInterval * 0.5f;

            // 현재 팝업의 텍스트 설정
            int curMonsterHitCnt = IngameData.PerfectMobCnt + IngameData.GoodMobCnt;
            bool isFail = (curMonsterHitCnt - lastMonsterHitCnt) < textInfo[i].monsterCutline;
            text.text = isFail ? textInfo[i].textContents.Last() : textInfo[i].textContents.First();
            
            // 팝업 나타났다 사라짐
            yield return StartCoroutine(SmoothyPopUp(true));
            yield return new WaitForSeconds(durationSec);
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
        if (isAppearing)
        {
            float timer = 0f;
            // 나타나는 애니메이션
            contents.transform.localPosition = originalPosition + new Vector3(0, startOffset, 0);
            canvasGroup.alpha = 0f;

            while (timer < 1f)
            {
                timer += Time.deltaTime / appearSpeed;
                float k = Mathf.Clamp01(timer);
                contents.transform.localPosition = Vector3.Lerp(originalPosition + new Vector3(0, startOffset, 0), originalPosition, timer);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, k);
                yield return null;
            }
            contents.transform.localPosition = originalPosition;
            canvasGroup.alpha = 1f;
        }
        else // isAppearing == false
        {
            // 사라지는 애니메이션
            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime / disappearSpeed;
                float k = Mathf.Clamp01(timer);
                contents.transform.localPosition = Vector3.Lerp(originalPosition, originalPosition + new Vector3(0, startOffset, 0), timer);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, k);
                yield return null;
            }
            contents.transform.localPosition = originalPosition + new Vector3(0, startOffset, 0);
            canvasGroup.alpha = 0f;
        }
    }
}