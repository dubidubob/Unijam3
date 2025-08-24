//using DG.Tweening;
//using System.Collections.Generic;
//using UnityEngine;

//public class NewDiagonalMonster : MonoBehaviour
//{
//    [SerializeField] List<float> scales = new List<float>();

//    int curCnt;
//    Vector3 baseScale;
//    Sequence seq; // 재사용

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
        
//        // 이미 펄스가 진행 중이면 이번 비트는 스킵 → 히치로 같은 프레임에 여러 번 와도 1회만 반영
//        if (seq != null && seq.IsActive() && seq.IsPlaying()) return;

//        float beat = (float)IngameData.BeatInterval;

//        var targetScale = baseScale * scales[curCnt++];

//        if (seq != null && seq.IsActive()) seq.Kill(); // 이전 잔여물 정리
//        seq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy).SetAutoKill(true);

//        // 앞 50%: 트윈 진행
//        seq.Append(transform.DOScale(targetScale*1.15f, beat * 0.3f).SetEase(Ease.OutCubic));
//        // 뒤 50%: 원래 크기 유지(대기)
//        // 1) 원래 크기로 즉시 스냅하고 쉬고 싶으면:
//        seq.Append(transform.DOScale(targetScale, beat*0.2f).SetEase(Ease.OutCubic));
//        seq.AppendInterval(beat*0.25f);
//    }
//}
