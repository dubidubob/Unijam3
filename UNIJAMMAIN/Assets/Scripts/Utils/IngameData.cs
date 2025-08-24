using System;

public static class IngameData 
{
    public static Action ChangeBpm;
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