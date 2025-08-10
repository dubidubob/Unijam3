using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BlurController : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    public Image damageImage;
    int beforeHealth=1; // 기존의 체력 상태 처음에는 가장 환한게 Default값으로 . 변화시 이거 변화시켜줘야함
    bool isCoolDown;
      public Transform cameraTransform; // 흔들릴 카메라 Transform
    public float shakeStrength = 0.2f; // 흔들림 강도
    public float shakeDuration = 0.2f; // 흔들림 지속 시간

    private void Start()
    {
        damageImage.color = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0); // 초기 알파 0
    }
    public void SetBlur(float healthValue) // 체력값을 비교하여 블러 변화
    {
        ShowDamageEffect();
        if (healthValue > 0.7) // BeforeHealth 값 1상태
        {
            if(beforeHealth==2) // 밝아지기
            {
                Change1_2(true);
            }

            beforeHealth = 1;
        }
        else if (healthValue <= 0.7 || healthValue > 0.4)
        {
            if (beforeHealth == 1) 
            {
                Change1_2(); // 어두워지기
            }
            else if(beforeHealth ==3)
            {
                Change2_3(true); // 밝아지기
            }


            beforeHealth = 2;
        }
        else if (healthValue <= 0.4 || healthValue > 0.1)
        {
            if (beforeHealth == 2)
            {
                Change2_3(); // 어두워지기
            }
            else if (beforeHealth == 4)
            {
                Change3_4(true); // 밝아지기
            }


            beforeHealth = 3;
        }
        else
        {
            if (beforeHealth == 3)
            {
                Change3_4(); // 어두워지기
            }

            beforeHealth = 4;
        }
    }

    private void Change1_2(bool reverse = false)
    {
        if (reverse)
        {
            // 2->1로 이미지 변환
            images[1].DOFade(0f, 0.5f);
        }
        else
        {
            // 1->2로 이미지 변환
            images[1].DOFade(1f, 0.5f);
        }       
    }
    private void Change2_3(bool reverse = false)
    {
        if (reverse)
        {
            // 3->2로 이미지 변환

            // 이미지가 짧은시간에 이미지 3이 투명처리됨
            images[2].DOFade(0f, 0.5f);
        }
        else
        {
            //2->3로 이미지 변환

            // 이미지가 짧은시간에 이미지 3이 생김
            images[2].DOFade(1f, 0.5f);
        }
    }

    private void Change3_4(bool reverse = false)
    {
        if (reverse)
        {
            // 4->3로 이미지 변환
            images[3].DOFade(0f, 0.5f);
        }
        else
        {
            // 3->4로 이미지 변환
            images[3].DOFade(1f, 0.5f);
        }
    }

    public void ShowDamageEffect()
    {
        if (isCoolDown) return;

        isCoolDown = true;
        damageImage.DOKill();
        cameraTransform.DOKill(); // 이전 흔들림 중단

        // 피해 효과 UI 페이드
        Sequence seq = DOTween.Sequence();
        seq.Append(damageImage.DOFade(0.8f, 0.1f));
        seq.Append(damageImage.DOFade(0f, 0.3f));

        // 카메라 흔들림 효과 추가
        cameraTransform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: 8, // 흔들리는 횟수
            randomness: 90,
            snapping: false,
            fadeOut: true
        );

        seq.OnComplete(() => isCoolDown = false);
    }

}
