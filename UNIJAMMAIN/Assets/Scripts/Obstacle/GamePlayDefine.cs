public class GamePlayDefine
{
    public enum AllType
    {
        A,
        D,
        W,
        S,
        LeftUp,
        LeftDown,
        RightUp,
        RightDown,
        Idle,
        MaxCnt

    }
    public enum WASDType //무슨 키로 받을지
    {
        A,
        D,
        W,
        S,
        None,
        Random
    }
    public enum DiagonalType
    {
        LeftUp,
        LeftDown,
        RightUp,
        RightDown,
        MaxCnt
    }
    public enum MouseType
    { 
        Left,
        Right,
        MaxCnt
    }

    public enum EvaluateType
    {
        Success, // 몹 없애기 성공
        Wrong, // 몹 없는데 키 누름
        Attacked // 몹이 플레이어 공격함
    }

    public enum RankType
    { 
        Miss,
        Good,
        Perfect,
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
