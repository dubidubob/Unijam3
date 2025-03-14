using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class S2_PopUp : UI_Popup
{
    public Image S2_01, S2_02;
    float duration = 1.7f;

    private void Start()
    {
        StartCoroutine(animationGo());       
    }
    private IEnumerator animationGo()
    {
        S2_01.DOFade(1f, duration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(3f);
        S2_02.DOFade(1f, duration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(3.2f);

        ClosePopUPUI();
    }
}
