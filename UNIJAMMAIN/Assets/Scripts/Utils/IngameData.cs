using System;

public static class IngameData 
{
    public static Action ChangeBpm;
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
    public static Define.Rank ChapterRank { set { _chapterRank = value; } get => _chapterRank; }
    public static float PhaseDuration { set; get; }
    public static int TotalMobCnt { set; get; }
    public static int PerfectMobCnt { set; get; }
    public static int GoodMobCnt { set; get; }
    public static int WrongInputCnt { set; get; }
    public static int AttackedMobCnt { set; get; }
    public static void RankInit()
    {
        TotalMobCnt = 0;
        PerfectMobCnt = 0;
        GoodMobCnt = 0;
        WrongInputCnt = 0;
        AttackedMobCnt = 0;
    }
}