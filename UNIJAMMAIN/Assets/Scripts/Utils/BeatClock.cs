using System;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    private double _startDsp;
    private double _beatInterval;
    private bool _running = false;
    private bool _wasPaused;
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
        if (IngameData.Pause)
        {
            _running = false;
            _wasPaused = true;
            return;
        }

        if (_wasPaused)
        {
            CatchUp();
            _wasPaused = false;
        }
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

    private void CatchUp()
    {
        double now = AudioSettings.dspTime;

        // 다음 줄이 핵심: 현재 시간 기준으로 tick을 스냅
        _tick = (long)System.Math.Floor((now - _startDsp) / _beatInterval);

        // 만약 재개 직후 비트가 즉시 울리는 것도 싫다면, Ceil로 바꿔:
        // _tick = (long)System.Math.Ceiling((now - _startDsp) / _beatInterval);

        _running = true;
    }
}