using DG.Tweening;
using UnityEngine;

public class BeatClockUI : MonoBehaviour
{
    private Sequence seq;

    private Vector3 baseScale;
    private float beatDuration;

    private void Awake()
    {
        baseScale = transform.localScale;
        IngameData.ChangeBpm += Init;
    }

    private void OnDestroy()
    {
        IngameData.ChangeBpm -= Init;
        BeatClock.OnBeat -= BeatMoving;

        seq?.Kill();
        transform.DOKill();
    }

    private void Init()
    {
        seq?.Kill();
        transform.DOKill();

        beatDuration = (float)IngameData.BeatInterval;

        seq = DOTween.Sequence()
           .SetAutoKill(false)
           .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
           .Pause();

        BeatClock.OnBeat -= BeatMoving;
        BeatClock.OnBeat += BeatMoving;
    }

    private void BeatMoving(double _, long __)
    {

        // 현재 크기 -> 1.2배 커지기 (전체 비트의 50% 동안)
        transform.DOScale(baseScale * 1.2f, 0.001f).SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // 애니메이션이 끝난 후 원래 크기로 돌아오는 애니메이션을 실행합니다.
                transform.DOScale(baseScale, beatDuration * 0.5f).SetEase(Ease.InOutQuad)
                    .SetTarget(this.gameObject);
            })
            .SetTarget(this.gameObject);
    }
}
