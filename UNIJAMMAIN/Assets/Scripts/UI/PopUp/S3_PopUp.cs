using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class S3_PopUp : UI_Popup
{
    public Image S3_01, S3_02;
    float duration = 2f;

    private void Start()
    {
        StartCoroutine(animationGo());

    }
    private IEnumerator animationGo()
    {
        float elapsedTime = 0f;
        S3_01.DOFade(1f, 2f);

        while (elapsedTime < 3.5f)
        {
            elapsedTime += Time.deltaTime;  // ��� �ð� ����
            yield return null;  // ���� �����ӱ��� ���
        }
        elapsedTime = 0f;
        S3_02.DOFade(1f, 2f);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;  // ��� �ð� ����
            yield return null;  // ���� �����ӱ��� ���
        }

        yield return new WaitForSeconds(3f);

        ClosePopUPUI();
    }
}
