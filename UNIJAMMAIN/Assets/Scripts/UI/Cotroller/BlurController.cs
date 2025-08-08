using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BlurController : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    int beforeHealth=1; // ������ ü�� ���� ó������ ���� ȯ�Ѱ� Default������ . ��ȭ�� �̰� ��ȭ���������

    public void SetBlur(float healthValue) // ü�°��� ���Ͽ� �� ��ȭ
    {
        if (healthValue > 0.7) // BeforeHealth �� 1����
        {
            if(beforeHealth==2) // �������
            {
                Change1_2(true);
            }

            beforeHealth = 1;
        }
        else if (healthValue <= 0.7 || healthValue > 0.4)
        {
            if (beforeHealth == 1) 
            {
                Change1_2(); // ��ο�����
            }
            else if(beforeHealth ==3)
            {
                Change2_3(true); // �������
            }


            beforeHealth = 2;
        }
        else if (healthValue <= 0.4 || healthValue > 0.1)
        {
            if (beforeHealth == 2)
            {
                Change2_3(); // ��ο�����
            }
            else if (beforeHealth == 4)
            {
                Change3_4(true); // �������
            }


            beforeHealth = 3;
        }
        else
        {
            if (beforeHealth == 3)
            {
                Change3_4(); // ��ο�����
            }

            beforeHealth = 4;
        }
    }

    private void Change1_2(bool reverse = false)
    {
        if (reverse)
        {
            // 2->1�� �̹��� ��ȯ
            images[1].DOFade(0f, 0.5f);
        }
        else
        {
            // 1->2�� �̹��� ��ȯ
            images[1].DOFade(1f, 0.5f);
        }       
    }
    private void Change2_3(bool reverse = false)
    {
        if (reverse)
        {
            // 3->2�� �̹��� ��ȯ

            // �̹����� ª���ð��� �̹��� 3�� ����ó����
            images[2].DOFade(0f, 0.5f);
        }
        else
        {
            //2->3�� �̹��� ��ȯ

            // �̹����� ª���ð��� �̹��� 3�� ����
            images[2].DOFade(1f, 0.5f);
        }
    }

    private void Change3_4(bool reverse = false)
    {
        if (reverse)
        {
            // 4->3�� �̹��� ��ȯ
            images[3].DOFade(0f, 0.5f);
        }
        else
        {
            // 3->4�� �̹��� ��ȯ
            images[3].DOFade(1f, 0.5f);
        }
    }
}
