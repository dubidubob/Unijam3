using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayDefine
{
    public enum MovingAttackType //���� Ű�� ������
    {
        A,
        D,
        W,
        S,
        MaxCnt
    }
    public enum RangedAttackType
    {
        LeftUp,
        LeftDown,
        RightUp,
        RightDown,
        MaxCnt
    }

    public enum IllustType
    { 
        Start,
        Phase1End,
        Phase2End,
        Fail,
        Success,
        Num
    }
}
