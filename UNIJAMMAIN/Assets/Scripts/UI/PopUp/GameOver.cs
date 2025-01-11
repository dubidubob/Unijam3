using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOver : UI_Popup
{
    public Image blackPanel;
    public TMP_Text text;
    private float duration = 4f;  // �������� �ɸ��� �ð�
    private void Start()
    {
        Init();
        StartCoroutine(GoingBlack());
        
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
        while (elapsedTime < duration)
        {
            text.color = new Color(255, 255, 255, Mathf.Lerp(1, 0f, elapsedTime / duration));
            elapsedTime += Time.deltaTime;  // ��� �ð� ����
            yield return null;  // ���� �����ӱ��� ���
        }

        // �������� ��Ȯ�� 1�� ����
        text.color = new Color(255, 255, 255, 1);
        blackPanel.color = new Color(0, 0, 0, 1);
    }
}

