using System;
using static GamePlayDefine;

public static class IngameData 
{
    public static Action ChangeBpm;
    public static Action<RankType> OnRankUpdate;

    public static bool Pause { set; get; }

    private static double _beatInterval;
    public static double BeatInterval
    {
        get => _beatInterval;
        set
        {
            if (Math.Abs(_beatInterval - value) > 1e-6)
            {
                _beatInterval = value;
                ChangeBpm?.Invoke();
            }
        }
    }

    public static int ChapterIdx { set; get; }

    private static Define.Rank _chapterRank = Define.Rank.Unknown;
    public static Define.Rank ChapterRank 
    { 
        set { _chapterRank = value; } 
        get => _chapterRank; 
    }

    public static float PhaseDuration { set; get; }
    public static int TotalMobCnt { set; get; }

    public static int PerfectMobCnt { get; private set; }
    public static int GoodMobCnt { get; private set; }
    public static int WrongInputCnt { get; private set; }
    public static int AttackedMobCnt { get; private set; }

    public static void IncPerfect() { PerfectMobCnt++; OnRankUpdate?.Invoke(RankType.Perfect); }
    public static void IncGood() { GoodMobCnt++; OnRankUpdate?.Invoke(RankType.Good); }
    public static void IncWrong() { WrongInputCnt++; OnRankUpdate?.Invoke(RankType.Miss); }
    public static void IncAttacked() { AttackedMobCnt++; OnRankUpdate?.Invoke(RankType.Miss); }

    public static void RankInit()
    {
        Pause = false;
        TotalMobCnt = 0;
        PerfectMobCnt = 0;
        GoodMobCnt = 0;
        WrongInputCnt = 0;
        AttackedMobCnt = 0;
    }
}