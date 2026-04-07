using System;
using UnityEngine;
using static GamePlayDefine;

public class HitJudge
{
    private float horizontalDiameter, verticalDiameter;
    private float perfectThreshold = 0.4f; //TODO : 80퍼센트 이상은 죄다 perfect 처리

    public HitJudge(float hori, float verti)
    {
        horizontalDiameter = hori;
        verticalDiameter = verti;
    }

    public bool CalculatePerfect(Vector2? hitPos, Vector2 target, WASDType t)
    {
        // Debug.Assert(hitPos.HasValue, $"success position target : {target}");

        float distanceFromTarget = Vector3.Distance(hitPos.Value, target);

        float circleDiameter;
        if (t == WASDType.A || t == WASDType.D)
            circleDiameter = horizontalDiameter;
        else
            circleDiameter = verticalDiameter;

        float ratio = distanceFromTarget / circleDiameter;
        // Debug.Log($"죽은 위치 : {hitPos}, 타켓 위치 : {target}, 지름 : {circleDiameter} 비율 : {ratio}");

        return ratio <= perfectThreshold;
    }

    public void UpdateRankCnt(RankNode rankNode, Vector2 target)
    {
        if(Managers.Game.currentPlayerState==GameManager.PlayerState.Ready) // 시작전이라면 받지않기
        {
            return;
        }
        switch (rankNode.RankT)
        {
            case EvaluateType.Attacked:
                IngameData.IncAttacked();
                break;
            case EvaluateType.Wrong:
                IngameData.IncWrong();
                break;
            case EvaluateType.Success:
                IngameData.IncMaxCombo();
                if (CalculatePerfect(rankNode.Pos, target, rankNode.WASDT))
                {
                    IngameData.IncPerfect();
                    if (Managers.Game.isADReverse)
                    {
                        if (rankNode.WASDT == WASDType.A)
                        {
                            rankNode.WASDT = WASDType.D;
                        }
                        else if (rankNode.WASDT == WASDType.D)
                        {
                            rankNode.WASDT = WASDType.A;
                        }
                    }
                    IngameData.OnPerfectEffect(rankNode.WASDT);
                }
                else
                    IngameData.IncGood();
                break;
        }
    }
}
