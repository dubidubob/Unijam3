using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BlurController : MonoBehaviour
{
    public Image damageImage;
    int beforeHealth=1; // 기존의 체력 상태 처음에는 가장 환한게 Default값으로 . 변화시 이거 변화시켜줘야함
    bool isCoolDown;
    public Camera camera; // 흔들릴 카메라 Transform
    public float shakeStrength = 0.2f; // 흔들림 강도
    public float shakeDuration = 0.2f; // 흔들림 지속 시간

    private void Start()
    {
        if (camera == null)
        { 
            camera = Camera.main;
        }
        damageImage.color = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0); // 초기 알파 0
        /*
        Debug.Log("테스트용 pitch1.3f");
        Time.timeScale = 1.3f;
        Managers.Sound.Play("BGM/84bpm_64_V1", Define.Sound.BGM, 1.3f);
        */
        Managers.Game.blur = this;
    }

    public Image[] blurImages; // Blur1 ~ BlurN (Inspector에 넣어줌)
    public Image goGrayBackGround;
    public float fadeDuration = 0.5f; // 전환 시간 (초)

    private int currentIndex = 0;
    private Coroutine fadeCoroutine;
    public Image lastBlacking;

    /// <summary>
    /// 체력에 따라 Blur 상태 업데이트
    /// </summary>
    public void SetBlur(float currentHp, float maxHp)
    { 

        // Debug.Log($"currentHP : {currentHp} - maxHP : {maxHp}");
        if (blurImages.Length == 0) return;

        // 몇 번째 Blur인지 계산
        float ratio = 1f - (currentHp / maxHp); // hp가 줄수록 ratio ↑
        int newIndex = Mathf.Clamp(Mathf.FloorToInt(ratio * blurImages.Length), 0, blurImages.Length - 1);

        if (newIndex != currentIndex) // 새로운 이미지로의 변환 
        {
            // 전환 시작
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeTransition(currentIndex, newIndex));
            currentIndex = newIndex;

            if (newIndex >= blurImages.Length - 2) // 마지막 2단계부터 새로 추가되는 Layer BackGround.LasBlacking
            {
                lastBlacking.DOFade(1f, 0.8f); // 어두워지기
            }
            else
            {
                lastBlacking.DOFade(0f, 0.8f); // 밝아지기
            }

        }
        
        
    }

    private IEnumerator FadeTransition(int oldIndex, int newIndex)
    {
        float time = 0f;

        Image oldImg = blurImages[oldIndex];
        Image newImg = blurImages[newIndex];

        // 시작 알파값
        float oldStartAlpha = oldImg.color.a;
        float newStartAlpha = newImg.color.a;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            // old → 투명
            Color c1 = oldImg.color;
            c1.a = Mathf.Lerp(oldStartAlpha, 0f, t);
            oldImg.color = c1;

            // new → 불투명
            Color c2 = newImg.color;
            c2.a = Mathf.Lerp(newStartAlpha, 1f, t);
            newImg.color = c2;

            yield return null;
        }

        // 최종 보정
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
        camera.transform.DOKill(); // 이전 흔들림 중단

        // 피해 효과 UI 페이드 
        Sequence seq = DOTween.Sequence();
        seq.Append(damageImage.DOFade(1f, 0.15f));
        seq.Append(damageImage.DOFade(0f, 0.15f));

        // blurImages 개수 기준으로 goGrayBackGround alpha 계산
        // currentIndex는 SetBlur에서 갱신됨, 0 ~ blurImages.Length-1
        float targetAlpha = (currentIndex + 1) / (float)blurImages.Length; // 1/6, 2/6, ... 비율

        // 배경 goGrayBackGround 투명도 점점 진해지도록 추가
        seq.Join(goGrayBackGround.DOFade(targetAlpha, 0.3f)); // damageImage와 동시에 페이드

        // 카메라 흔들림 효과 추가
        camera.transform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: 8, // 흔들리는 횟수
            randomness: 90,
            snapping: false,
            fadeOut: true
        );

        seq.OnComplete(() => isCoolDown = false);
    }


    public void ComboEffect()
    {
        float defaultSize = camera.orthographicSize;

        // 줌인
        camera.DOOrthoSize(defaultSize * 0.9f, 0.4f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
            // 원래 크기로 복귀
            camera.DOOrthoSize(defaultSize, 0.4f)
                    .SetEase(Ease.InOutQuad);
            });
    }
}
