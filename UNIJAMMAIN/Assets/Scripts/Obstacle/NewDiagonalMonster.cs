//using DG.Tweening;
//using System.Collections.Generic;
//using UnityEngine;

//public class NewDiagonalMonster : MonoBehaviour
//{
//    [SerializeField] List<float> scales = new List<float>();

//    int curCnt;
//    Vector3 baseScale;
//    Sequence seq; // ����

//    void OnEnable()
//    {
//        baseScale = transform.localScale;
//        curCnt = 0;

//        BeatClock.OnBeat -= BeatMoving;
//        BeatClock.OnBeat += BeatMoving;
//    }

//    void OnDisable()
//    {
//        BeatClock.OnBeat -= BeatMoving;
//        if (seq != null && seq.IsActive()) seq.Kill();
//        transform.DOKill();
//        transform.localScale = baseScale;
//    }

//    void BeatMoving(double t)
//    {
//        if (curCnt >= scales.Count)
//        {
//            BeatClock.OnBeat -= BeatMoving;
//            return;
//        }
        
//        // �̹� �޽��� ���� ���̸� �̹� ��Ʈ�� ��ŵ �� ��ġ�� ���� �����ӿ� ���� �� �͵� 1ȸ�� �ݿ�
//        if (seq != null && seq.IsActive() && seq.IsPlaying()) return;

//        float beat = (float)IngameData.BeatInterval;

//        var targetScale = baseScale * scales[curCnt++];

//        if (seq != null && seq.IsActive()) seq.Kill(); // ���� �ܿ��� ����
//        seq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy).SetAutoKill(true);

//        // �� 50%: Ʈ�� ����
//        seq.Append(transform.DOScale(targetScale*1.15f, beat * 0.3f).SetEase(Ease.OutCubic));
//        // �� 50%: ���� ũ�� ����(���)
//        // 1) ���� ũ��� ��� �����ϰ� ���� ������:
//        seq.Append(transform.DOScale(targetScale, beat*0.2f).SetEase(Ease.OutCubic));
//        seq.AppendInterval(beat*0.25f);
//    }
//}
