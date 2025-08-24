public class GamePlayDefine
{
    public enum WASDType //무슨 키로 받을지
    {
        A,
        D,
        W,
        S,
        MaxCnt
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
