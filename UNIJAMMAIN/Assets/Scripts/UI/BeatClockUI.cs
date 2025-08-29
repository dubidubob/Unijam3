using DG.Tweening;
using UnityEngine;

public class BeatClockUI : MonoBehaviour
{
    private Vector3 baseScale;
    private float beatDuration;
    private Sequence seq;
    private void Awake()
    {
        IngameData.ChangeBpm -= Init;
        IngameData.ChangeBpm += Init;
    }

    private void OnDestroy()
    {
        IngameData.ChangeBpm -= Init;
        BeatClock.OnBeat -= BeatMoving;
        seq?.Kill();
        DOTween.Kill(transform);
    }

    private void Init()
    {
        baseScale = transform.localScale;
        beatDuration = (float)IngameData.BeatInterval;
        seq?.Kill();
        DOTween.Kill(transform);

        seq = DOTween.Sequence()
           .SetAutoKill(false)
           .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        seq.AppendCallback(() => transform.localScale = baseScale * 1.2f)
                   .Append(transform.DOScale(baseScale, beatDuration * 0.5f)
                       .SetEase(Ease.InOutQuad))
                   .Pause();

        BeatClock.OnBeat -= BeatMoving;
        BeatClock.OnBeat += BeatMoving;
    }
    private void BeatMoving(double _, long __)
    {
        // 현재 크기 -> 1.2배 커지기 (전체 비트의 50% 동안)
        if (seq == null) return;
        seq.Restart();
    }
}
