using DG.Tweening;
using UnityEngine;

public class DotweenTest : MonoBehaviour
{
    void Start()
    {
        DoTweenScaleUp(this.transform);
    }

    public void DoTweenScaleUp(Transform transform, float upScaleAmount = 1.2f)
    {
        transform.DOScale(Vector3.one * upScaleAmount, 0.2f)
            .OnComplete(() => transform.DOScale(Vector3.one, 0.2f));
    }
}
