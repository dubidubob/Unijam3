using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class S1_PopUp : UI_Popup
{
    public Image S1_01, S1_02, S1_03, Hu_01, U_02, U_03, Ung_04;
    float duration = 2f;
    float duration_text = 1.5f;
    private void Start()
    {
        StartCoroutine(animationGo());
        StartCoroutine(animationTextGo());
    }
    private IEnumerator animationGo()
    {
        S1_01.DOFade(1f, duration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(duration);
        S1_02.DOFade(1f, duration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(duration);
        S1_03.DOFade(1f, duration).SetUpdate(true);

        yield return new WaitForSecondsRealtime(3f);
        ClosePopUPUI();
    }

    private IEnumerator animationTextGo()
    {
    Hu_01.DOFade(1f, duration_text)
    .SetUpdate(true);

    yield return new WaitForSecondsRealtime(duration_text);

    U_02.DOFade(1f, duration_text).SetUpdate(true);

    yield return new WaitForSecondsRealtime(duration_text);
        
    U_03.DOFade(1f, duration_text).SetUpdate(true);
    yield return new WaitForSecondsRealtime(duration_text);
        
    Ung_04.DOFade(1f, duration_text).SetUpdate(true);
    }
}
