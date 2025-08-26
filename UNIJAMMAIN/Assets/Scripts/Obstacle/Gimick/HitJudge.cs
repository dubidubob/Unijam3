using System;
using UnityEngine;
using static GamePlayDefine;

public class HitJudge
{
    private float circleDiameter;
    private float perfectThreshold = 0.5f; //TODO : 80퍼센트 이상은 죄다 perfect 처리
    public static Action<RankType> OnRankUpdate;

    public HitJudge(float diameter)
    {
        circleDiameter = diameter;
    }

    public bool CalculatePerfect(Vector2? hitPos, Vector2 target)
    {
        Debug.Assert(hitPos.HasValue, $"success position target : {target}");

        float distanceFromTarget = Vector3.Distance(hitPos.Value, target);
        
        float ratio = distanceFromTarget / circleDiameter;
        Debug.Log($"죽은 위치 : {hitPos}, 타켓 위치 : {target}, 비율 : {ratio}");

        return ratio <= perfectThreshold;
    }

    public void UpdateRankCnt(RankNode rankNode, Vector2 target)
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
