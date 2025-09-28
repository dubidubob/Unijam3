using System;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    public static class GameSettings
    {
        // 노트가 보이는 타이밍 조절 (단위: 초)
        public static double VisualOffset = 0.0;

        // 사용자 입력 판정 타이밍 조절 (단위: 초)
        public static double JudgmentOffset = 0.0;
    }

    // --- 기존 변수 ---
    private double _startDsp;       // 노래가 처음 시작된 절대 시간 (고정)
    private double _beatInterval;   // 현재 비트 간격
    private bool _running = false;
    private bool _wasPaused;
    private long _tick;             // 현재 비트 카운트

    // --- 새로 추가된 변수 (타이밍 앵커) ---
    private double _lastBpmChangeDspTime; // 마지막으로 BPM이 변경된 시점의 dspTime
    private long _lastBpmChangeTick;      // 마지막으로 BPM이 변경된 시점의 tick
    

    public static Action<double, long> OnBeat;
    // --- phase와 연동 --- //
    [SerializeField] PhaseController phase;

    private void Start()
    {
        // Init() 대신 명확한 이름의 함수를 사용합니다.
        IngameData.ChangeBpm -= HandleBpmChange;
        IngameData.ChangeBpm += HandleBpmChange;

        // 게임 시작 시 최초 한 번만 호출되도록 변경 (예시)
        // 실제 프로젝트에 맞게 StartClock()을 적절한 시점에 호출해주세요.
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
        _startDsp = AudioSettings.dspTime;
        _tick = 0;

        // 첫 시작 시 앵커를 초기값으로 설정합니다.
        _lastBpmChangeDspTime = _startDsp;
        _lastBpmChangeTick = 0;

        _running = true;
        _wasPaused = false;
    }

    /// <summary>
    /// BPM 변경 이벤트가 발생할 때 호출됩니다.
    /// </summary>
    private void HandleBpmChange()
    {
        // 최초 시작이 아닌 경우에만 (이미 실행 중일 때) 앵커를 업데이트합니다.
        if (!_running && !_wasPaused) return;

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

    void Update()
    {
        if (!_running) return;

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

        double now = AudioSettings.dspTime;

        while (now >= ScheduledTime(_tick + 1))
        {
            _tick++;
            double scheduled = ScheduledTime(_tick);
            OnBeat?.Invoke(scheduled, _tick);
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
/* 다른 코드 적용해보기
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

    // ---비트 타이밍 조정--- //
    private double _lastBpmChangeDspTime;
    private long _lastBpmChangeTick;
    private void Start()
    {
        IngameData.ChangeBpm -= Init;
        IngameData.ChangeBpm += Init;
    }

    private void OnDestroy()
    {
        IngameData.ChangeBpm -= Init;
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
        _tick = (long)Math.Floor((now - _startDsp) / _beatInterval);
        _running = true;
    }
}
*/