using System;
using UnityEngine;
using static GamePlayDefine;

public class HitJudge
{
    private float horizontalDiameter, verticalDiameter;
    private float perfectThreshold = 0.4f; //TODO : 80�ۼ�Ʈ �̻��� �˴� perfect ó��
    
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
        // Debug.Log($"���� ��ġ : {hitPos}, Ÿ�� ��ġ : {target}, ���� : {circleDiameter} ���� : {ratio}");

        return ratio <= perfectThreshold;
    }

    public void UpdateRankCnt(RankNode rankNode, Vector2 target)
    {

        switch (rankNode.RankT)
        {
            case EvaluateType.Attacked:
                IngameData.IncAttacked();
                break;
            case EvaluateType.Wrong: 
                IngameData.IncWrong();
                break;
            case EvaluateType.Success:
                if (CalculatePerfect(rankNode.Pos, target, rankNode.WASDT))
                    IngameData.IncPerfect();
                else
                    IngameData.IncGood();
                break;
        }
    }
}
