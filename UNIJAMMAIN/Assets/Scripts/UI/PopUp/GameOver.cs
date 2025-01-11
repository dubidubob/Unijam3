using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
public class GameOver : UI_Popup
{
    public Image blackPanel,goodSleepMan,WhyAmI;
    public TMP_Text text;
    private float duration = 4f;  // �������� �ɸ��� �ð�
    enum Buttons
    {
        Why_Am_I_Here
    }
   
    private void Start()
    {
        Init();
        StartCoroutine(GoingBlack());
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.Why_Am_I_Here).gameObject.AddUIEvent(gohome);
        
    }
    void gohome(PointerEventData eventData)
    {
        Managers.Scene.LoadScene(Define.Scene.MainTitle);
    }
    public override void Init()
    {
        base.Init();
    }

    private IEnumerator GoingBlack()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Debug.Log("�������?");
            // alpha ���� 0���� 1�� ���������� ���ϵ��� ����
            blackPanel.color = new Color(0, 0, 0, Mathf.Lerp(0, 1f, elapsedTime / duration));
            elapsedTime += Time.deltaTime;  // ��� �ð� ����
            yield return null;  // ���� �����ӱ��� ���
        }
        elapsedTime = 0f;
        while (elapsedTime < duration)
            {
                text.color = new Color(1, 1, 1, Mathf.Lerp(0, 1f, elapsedTime / duration));
                elapsedTime += Time.deltaTime;  // ��� �ð� ����
                yield return null;  // ���� �����ӱ��� ���
          }
        elapsedTime = 0f;
        text.DOFade(0f, 4f);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;  // ��� �ð� ����
            yield return null;  // ���� �����ӱ��� ���
        }
        goodSleepMan.DOFade(1f, 2f);
        elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;  // ��� �ð� ����
            yield return null;  // ���� �����ӱ��� ���
        }
        WhyAmI.DOFade(1f, 4f);
    }
}

