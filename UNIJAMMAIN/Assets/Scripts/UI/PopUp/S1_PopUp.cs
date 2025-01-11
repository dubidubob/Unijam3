using System.Collections;
using System.Collections.Generic;
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
        float elapsedTime = 0f;
        S1_01.DOFade(1f, 0.5f);

        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        elapsedTime = 0f;
        S1_02.DOFade(1f, 0.5f);
        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        elapsedTime = 0f;
        S1_03.DOFade(1f, 0.5f);



        yield return new WaitForSeconds(2f);
        ClosePopUPUI();
    }
        private IEnumerator animationTextGo()
        {
        float elapsedTime = 0;
        
            Hu_01.DOFade(1f, 0.5f);

             while (elapsedTime < 0.5f)
             {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        elapsedTime = 0f;
        U_02.DOFade(1f, 0.5f);
        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        elapsedTime = 0f;
        U_03.DOFade(1f, 0.5f);
        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        elapsedTime = 0f;
        Ung_04.DOFade(1f, 2f);

        }
}
