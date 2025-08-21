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
}