using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class S3_PopUp : UI_Popup
{
    public Image S3_01, S3_02;
    float firstDuration = 3.5f;
    float duration = 2f;

    private void Start()
    {
        StartCoroutine(animationGo());
    }

    private IEnumerator animationGo()
    {
        S3_01.DOFade(1f, duration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(firstDuration);
        S3_02.DOFade(1f, duration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(duration+1);

        ClosePopUPUI();
    }
}
