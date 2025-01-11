using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class S2_PopUp : UI_Popup
{
    public Image S2_01, S2_02;
    float duration = 2f;

    private void Start()
    {
        StartCoroutine(animationGo());
       
    }
    private IEnumerator animationGo()
    {
        float elapsedTime = 0f;
        S2_01.DOFade(1f, 2f);

        while (elapsedTime < 3.5f)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        elapsedTime = 0f;
        S2_02.DOFade(1f, 2f);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }

        yield return new WaitForSeconds(3f);

        ClosePopUPUI();
    }
}
