public class GamePlayDefine
{
    public enum WASDType //���� Ű�� ������
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

    public enum RankType
    {
        Success, // �� ���ֱ� ����
        Wrong, // �� ���µ� Ű ����
        Attacked // ���� �÷��̾� ������
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
