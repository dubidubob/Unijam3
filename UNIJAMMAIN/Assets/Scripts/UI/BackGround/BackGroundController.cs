using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks; // UniTask 네임스페이스 추가
using System.Threading; // CancellationToken 사용

public class BackGroundController : MonoBehaviour
{
    private Sequence seq;
    private readonly string satPropName = "_Saturation";

    public Material sharedMaterial;
    private float beatDuration;


    [SerializeField] Image extraObjectImage;
    private void OnDestroy()
    {
        IngameData.ChangeBpm -= Init;
        BeatClock.OnBeat -= BeatOnBackGroundAction;
        seq?.Kill();
        sharedMaterial.SetFloat(satPropName, 1f);
    }
    private void Awake()
    {
        IngameData.ChangeBpm -= Init;
        IngameData.ChangeBpm += Init;
    }
    private void Init()
    {
        beatDuration = (float)IngameData.BeatInterval;
        seq?.Kill();
        seq = DOTween.Sequence()
           .SetAutoKill(false)
           .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
           .Pause();

        BeatClock.OnBeat -= BeatOnBackGroundAction;
        BeatClock.OnBeat += BeatOnBackGroundAction;
    }

    private int actionNumberTarget = 4;
    private int nowactionNumber=1;
    private void BeatOnBackGroundAction(long _)
    {
        if (extraObjectImage != null)
        {
            if(nowactionNumber==actionNumberTarget)
            {
                nowactionNumber = 1;
            }
            else
            {
                nowactionNumber++;
                return;
            }

            RectTransform rect = extraObjectImage.rectTransform;

            // 1. 이 오브젝트에 걸린 모든 트윈을 확실하게 제거
            // rect(컴포넌트)가 아니라 gameObject를 기준으로 킬을 해야 안전합니다.
            rect.DOKill();

            // -------------------------------------------------------
            // [설정]
            // startMargin: 0 (꽉 찬 상태)
            // shrinkMargin: 100 (안쪽으로 쪼그라든 상태)
            // -------------------------------------------------------
            float startMargin = -80f;
            float shrinkMargin = 0f;

            // 2. 시작 상태 강제 초기화 (0에서 시작)
            UpdateRectMargin(rect, startMargin);

            // 3. 애니메이션 실행
            // BeatClockUI처럼 "팍!(Kick)" 하고 커졌다가 "스르륵(Rest)" 돌아오는 구조입니다.

            // [단계 A] Kick: 0 -> 100 (아주 빠르게! 0.05초)
            // 타겟을 rect로 명시(.SetTarget)해야 DOKill이 먹힙니다.
            DOVirtual.Float(startMargin, shrinkMargin, 0.05f, (x) =>
            {
                UpdateRectMargin(rect, x);
            })
            .SetTarget(rect) // [중요] 타겟을 설정해야 DOKill로 멈출 수 있음
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // [단계 B] Rest: 100 -> 0 (나머지 시간 동안 천천히 복구)
                DOVirtual.Float(shrinkMargin, startMargin, beatDuration * 0.9f, (x) =>
                {
                    UpdateRectMargin(rect, x);
                })
                .SetTarget(rect) // [중요] 여기도 타겟 설정
                .SetEase(Ease.OutQuad);
            });
        }
    }

    // [헬퍼 함수] 상하좌우 여백(Margin)을 한 번에 조절하는 함수
    // margin이 0이면 stretch 꽉 채움, 50이면 사방에서 50씩 들어옴
    private void UpdateRectMargin(RectTransform rect, float margin)
    {
        // Left, Bottom은 정수값 그대로
        rect.offsetMin = new Vector2(margin, margin);
        // Right, Top은 음수값이어야 안쪽으로 들어옴
        rect.offsetMax = new Vector2(-margin, -margin);
    }

    public void SettingSatuation(float value)
    {
        if (sharedMaterial != null)
        {
            sharedMaterial.SetFloat(satPropName, value);
        }
    }


    private void OnApplicationQuit()
    {
        if (sharedMaterial != null)
        {
            sharedMaterial.SetFloat(satPropName, 1f); // 1.0(원본)으로 복구
        }
    }

}

