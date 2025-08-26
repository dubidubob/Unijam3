using System;
using UnityEngine;
using static GamePlayDefine;

public class HitJudge
{
    private float circleDiameter;
    private float perfectThreshold = 0.8f; //80퍼센트 이상은 죄다 perfect 처리
    public static Action<RankType> OnRankUpdate;

    public void SetRadius(Vector3 target, Vector3 end)
    {
        // 풀 지름
        circleDiameter = Mathf.Abs(Vector3.Distance(target, end)) * 2f;
    }

    public bool CalculatePerfect(Vector2? hitPos, Vector3 target)
    {
        Debug.Assert(hitPos.HasValue, $"success position target : {target}");
        
        float distanceFromTarget = Vector3.Distance(hitPos.Value, target);
        float ratio = distanceFromTarget / circleDiameter;

        return ratio <= perfectThreshold;
    }

    public void UpdateRankCnt(RankNode rankNode, Vector3 target)
    {

        switch (rankNode.RankT)
        {
            case EvaluateType.Attacked:
                IngameData.AttackedMobCnt++;
                OnRankUpdate?.Invoke(RankType.Miss);
                break;
            case EvaluateType.Wrong: 
                IngameData.WrongInputCnt++;
                OnRankUpdate?.Invoke(RankType.Miss);
                break;
            case EvaluateType.Success:
                if (CalculatePerfect(rankNode.Pos, target))
                {
                    IngameData.PerfectMobCnt++;
                    OnRankUpdate?.Invoke(RankType.Perfect);
                }
                else
                {
                    IngameData.GoodMobCnt++;
                    OnRankUpdate?.Invoke(RankType.Good);
                }
                break;
        }
    }
}
