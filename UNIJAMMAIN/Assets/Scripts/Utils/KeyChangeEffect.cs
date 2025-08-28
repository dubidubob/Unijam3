using DG.Tweening;
using UnityEngine;

public class KeyChangeEffect : MonoBehaviour
{
    [SerializeField] private Transform targetPos; // Inspector���� ����

    private Tween _motionTween;
    private float _goingUpOffset = 0.5f;
    private Vector3 targetPosition;
    private Vector3 originPosition;

    private void Start()
    {
        targetPosition = targetPos.position;
        originPosition = transform.position;

        PhaseManager.ChangeKey -= ChangeMotion;
        PhaseManager.ChangeKey += ChangeMotion;
    }
    private void OnDestroy()
    {
        PhaseManager.ChangeKey -= ChangeMotion;
    }
    private void ChangeMotion(float startDelay)
    {
        if (startDelay < 0)
        { 
            transform.position = originPosition;
            return;
        }
        _motionTween?.Kill();

        Sequence seq = DOTween.Sequence();

        // ��¦ �������� (Y�� ����)
        seq.Append(transform.DOMoveY(transform.position.y + _goingUpOffset, startDelay*0.1f)
                          .SetEase(Ease.OutQuad));
        // ��� ���
        seq.AppendInterval(startDelay * 0.1f);

        // Ÿ������ �� �̵�
        seq.Append(transform.DOMove(AdjustTargetPosY(), startDelay*0.2f)
                            .SetEase(Ease.InOutQuad));

        seq.AppendInterval(startDelay * 0.1f);
        seq.Append(transform.DOMoveY(targetPosition.y, startDelay * 0.1f)
                            .SetEase(Ease.InQuad));

        _motionTween = seq;
    }

    private Vector3 AdjustTargetPosY()
    { 
        return new Vector3(targetPosition.x, targetPosition.y + _goingUpOffset, targetPosition.z);
    }
}
