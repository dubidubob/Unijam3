using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BlurController : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    public Image damageImage;
    int beforeHealth=1; // ������ ü�� ���� ó������ ���� ȯ�Ѱ� Default������ . ��ȭ�� �̰� ��ȭ���������
    bool isCoolDown;
      public Transform cameraTransform; // ��鸱 ī�޶� Transform
    public float shakeStrength = 0.2f; // ��鸲 ����
    public float shakeDuration = 0.2f; // ��鸲 ���� �ð�

    private void Start()
    {
        damageImage.color = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0); // �ʱ� ���� 0
    }
    public void SetBlur(float healthValue) // ü�°��� ���Ͽ� �� ��ȭ
    {
        ShowDamageEffect();
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

    public void ShowDamageEffect()
    {
        if (isCoolDown) return;

        isCoolDown = true;
        damageImage.DOKill();
        cameraTransform.DOKill(); // ���� ��鸲 �ߴ�

        // ���� ȿ�� UI ���̵�
        Sequence seq = DOTween.Sequence();
        seq.Append(damageImage.DOFade(0.8f, 0.1f));
        seq.Append(damageImage.DOFade(0f, 0.3f));

        // ī�޶� ��鸲 ȿ�� �߰�
        cameraTransform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: 8, // ��鸮�� Ƚ��
            randomness: 90,
            snapping: false,
            fadeOut: true
        );

        seq.OnComplete(() => isCoolDown = false);
    }

}
