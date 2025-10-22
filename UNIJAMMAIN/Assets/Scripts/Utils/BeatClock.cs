    using System;
    using UnityEngine;

    public class BeatClock : MonoBehaviour
    {
        const double EPS = 0.002;

        private double _beatInterval;    // 현재 비트 간격
        private bool _running = false;
        // private bool _paused = false; // <-- BeatClock의 _paused 대신 IngameData.Pause를 직접 사용
        private bool _initialized = false;
        public long _tick;            // 현재 비트 카운트

        // --- 타이밍 앵커 ---
        private double _lastBpmChangeDspTime; // 마지막으로 BPM이 변경된 시점의 dspTime
        private long _lastBpmChangeTick;      // 마지막으로 BPM이 변경된 시점의 tick

        // --- 일시정지 시간 추적 ---
        private double _pauseStartedDspTime = -1.0; // 일시정지가 시작된 dspTime

        public static Action<long> OnBeat;

        private bool isStart = false;
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

        public void StartClock()
        {
            _beatInterval = IngameData.BeatInterval;
            _tick = 0;

            _lastBpmChangeDspTime = AudioSettings.dspTime; // 현재 dspTime을 기준점으로 설정
            _lastBpmChangeTick = 0;
            _pauseStartedDspTime = -1.0; // 일시정지 추적 변수 초기화

            _running = true;
            // _paused = false;
        }

        private void HandleBpmChange()
        {
            // if (!_running || _paused) return; // _paused 대신 IngameData.Pause 사용
            if (!_running || IngameData.Pause) return;
            if (!_initialized)
            {
                _initialized = true;
            
            }

            double now = AudioSettings.dspTime;

            // 이 계산은 _lastBpmChangeDspTime이 일시정지에 의해 보정되므로 항상 정확합니다.
            long currentTick = _lastBpmChangeTick + (long)Math.Floor((now - _lastBpmChangeDspTime) / _beatInterval);

            _lastBpmChangeDspTime = now;
            _lastBpmChangeTick = currentTick;
            _beatInterval = IngameData.BeatInterval;
        }


        void Update()
        {
            if (!isStart)
            {
                Managers.Sound.Play(phase.chapters[phase._chapterIdx].MusicPath, Define.Sound.BGM);
               StartClock();
               isStart = true;
            _initialized = true;
            return;
            }

            // --- [수정된 일시정지 로직] ---
            if (IngameData.Pause)
            {
                if (_pauseStartedDspTime < 0.0) // 일시정지가 *방금* 시작됨
                {
                    _pauseStartedDspTime = AudioSettings.dspTime;
                }
                return; // 일시정지 중에는 아무것도(틱 계산) 하지 않음
            }

            if (_pauseStartedDspTime >= 0.0) // 일시정지가 *방금* 풀림
            {
                // 일시정지된 기간을 계산
                double pauseDuration = AudioSettings.dspTime - _pauseStartedDspTime;

                // [핵심] 시간의 기준점(앵커)을 일시정지된 기간만큼 뒤로 밀어버림
                _lastBpmChangeDspTime += pauseDuration;

                _pauseStartedDspTime = -1.0; // 추적 변수 리셋
            }
            // --- [수정된 로직 끝] --- (CatchUp() 호출이 사라짐)


            double now = AudioSettings.dspTime;

            // ScheduledTime()이 _lastBpmChangeDspTime을 기반으로 계산하므로
            // 이제 'now'와 비교하는 이 로직은 일시정지를 고려한 채로 정상 작동합니다.
            while (now + EPS >= ScheduledTime(_tick + 1))
            {
                _tick++;
                OnBeat?.Invoke(_tick);
                phase.SetStageTimerGoScheduled(_tick, ScheduledTime(_tick));
            }
        }

        private double ScheduledTime(long tickIndex)
        {
            // _lastBpmChangeDspTime이 일시정지 보정을 받으므로 이 함수는 수정할 필요 없음
            return _lastBpmChangeDspTime + (tickIndex - _lastBpmChangeTick) * _beatInterval;
        }

        /// <summary>
        /// CatchUp() 함수는 이제 필요 없으므로 삭제합니다.
        /// </summary>
        // private void CatchUp() { ... }

        public double GetScheduledDspTimeForTick(long tick)
        {
            // ScheduledTime과 동일한 로직을 사용 (수정 필요 없음)
            return _lastBpmChangeDspTime + (tick - _lastBpmChangeTick) * _beatInterval;
        }

        public void GetNowTickDebugLog()
        {
        Debug.Log($"몬스터 처치 틱 : {_tick}");
        }
}