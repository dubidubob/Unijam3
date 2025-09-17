using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BlurController : MonoBehaviour
{
    public Image damageImage;
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
    public Image gameOverBlack;
    public TMP_Text gameOverText;
    public TMP_Text gameOverDownText;
    public float fadeDuration = 0.5f; // ��ȯ �ð� (��)

    private int currentIndex = 0;
    private Coroutine fadeCoroutine;
    public Image lastBlacking;

    public int[] hpBoundaryWeight = new int[7];

    /// <summary>
    /// ü�¿� ���� Blur ���� ������Ʈ
    /// </summary>
    public void SetBlur (float currentHp, float maxHp)
    { 

        // Debug.Log($"currentHP : {currentHp} - maxHP : {maxHp}");
        if (blurImages.Length == 0) return;

        // �� ��° Blur���� ���
        float lostHp = maxHp - currentHp;
        float cumulativeHpBoundary = 0f;
        int newIndex=0;
        for (int i = 0; i < hpBoundaryWeight.Length; i++)
        {
            cumulativeHpBoundary += hpBoundaryWeight[i];
            if (lostHp < cumulativeHpBoundary)
            {
                newIndex = i;
                break; // ���� ü�¿� �ش��ϴ� ������ ã�����Ƿ� ���� ����
            }
            // ���� ���� ü���� ��� ����ġ �պ��� ũ�ų� ���ٸ� ������ �ε����� �����˴ϴ�.
            newIndex = i;
        }
        // ������ ���� Clamp ó�� (����ġ ���� ���� ����)
        newIndex = Mathf.Clamp(newIndex, 0, blurImages.Length - 1);

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

    private void OnDestroy()
    {
        transform.DOKill();
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

        PlayRandomHurtSound();

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

    public void GameOverBlurEffect()
    {
        // InCirc õõ�� ��ο����ٰ� ���ڱ� ��ο�����
        gameOverBlack.DOFade(1 / 255f * 248f, 1f)
            .SetEase(Ease.InCirc)
            .SetUpdate(UpdateType.Normal, true);
        gameOverText.DOFade(1 / 255f * 248f, 1f)
            .SetEase(Ease.InCirc)
            .SetUpdate(UpdateType.Normal, true);
        gameOverDownText.DOFade(1 / 255f * 248f, 1f )
            .SetEase(Ease.InCirc)
            .SetUpdate(UpdateType.Normal, true);
        
        if(Managers.Game.currentPlayerState==GameManager.PlayerState.Die)
        {
            StartCoroutine(WaitForGameOver());
        }
        
    }

    private IEnumerator WaitForGameOver()
    {
        // �ִϸ��̼��� ���� ������ ��ٸ��ϴ�.
        yield return new WaitForSecondsRealtime(1.0f);

        
        // ���� ���°� '���'�� ���� Ŭ�� �̺�Ʈ�� ó���մϴ�.
        if (Managers.Game.currentPlayerState == GameManager.PlayerState.Die)
        {
            // ȭ�鿡 Ŭ�� ������ EventTrigger ������Ʈ�� �߰��մϴ�.
            var eventTrigger = gameOverBlack.gameObject.GetOrAddComponent<EventTrigger>();

            // Ŭ�� �̺�Ʈ�� �����մϴ�.
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) =>
            {
                // Ŭ�� �� �������� ������ �̵�
                SceneManager.LoadScene("StageScene");
                Time.timeScale = 1f; // Ÿ�ӽ����� ���� ����
            });

            // EventTrigger�� �̺�Ʈ�� �߰��մϴ�.
            eventTrigger.triggers.Add(entry);
            Debug.Log("����");
        }


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

    #region tool

    private void PlayRandomHurtSound()
    {
        // 0 �Ǵ� 1�� �������� ����
        int randomIndex = Random.Range(0, 2);

        if (randomIndex == 0)
        {
            Managers.Sound.Play("SFX/Damaged/Hurt1_V1");
        }
        else
        {
            Managers.Sound.Play("SFX/Damaged/Hurt2_V1");
        }
    }
    #endregion
}
