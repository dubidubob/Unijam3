using System;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    const double EPS = 0.002;

    private double _beatInterval;    // 현재 비트 간격
    private bool _running = false;

    public long _tick;            // 현재 비트 카운트

    // --- 타이밍 앵커 ---
    private double _startTime;            // BGM이 시작될(된) 정확한 dspTime
    private double _lastBpmChangeDspTime; // 마지막으로 BPM이 변경된 시점의 dspTime (Tick 계산 기준점)
    private long _lastBpmChangeTick;      // 마지막으로 BPM이 변경된 시점의 tick

    // --- 일시정지 시간 추적 ---
    private double _pauseStartedDspTime = -1.0; // 일시정지가 시작된 dspTime

    public static Action<long> OnBeat;

    [SerializeField] PhaseController phase;

    private void Start()
    {
        IngameData.ChangeBpm -= HandleBpmChange;
        IngameData.ChangeBpm += HandleBpmChange;
        Managers.Game.beatClock = this;
    }

    private void OnDestroy()
    {
        IngameData.ChangeBpm -= HandleBpmChange;
    }

    /// <summary>
    /// [수정됨] 외부(RunChapter)에서 예약된 BGM 시작 시간을 받아 초기화합니다.
    /// 이 함수가 호출되면 시계가 '장전'된 상태가 됩니다.
    /// </summary>
    /// <param name="scheduledStartTime">AudioSettings.dspTime + delay된 미래 시간</param>
    public void SetStartTime(double scheduledStartTime)
    {
        _beatInterval = IngameData.BeatInterval;
        _tick = 0;

        // 시작 시간 설정
        _startTime = scheduledStartTime;

        // 첫 시작이므로 BPM 변경 기준점도 시작 시간과 동일하게 맞춤 (Tick 0 = StartTime)
        _lastBpmChangeDspTime = _startTime;
        _lastBpmChangeTick = 0;

        _pauseStartedDspTime = -1.0;
        _running = true;

        Debug.Log($"[BeatClock] Timer Set. Starts at DSP: {_startTime}");
    }

    // 런타임 도중(게임 진행 중) BPM이 바뀔 때 호출
    private void HandleBpmChange()
    {
        if (!_running || IngameData.Pause) return;

        // [삭제됨] BGM 재생 로직 및 초기화 로직은 SetStartTime과 PhaseController로 이동했습니다.

        double now = AudioSettings.dspTime;

        // 현재 시점까지의 틱을 확정 짓고, 기준점(앵커)을 현재로 갱신
        long currentTick = _lastBpmChangeTick + (long)Math.Floor((now - _lastBpmChangeDspTime) / _beatInterval);

        _lastBpmChangeDspTime = now;
        _lastBpmChangeTick = currentTick;
        _beatInterval = IngameData.BeatInterval;
    }

    void Update()
    {
        if (!_running) return;

        // --- [일시정지 로직] ---
        if (IngameData.Pause)
        {
            if (_pauseStartedDspTime < 0.0) // 일시정지가 *방금* 시작됨
            {
                _pauseStartedDspTime = AudioSettings.dspTime;
            }
            return; // 일시정지 중에는 틱 계산 중단
        }

        if (_pauseStartedDspTime >= 0.0) // 일시정지가 *방금* 풀림
        {
            // 일시정지된 기간 계산
            double pauseDuration = AudioSettings.dspTime - _pauseStartedDspTime;

            // [핵심] 모든 시간 기준점을 일시정지 기간만큼 뒤로 미룸
            _lastBpmChangeDspTime += pauseDuration;
            _startTime += pauseDuration; // 시작 예정 시간도 밀어야 함 (시작 전 일시정지 대응)

            _pauseStartedDspTime = -1.0;
        }

        double now = AudioSettings.dspTime;

        // [추가됨] 아직 예약된 시작 시간(1초 대기 등)이 안 됐으면 아무것도 안 함
        if (now < _startTime)
        {
            return;
        }

        // --- [틱 계산 로직] ---
        // ScheduledTime()은 _lastBpmChangeDspTime을 기준으로 계산하므로
        // 일시정지 보정이 적용된 상태임.
        while (now + EPS >= ScheduledTime(_tick + 1))
        {
            _tick++;

            // 틱 이벤트 발생
            OnBeat?.Invoke(_tick);

            // 페이즈 컨트롤러 업데이트
            phase.SetStageTimerGoScheduled(_tick, ScheduledTime(_tick));
        }
    }

    /// <summary>
    /// 특정 틱이 되어야 할 정확한 dspTime을 반환
    /// </summary>
    private double ScheduledTime(long tickIndex)
    {
        return _lastBpmChangeDspTime + (tickIndex - _lastBpmChangeTick) * _beatInterval;
    }

    public double GetScheduledDspTimeForTick(long tick)
    {
        return ScheduledTime(tick);
    }

    public void GetNowTickDebugLog()
    {
        Debug.Log($"Current Tick : {_tick}");
    }
}