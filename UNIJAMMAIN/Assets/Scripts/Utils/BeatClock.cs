using System;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    private double _startDsp;
    private double _beatInterval;
    private bool _running = false;
    private long _tick;

    public static Action<double, long> OnBeat;

    private void Start()
    {
        IngameData.ChangeBpm -= Init;
        IngameData.ChangeBpm += Init;
    }
    private void Init()
    {
        _beatInterval = IngameData.BeatInterval;
        _startDsp = AudioSettings.dspTime;
        _tick = 0;
        _running = true;
    }

    void Update()
    {
        if (!_running) return;

        double now = AudioSettings.dspTime;

        // 한 프레임에 여러 박자가 지나갔을 경우 처리
        while (now >= ScheduledTime(_tick +1))
        {
            _tick += 1;
            double scheduled = ScheduledTime(_tick);
            OnBeat?.Invoke(scheduled, _tick);
        }
        
    }
    private double ScheduledTime(long tickIndex)
       => _startDsp + tickIndex * _beatInterval;
}