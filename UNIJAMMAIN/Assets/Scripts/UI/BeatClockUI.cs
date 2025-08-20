using DG.Tweening;
using UnityEngine;

public class BeatClockUI : MonoBehaviour
{
    private Vector3 baseScale;
    private float beatDuration;

    private void Awake()
    {
        baseScale = transform.localScale;
        beatDuration = (float)IngameData.BeatInterval;

        BeatClock.OnBeat -= BeatMoving;
        BeatClock.OnBeat += BeatMoving;

    }

    private void BeatMoving(double t)
    {
        Sequence seq = DOTween.Sequence();

        // 현재 크기 -> 1.2배 커지기 (전체 비트의 50% 동안)

        seq.Append(transform.DOScale(baseScale * 1.2f, 0.001f)
            .SetEase(Ease.OutCubic));

        // 다시 원래 크기로 돌아오기 (전체 비트의 50% 동안)
        seq.Append(transform.DOScale(baseScale, beatDuration * 0.5f)
            .SetEase(Ease.InOutQuad));

        Debug.Log($"{t}");
    }
}
