using Cysharp.Threading.Tasks; // UniTask 네임스페이스 추가
using DG.Tweening;
using UnityEngine;

public class KeyChangeEffect : MonoBehaviour
{
    [SerializeField] private Transform targetPos; // Inspector에서 지정

    private Tween _motionTween;
    private float _goingUpOffset = 0.5f;
    private Vector3 targetPosition;
    private Vector3 originPosition;

    private void Start()
    {
        targetPosition = targetPos.position;
        originPosition = transform.position;

        PhaseController.ChangeKey -= ChangeMotion;
        PhaseController.ChangeKey += ChangeMotion;
    }

    private void OnDestroy()
    {
        PhaseController.ChangeKey -= ChangeMotion;
        transform.DOKill();
    }

    private bool isChangeActionOn = false;
    private void ChangeMotion(float startDelay)
    {
        _motionTween?.Kill();

        if (startDelay < 0)
        {
            if (isChangeActionOn)
            {
                // -1 등의 음수 값이 들어올 경우, 절댓값(양수)으로 변환하여 복귀 애니메이션 실행
                ReturnToOriginAsync(Mathf.Abs(startDelay)).Forget();
            }
            return;
        }

        MoveToTargetAsync(startDelay).Forget();
    }

    // --- 기존: 타겟으로 이동하는 애니메이션 ---
    private async UniTaskVoid MoveToTargetAsync(float duration)
    {
        
        Sequence seq = DOTween.Sequence();

        // 살짝 떠오르기
        seq.Append(transform.DOMoveY(transform.position.y + _goingUpOffset, duration * 0.1f).SetEase(Ease.OutQuad));
        seq.AppendInterval(duration * 0.1f);

        // 타겟으로 슉 이동
        seq.Append(transform.DOMove(AdjustTargetPosY(), duration * 0.2f).SetEase(Ease.InOutQuad));
        seq.AppendInterval(duration * 0.1f);

        // 타겟 위치로 착지
        seq.Append(transform.DOMoveY(targetPosition.y, duration * 0.1f).SetEase(Ease.InQuad));

        _motionTween = seq;

        isChangeActionOn = true;
        // UniTask를 활용해 애니메이션 종료까지 대기 (필요 시 이 아래에 후처리 로직 추가 가능)
        await seq.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())
                 .SuppressCancellationThrow(); // 취소 시 발생하는 에러 로그를 숨겨줍니다.
    }

    // --- 신규: 원래 위치로 되돌아가는 애니메이션 ---
    private async UniTaskVoid ReturnToOriginAsync(float duration)
    {
        Sequence seq = DOTween.Sequence();

        // 1. 현재 위치에서 살짝 떠오르기 (Y축 기준)
        seq.Append(transform.DOMoveY(transform.position.y + _goingUpOffset, duration * 0.1f).SetEase(Ease.OutQuad));
        seq.AppendInterval(duration * 0.1f);

        // 2. 원래 위치(origin)의 상단으로 슉 이동
        Vector3 elevatedOrigin = new Vector3(originPosition.x, originPosition.y + _goingUpOffset, originPosition.z);
        seq.Append(transform.DOMove(elevatedOrigin, duration * 0.2f).SetEase(Ease.InOutQuad));
        seq.AppendInterval(duration * 0.1f);

        // 3. 원래 위치로 안전하게 착지
        seq.Append(transform.DOMoveY(originPosition.y, duration * 0.1f).SetEase(Ease.InQuad));

        _motionTween = seq;

        isChangeActionOn = false;
        // UniTask 대기 (복귀가 완료된 후 실행할 이벤트가 있다면 여기서 처리)
        await seq.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())
                 .SuppressCancellationThrow();
    }

    private Vector3 AdjustTargetPosY()
    {
        return new Vector3(targetPosition.x, targetPosition.y + _goingUpOffset, targetPosition.z);
    }
}