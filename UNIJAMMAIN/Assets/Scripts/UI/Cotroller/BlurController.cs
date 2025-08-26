using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BlurController : MonoBehaviour
{
    public Image damageImage;
    int beforeHealth=1; // ������ ü�� ���� ó������ ���� ȯ�Ѱ� Default������ . ��ȭ�� �̰� ��ȭ���������
    bool isCoolDown;
    public Camera camera; // ��鸱 ī�޶� Transform
    public float shakeStrength = 0.2f; // ��鸲 ����
    public float shakeDuration = 0.2f; // ��鸲 ���� �ð�

    private void Start()
    {
        if (camera == null)
        { 
            camera = Camera.main;
        }
        damageImage.color = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0); // �ʱ� ���� 0
        /*
        Debug.Log("�׽�Ʈ�� pitch1.3f");
        Time.timeScale = 1.3f;
        Managers.Sound.Play("BGM/84bpm_64_V1", Define.Sound.BGM, 1.3f);
        */
        Managers.Game.blur = this;
    }

    public Image[] blurImages; // Blur1 ~ BlurN (Inspector�� �־���)
    public Image goGrayBackGround;
    public float fadeDuration = 0.5f; // ��ȯ �ð� (��)

    private int currentIndex = 0;
    private Coroutine fadeCoroutine;
    public Image lastBlacking;

    /// <summary>
    /// ü�¿� ���� Blur ���� ������Ʈ
    /// </summary>
    public void SetBlur(float currentHp, float maxHp)
    { 

        // Debug.Log($"currentHP : {currentHp} - maxHP : {maxHp}");
        if (blurImages.Length == 0) return;

        // �� ��° Blur���� ���
        float ratio = 1f - (currentHp / maxHp); // hp�� �ټ��� ratio ��
        int newIndex = Mathf.Clamp(Mathf.FloorToInt(ratio * blurImages.Length), 0, blurImages.Length - 1);

        if (newIndex != currentIndex) // ���ο� �̹������� ��ȯ 
        {
            // ��ȯ ����
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeTransition(currentIndex, newIndex));
            currentIndex = newIndex;

            if (newIndex >= blurImages.Length - 2) // ������ 2�ܰ���� ���� �߰��Ǵ� Layer BackGround.LasBlacking
            {
                lastBlacking.DOFade(1f, 0.8f); // ��ο�����
            }
            else
            {
                lastBlacking.DOFade(0f, 0.8f); // �������
            }

        }
        
        
    }

    private IEnumerator FadeTransition(int oldIndex, int newIndex)
    {
        float time = 0f;

        Image oldImg = blurImages[oldIndex];
        Image newImg = blurImages[newIndex];

        // ���� ���İ�
        float oldStartAlpha = oldImg.color.a;
        float newStartAlpha = newImg.color.a;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            // old �� ����
            Color c1 = oldImg.color;
            c1.a = Mathf.Lerp(oldStartAlpha, 0f, t);
            oldImg.color = c1;

            // new �� ������
            Color c2 = newImg.color;
            c2.a = Mathf.Lerp(newStartAlpha, 1f, t);
            newImg.color = c2;

            yield return null;
        }

        // ���� ����
        Color endOld = oldImg.color;
        endOld.a = 0f;
        oldImg.color = endOld;

        Color endNew = newImg.color;
        endNew.a = 1f;
        newImg.color = endNew;
    }


    public void ShowDamageEffect()
    {
        if (isCoolDown) return;

        isCoolDown = true;
        damageImage.DOKill();
        camera.transform.DOKill(); // ���� ��鸲 �ߴ�

        // ���� ȿ�� UI ���̵� 
        Sequence seq = DOTween.Sequence();
        seq.Append(damageImage.DOFade(1f, 0.15f));
        seq.Append(damageImage.DOFade(0f, 0.15f));

        // blurImages ���� �������� goGrayBackGround alpha ���
        // currentIndex�� SetBlur���� ���ŵ�, 0 ~ blurImages.Length-1
        float targetAlpha = (currentIndex + 1) / (float)blurImages.Length; // 1/6, 2/6, ... ����

        // ��� goGrayBackGround ���� ���� ���������� �߰�
        seq.Join(goGrayBackGround.DOFade(targetAlpha, 0.3f)); // damageImage�� ���ÿ� ���̵�

        // ī�޶� ��鸲 ȿ�� �߰�
        camera.transform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: 8, // ��鸮�� Ƚ��
            randomness: 90,
            snapping: false,
            fadeOut: true
        );

        seq.OnComplete(() => isCoolDown = false);
    }


    public void ComboEffect()
    {
        float defaultSize = camera.orthographicSize;

        // ����
        camera.DOOrthoSize(defaultSize * 0.9f, 0.4f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
            // ���� ũ��� ����
            camera.DOOrthoSize(defaultSize, 0.4f)
                    .SetEase(Ease.InOutQuad);
            });
    }
}
