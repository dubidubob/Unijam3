using System;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    const double EPS = 0.002;

    private double _beatInterval;   // 현재 비트 간격
    private bool _running = false;
    private bool _paused = false;
    private bool _initialized = false;
    public long _tick;             // 현재 비트 카운트
    private double _test_BeforeTime;
    private double _test_BeforeScheduledTime;

    // --- 타이밍 앵커 ---
    private double _lastBpmChangeDspTime; // 마지막으로 BPM이 변경된 시점의 dspTime
    private long _lastBpmChangeTick;      // 마지막으로 BPM이 변경된 시점의 tick

    public static Action<long> OnBeat;
    

    private bool isStart = false;
    // --- phase와 연동 --- //
    [SerializeField] PhaseController phase;

   

    private void Start()
    {
        IngameData.ChangeBpm -= HandleBpmChange;
        IngameData.ChangeBpm += HandleBpmChange;


        StartClock();
    }

    private void OnDestroy()
    {
        IngameData.ChangeBpm -= HandleBpmChange;
    }

    /// <summary>
    /// 시계를 맨 처음 시작할 때 호출됩니다.
    /// </summary>
    public void StartClock()
    {
        _beatInterval = IngameData.BeatInterval;
        _tick = 0;


        _lastBpmChangeDspTime = AudioSettings.dspTime;
        _lastBpmChangeTick = 0;

        _running = true;
        _paused = false;
    }

    /// <summary>
    /// BPM 변경 이벤트가 발생할 때 호출됩니다.
    /// </summary>
    private void HandleBpmChange()
    {
        
        if (!_running || _paused) return;
        if (!_initialized)
        {
            StartClock();
            _initialized = true;
        }
        // 새로운 beatInterval로 변경
        
        Debug.Log($"BPM이 Change된 Tick : {_tick}");
        double now = AudioSettings.dspTime;

        // 이전 beatInterval을 기준으로 현재 tick 위치를 정확히 계산
        long currentTick = _lastBpmChangeTick + (long)Math.Floor((now - _lastBpmChangeDspTime) / _beatInterval);

        // 현재 시점과 tick을 새로운 앵커로 설정
        _lastBpmChangeDspTime = now;
        _lastBpmChangeTick = currentTick;

        _beatInterval = IngameData.BeatInterval;
    }

    // 박자가 끊길 때마다 -> 부하가 너무 심하다
    // Coroutine : 부하 꽤 많음. -> 비동기 => Text가 있을 때마다
    void Update()
    {

        if (!isStart)
        {
            Managers.Sound.Play(phase.chapters[phase._chapterIdx].MusicPath, Define.Sound.BGM);
            isStart = true;
        }
        if (IngameData.Pause)
        {
            _paused = true;
            return;
        }

        if (_paused)
        {
            CatchUp();
            _paused = false;    
        }

        double now = AudioSettings.dspTime;

        // 시작 + (현재 tick+1) * 박자간격
        while (now + EPS >= ScheduledTime(_tick + 1))
        {
            _tick++;
            OnBeat?.Invoke(_tick);

            double scheduled = ScheduledTime(_tick); // 이 tick의 '정확한' dsp 예정시각

            // (변경) phase에 scheduled tick/time을 직접 전달
            phase.SetStageTimerGoScheduled(_tick, scheduled);

            _test_BeforeScheduledTime = scheduled; // 디버그용: 예정 시간 기준으로 체크
        }
    }

    /// <summary>
    /// 특정 tick의 정확한 예정 시간을 계산합니다. (수정된 핵심 로직)
    /// </summary>
    private double ScheduledTime(long tickIndex)
    {
        // 마지막 앵커 시점으로부터 얼마나 많은 비트가 지났는지 계산
        return _lastBpmChangeDspTime + (tickIndex - _lastBpmChangeTick) * _beatInterval;
    }

    /// <summary>
    /// 일시정지 후 복귀 시 현재 시간에 맞게 tick을 보정합니다. (수정된 로직)
    /// </summary>
    private void CatchUp()
    {
        Debug.Log("CatchUp!");
        double now = AudioSettings.dspTime;
        long calculatedTick = _lastBpmChangeTick + (long)Math.Floor((now - _lastBpmChangeDspTime) / _beatInterval);

        // 계산된 틱이 현재 틱보다 작으면, 현재 틱을 유지
        _tick = Math.Max(_tick, calculatedTick);

        _running = true;
    }

    public double GetScheduledDspTimeForTick(long tick)
    {
        // 같은 로직을 재사용: last anchor + (tick - anchorTick) * beatInterval
        return _lastBpmChangeDspTime + (tick - _lastBpmChangeTick) * _beatInterval;
    }

}