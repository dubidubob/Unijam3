using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

public class Rank : MonoBehaviour
{
    public void UpdateRankCnt(RankNode rankNode, Vector3 target)
    {

        switch (rankNode.RankT)
        {
            case RankType.Attacked:
                IngameData.AttackedMobCnt++;
                break;
            case RankType.Wrong: 
                IngameData.WrongInputCnt++;
                break;
            case RankType.Success: 

                break;
        }
    }


}
