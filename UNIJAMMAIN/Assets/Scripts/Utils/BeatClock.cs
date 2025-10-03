using System;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    const double EPS = 0.002;

    private double _beatInterval;   // 현재 비트 간격
    private bool _running = false;
    private bool _paused = false;
    private bool _initialized = false;
    private long _tick;             // 현재 비트 카운트

    // --- 타이밍 앵커 ---
    private double _lastBpmChangeDspTime; // 마지막으로 BPM이 변경된 시점의 dspTime
    private long _lastBpmChangeTick;      // 마지막으로 BPM이 변경된 시점의 tick

    public static Action<long> OnBeat;
    public static long CurrentTick { get; private set; }
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
        CurrentTick = 0; // 인스턴스 변수 초기화


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

        double now = AudioSettings.dspTime;

        // 이전 beatInterval을 기준으로 현재 tick 위치를 정확히 계산
        long currentTick = _lastBpmChangeTick + (long)Math.Floor((now - _lastBpmChangeDspTime) / _beatInterval);

        // 현재 시점과 tick을 새로운 앵커로 설정
        _lastBpmChangeDspTime = now;
        _lastBpmChangeTick = currentTick;
        _tick = currentTick; // 현재 tick도 업데이트

        // 새로운 beatInterval로 변경
        _beatInterval = IngameData.BeatInterval;
    }

    // 박자가 끊길 때마다 -> 부하가 너무 심하다
    // Coroutine : 부하 꽤 많음. -> 비동기 => Text가 있을 때마다
    void Update()
    {
       
        
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
             CurrentTick = _tick;
            OnBeat?.Invoke(_tick);
            phase.SetStageTimerGo();
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
        double now = AudioSettings.dspTime;

        // 앵커를 기준으로 현재 tick을 정확히 스냅합니다.
        _tick = _lastBpmChangeTick + (long)Math.Floor((now - _lastBpmChangeDspTime) / _beatInterval);
        _running = true;
    }
}