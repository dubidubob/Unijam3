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
    public enum WASDType //���� Ű�� ������
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
        Success, // �� ���ֱ� ����
        Wrong, // �� ���µ� Ű ����
        Attacked // ���� �÷��̾� ������
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
