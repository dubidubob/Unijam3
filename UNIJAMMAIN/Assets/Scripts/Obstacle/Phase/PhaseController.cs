using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks; // UniTask 네임스페이스 추가
using System.Threading; // CancellationToken 사용

/// <summary>
/// Phase를 순서대로 재생시키는 클래스
/// </summary>
[Serializable]
struct StartMotionUIs
{
    public StartCountUI startCountUI;
    public PlayerActionUI playerActionUI;
}

[RequireComponent(typeof(SpawnController))]
public class PhaseController : MonoBehaviour
{
    public bool isTest;
    public int TestStageIndex;
    public bool isSinkStage = false;

    [SerializeField] private MonsterDatabaseSO monsterDatabase;


    [SerializeField] Image backGround;
    [SerializeField] SpriteRenderer characterSprite;
    [SerializeField] ResultUI Scoreboard;
    [SerializeField] BeatClock beatClock;
    [SerializeField] UI_Popup tutorialPopUp;

    [SerializeField] public ChapterSO[] chapters;
    [SerializeField] StartMotionUIs startMotions;
    [SerializeField] SpriteRenderer areaBaseInLine;
    [SerializeField] Image gaugeImage;

    public static Action<float> ChangeKey;
    public static Action<bool> TutorialStoped;
    public int _chapterIdx;
    private int _lastMonsterHitCnt = 0;
    public float _totalBeat = 0;
    private float _beatCount = 0;
    SpawnController spawnController;
    [SerializeField] WASDMonsterSpawner wasdMonsterSpawner;
    [SerializeField] CameraController cameraController;


    private bool beatSynced = false; // 비트 동기화 신호를 위한 플래그
    private bool isMonsterGoStart = false;

    float beatInterval;
    float delaySec;
    float durationSec;

    // UniTask 취소 토큰 (오브젝트 파괴 시 작업 중단용)
    private CancellationTokenSource _cts;

    private void Start()
    {

        Managers.Game.phaseController = this;
        IngameData.IsStart = false;
        spawnController = GetComponent<SpawnController>();
        Scoreboard.gameObject.SetActive(false);
        monsterDatabase.Init();

        IngameData.RankInit();
        Managers.Game.monster = monsterDatabase;

       
       

        

        _chapterIdx = Mathf.Min(IngameData.ChapterIdx, chapters.Count() - 1);
        if (isTest)
        {
            _chapterIdx = TestStageIndex;
            IngameData.ChapterIdx = _chapterIdx;
        }
        PreloadWASDPatterns(_chapterIdx);
        // 데이터 캐싱


        SetStageBackGround();

        Color tmpColor = chapters[_chapterIdx].colorPalette;
        tmpColor.a = 0.7f;
        areaBaseInLine.color = tmpColor;

        SetStageTimerInitialize();
        Managers.Game.GameStart();
        // UniTask 실행 (Start는 void이므로 async void 대신 Forget() 패턴 사용 권장)
        RunChapter().Forget();
       
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 실행 중인 UniTask 정리
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private void HandleBeatSync(long __)
    {
        beatSynced = true;
        BeatClock.OnBeat -= HandleBeatSync;
    }

    // async UniTask로 변경
    private async UniTaskVoid RunChapter()
    {   
        // 토큰 획득 (이 스크립트가 파괴되면 await 중인 작업들이 캔슬됨)
        var token = this.GetCancellationTokenOnDestroy();

        // 게임 안정화 로딩
        await GameInitLoading(token);

       
        IngameData.IsStart = true;

        // 싱크스테이지라면
        if (chapters[_chapterIdx].SinkStage)
        {
            // SinkStageInit도 UniTask로 대기
            await SinkStageInit(token);
            return;
        }
       

       



        // --- 루프 시작 전, 첫 번째 페이즈의 BPM을 미리 설정합니다 ---
        if (chapters[_chapterIdx].Phases.Count > 0)
        {
            var firstPhase = chapters[_chapterIdx].Phases[0];
            beatInterval = 60.0f / firstPhase.bpm;
            IngameData.GameBpm = (int)firstPhase.bpm;
            delaySec = firstPhase.startDelayBeat * beatInterval;
            durationSec = firstPhase.durationBeat * beatInterval;
            IngameData.PhaseDurationSec = durationSec;

            IngameData.BeatInterval = 60.0 / firstPhase.bpm;
            IngameData.ChangeBpm?.Invoke();
        }
        var _event = chapters[_chapterIdx].Phases[0];
        if(_event is PhaseEvent _parsingEvent)
        {
            // Phase의 0 부터 끝까지
            // _parsingEvent.MonsterData[0] 부터 끝까지 MonsterData[i].WASD_Pattern <- string 에 대해 
            // 미리 파싱해두기 
            // GetOrParsePattern 을 타 함수(datainitialize 하는 타 코드에서) 실행할것임.
        }

        long targetTick = 0;
        // 현재 오디오 엔진 시간에서 1초(혹은 0.5초) 뒤를 '시작 시간'으로 잡습니다.
        //    이 1초의 여유 동안 오디오 엔진은 소리 낼 준비를 완벽히 마칩니다.
        double musicStartTime = AudioSettings.dspTime + 1f;

        // 사운드 매니저에게 "이 시간에 정확히 틀어라"고 예약합니다.
        string bgmPath = chapters[_chapterIdx].MusicPath;


        // DelayPadding의 Tick의 Second 만큼 대기 ( 몬스터를 미리 소환하고 노래를 늦게 재생 ) 
        // sinkTimer추가, sinkTimer만큼 노래가 늦게 재생되거나 빨리재생됨
        // 노래가 빨리 재생된다는것(SinTimer가 + 라는것은 몹이 늦게 도착한다는 뜻이다)
        double waitSecondTarget = (long)chapters[_chapterIdx].DelayPaddingTick * IngameData.BeatInterval+IngameData.sinkTimer;
        Managers.Sound.PlayScheduled(bgmPath, musicStartTime + waitSecondTarget, Define.Sound.BGM);


        // BeatClock에게도 "게임 시작 시간은 startTime이다"라고 알려줍니다.
        //    BeatClock은 이제 Update에서 (dspTime - startTime)을 통해 틱을 계산해야 합니다.
        beatClock.SetStartTime(musicStartTime);
        
        

        //  BGM이 시작될 때까지(1초간) 대기
        //    이렇게 하면 "소리가 나는 순간"과 "로직이 시작되는 순간"이 맞음.
        await UniTask.WaitUntil(() => AudioSettings.dspTime >= musicStartTime, cancellationToken: token);

        // 주석처리하면 윈도우 스트레치 꺼짐
        //cameraController.WindowStretchAction(60,2,0).Forget();
       // cameraController.WindowRythmContinueStretchAction(60).Forget();
        for (int i = 0; i < chapters[_chapterIdx].Phases.Count; i++)
        {
            var gameEvent = chapters[_chapterIdx].Phases[i];
            if (!gameEvent.isIn) continue;

            float durationSec = gameEvent.durationBeat * beatInterval;

            // [핵심 수정 - 대윤이형 확인 바람] 
            // "비트를 초로 바꾸는 게 목적"이 아니라, 
            // "스포너가 초 단위(dspTime)로 작동하니까, 연장할 비트만큼 시간을 더해주는 것"입니다.
            if (gameEvent is PhaseEvent pEvent)
            {
                // 연장할 비트(extensionCreateBeat)가 있다면, 
                // 그만큼의 시간(beatInterval 곱하기)을 더해줍니다.
                float extensionSec = pEvent.extensionCreateBeat * beatInterval;

                // 최종적으로 "원래 시간 + 연장된 시간"을 전역 변수에 넣어줍니다.
                IngameData.PhaseDurationSec = durationSec + extensionSec;
            }
            else
            {
                IngameData.PhaseDurationSec = durationSec;
            }
            // [확인 바람]


            // 기존 코드
            //IngameData.PhaseDurationSec = durationSec;


            // --- 1. Delay 구간 처리 ---
            long delayBeats = Mathf.RoundToInt(gameEvent.startDelayBeat);

            if (gameEvent is PhaseEvent phaseEvent)
            {
                Managers.Game.CurrentState = GameManager.GameState.Battle;
                float delaySec = delayBeats * (float)IngameData.BeatInterval;
                HandleFlipKeyEvent(phaseEvent, delaySec);
            }
            else if (gameEvent is TutorialEvent tutorialEvent)
            {
                Managers.Game.CurrentState = GameManager.GameState.Tutorial;
                if (i == 0)
                    TutorialStoped?.Invoke(true);
                HandleTutorialEvent(tutorialEvent);
            }

            // [WaitUntil 변경] 목표 틱까지 대기
            targetTick += delayBeats;
            await UniTask.WaitUntil(() => beatClock._tick >= targetTick, cancellationToken: token);

            // --- 2. Duration 구간 처리 ---
            long durationBeats = Mathf.RoundToInt(gameEvent.durationBeat);
            targetTick += durationBeats;

            // Delay 직후 로직 실행
            if (gameEvent is PhaseEvent phaseEventAfterDelay)
            {
                SpawnMonsters(phaseEventAfterDelay, targetTick+phaseEventAfterDelay.extensionCreateBeat);
            }
            else if (gameEvent is TutorialEvent tutorialEventAfterDelay)
            {
                if (i == 0)
                    TutorialStoped?.Invoke(false);
            }

            if (i + 1 < chapters[_chapterIdx].Phases.Count)
            {
                var next = chapters[_chapterIdx].Phases[i + 1];
                if (next.bpm == chapters[_chapterIdx].Phases[i].bpm)
                {
                    targetTick -= (int)next.preGenerateBeat;
                }
            }

            // [WaitUntil 변경] duration 종료까지 대기
            await UniTask.WaitUntil(() => beatClock._tick >= targetTick, cancellationToken: token);

            // --- 3. 다음 페이즈 준비 ---
            if (i + 1 < chapters[_chapterIdx].Phases.Count)
            {
                var nextPhase = chapters[_chapterIdx].Phases[i + 1];
                if (!(nextPhase.bpm == chapters[_chapterIdx].Phases[i].bpm))
                {
                    IngameData.BeatInterval = 60.0 / nextPhase.bpm;
                    beatInterval = 60.0f / nextPhase.bpm;
                    IngameData.GameBpm = (int)nextPhase.bpm;
                    delaySec = nextPhase.startDelayBeat * beatInterval;

                    IngameData.PhaseDurationSec = durationSec;
                    IngameData.ChangeBpm?.Invoke();
                }
            }
        }

        // 모든 페이즈 종료 후 여유 비트 대기
        targetTick += 2;
        await UniTask.WaitUntil(() => beatClock._tick >= targetTick, cancellationToken: token);

        EndChapter();
    }

    private void HandleFlipKeyEvent(PhaseEvent phaseEvent, float delaySec)
    {
        if (phaseEvent.isFlipAD)
        {
            Managers.Game.SetADReverse(true);
            ChangeKey?.Invoke(delaySec);
        }
        else
        {
            Managers.Game.SetADReverse(false);
            ChangeKey?.Invoke(-1f);
        }
    }

    private void SpawnMonsters(PhaseEvent phaseEvent, long targetTick)
    {
        // SpawnController가 IEnumerator라면 ToUniTask로 감싸고, 
        // UniTaskVoid라면 그냥 호출, 여기서는 기존 호환성을 위해 StartCoroutine 유지 가능하지만
        // UniTask 변환을 권장합니다. 아래는 SpawnController가 기존 코루틴일 경우의 호환 코드입니다.
        // 만약 SpawnController도 UniTask로 바꿨다면 .Forget()을 쓰세요.

        spawnController.SpawnUntilTargetTick(phaseEvent.MonsterDatas, targetTick).Forget();
    }

    private void HandleTutorialEvent(TutorialEvent tutorialEvent)
    {
        tutorialPopUp.gameObject.SetActive(true);
        var tuto = tutorialPopUp.GetComponent<Tutorial_PopUp>();
        tuto.StartTutorial(tutorialEvent.Steps, _lastMonsterHitCnt);
        _lastMonsterHitCnt = IngameData.PerfectMobCnt + IngameData.GoodMobCnt;
    }

    private void EndChapter()
    {
        spawnController.StopMonsterInPhase();
        Managers.Sound.PauseBGM(false);
        IngameData.Pause = false;

        // 스팀 업적
        CheckFirstClearSteamAchievement();


        Scoreboard.ChangeUI(CalculateScore());
        Scoreboard.gameObject.SetActive(true);


        SetStageIndex(_chapterIdx);
    }

    private float perfectWeight = 1.0f;
    private float goodWeight = 0.5f;
    private float CalculateScore()
    {
        float perfectCnt = IngameData.PerfectMobCnt;
        float goodCnt = IngameData.GoodMobCnt;
        float rate = (perfectCnt * perfectWeight + goodCnt * goodWeight);
        float totalCnt = IngameData.TotalMobCnt;
        float total = totalCnt;


        return (rate / total) * 100f;
    }

    #region Setting

    public void PreloadWASDPatterns(int chapterIdx)
    {
        // 1. WASD 스포너 참조 가져오기
        // (싱글톤 매니저나, 참조 변수가 있다면 그것을 사용하세요)
        WASDMonsterSpawner wasdSpawner = FindObjectOfType<WASDMonsterSpawner>();

        if (wasdSpawner == null)
        {
            Debug.LogWarning("WASD 스포너를 찾을 수 없어 프리로딩을 건너뜁니다.");
            return;
        }

        // 2. 해당 챕터 가져오기
        var currentChapter = chapters[chapterIdx];
        if (currentChapter == null) return;

        // 3. 챕터 내의 '모든 페이즈' 순회 (0부터 끝까지)
        foreach (var phase in currentChapter.Phases)
        {
            // PhaseEvent 타입인 경우에만 처리 (몬스터 데이터가 있는 이벤트)
            if (phase is PhaseEvent phaseEvent)
            {
                // 4. 페이즈 내의 '모든 몬스터 데이터' 순회
                foreach (var monsterData in phaseEvent.monsterDatas)
                {
                    // 패턴 문자열이 비어있지 않다면 파싱 요청
                    if (!string.IsNullOrEmpty(monsterData.WASD_Pattern))
                    {
                        // [핵심] 리턴값은 필요 없습니다. 호출하는 순간 스포너 내부 Dictionary에 저장됩니다.
                        wasdSpawner.GetOrParsePattern(monsterData.WASD_Pattern);
                    }
                }
            }
        }

        Debug.Log($"Chapter {chapterIdx}의 모든 WASD 패턴 프리로딩 완료!");
    }
    async void WaitTick(float waitTick)
    {
        await UniTask.WaitForSeconds((float)beatClock.GetScheduledDspTimeForTick((long)waitTick));
    }


    private void SetStageIndex(int index)
    {
        int minMaxStage_nowStageIndexPlus = Mathf.Min(IngameData._nowStageIndex+1, 7); // 7보다 초과되면(최대스테이지라면) 7로 고정
        IngameData._clearStageIndex = Mathf.Max(IngameData._clearStageIndex, minMaxStage_nowStageIndexPlus); 
    }

    private void SetStageBackGround()
    {
        backGround.overrideSprite = chapters[_chapterIdx].backGroundSprite;
        // 프리팹 로드 로직 (UniTask에서는 LoadAsync 사용 가능하나 여기선 기존 로직 유지)
        if (_chapterIdx == 1)
        {
            GameObject particle = Resources.Load<GameObject>("Prefabs/LeafParticleRain");
            if (particle != null) Instantiate(particle);
        }

        if (_chapterIdx == 3)
        {
            GameObject rain = Resources.Load<GameObject>("Prefabs/Rain_Front");
            Instantiate(rain);
            backGround.color = new Color(127f / 255f, 133f / 255f, 145f / 255f);
            characterSprite.color = new Color(180f / 255f, 180f / 255f, 180f / 255f);
        }

        if (_chapterIdx == 4)
        {
            GameObject particle = Resources.Load<GameObject>("Prefabs/SnowParticleRain");
            if (particle != null) Instantiate(particle);
        }
    }

    private void SetStageTimerInitialize()
    {
        for (int i = 0; i < chapters[_chapterIdx].Phases.Count; i++)
        {
            _totalBeat += chapters[_chapterIdx].Phases[i].durationBeat;
            _totalBeat += chapters[_chapterIdx].Phases[i].startDelayBeat;
        }
    }

    public void SetStageTimerGo()
    {
        double now = AudioSettings.dspTime;
        long inferredTick = _lastScheduledTick >= 0 ? _lastScheduledTick + 1 : (_lastScheduledTick = 1);
        SetStageTimerGoScheduled(inferredTick, now);
    }

    // Steam 업적 :  처음으로 클리어되었다면
    private void CheckFirstClearSteamAchievement()
    {
        string achievementID = $"ACH_CHAPTER_{_chapterIdx}_CLEAR";

        // 챕터 기록이 없고 연습모드가 아니라면
        if(IngameData.GetRankForChapter(_chapterIdx)==Define.Rank.Unknown&&!IngameData.boolPracticeMode)
        {
            Managers.Steam.UnlockAchievement(achievementID);
        }
    }

    

    public async UniTask GoStart()
    {
        beatSynced = false;
        BeatClock.OnBeat += HandleBeatSync;

        // WaitUntil로 동기화 대기
        await UniTask.WaitUntil(() => beatSynced, cancellationToken: this.GetCancellationTokenOnDestroy());

        // startCountUI가 IEnumerator라면 ToUniTask로 변환하여 대기
        if (startMotions.startCountUI != null)
        {
            await startMotions.startCountUI.Play123Coroutine().ToUniTask(this);
        }

        startMotions.playerActionUI.StartMonkAnimAfter123Count();
    }
    #endregion

    #region QA
    float qa_startDelay, qa_phaseDuration;
    MonsterData[] monsters = new MonsterData[1];

    public void Play()
    {
        StopAllCoroutines(); // 기존 코루틴 정지
        // UniTask의 경우 CancellationToken으로 제어하지만, 
        // 여기서는 간단히 QA용 Task를 실행합니다.
        RunQAPhase().Forget();
    }

    private async UniTaskVoid RunQAPhase()
    {
        var token = this.GetCancellationTokenOnDestroy();

        // WaitForSeconds 대체 (밀리초 단위 변환)
        await UniTask.Delay(TimeSpan.FromSeconds(qa_startDelay), cancellationToken: token);

        spawnController.SpawnMonsterInPhase(monsters);

        await UniTask.Delay(TimeSpan.FromSeconds(qa_phaseDuration), cancellationToken: token);

        EndChapter();
    }

    public void QAPhaseVariance(float startDelay, float phaseDuration, MonsterData monster)
    {
        qa_startDelay = startDelay;
        qa_phaseDuration = phaseDuration;
        monsters[0] = monster;
    }


    // scheduled 기반 타이밍 추적용
    private double _lastScheduledDspTime = double.NaN;
    private long _lastScheduledTick = -1;

    public void SetStageTimerGoScheduled(long scheduledTick, double scheduledDspTime)
    {
        long deltaTicks;
        if (_lastScheduledTick >= 0)
        {
            deltaTicks = scheduledTick - _lastScheduledTick;
            if (deltaTicks < 1) deltaTicks = 1;
        }
        else
        {
            deltaTicks = Math.Max(1, scheduledTick);
        }

        _beatCount += deltaTicks;
        _lastScheduledTick = scheduledTick;
        _lastScheduledDspTime = scheduledDspTime;

        if (!isMonsterGoStart && _beatCount >= chapters[_chapterIdx].StartBeat)
        {
            isMonsterGoStart = true;
            GoStart().Forget(); // async UniTask 실행
        }

        float progress = (float)_beatCount / _totalBeat;
        gaugeImage.fillAmount = 1.0f - progress;
        gaugeImage.fillAmount = Mathf.Clamp01(gaugeImage.fillAmount);
    }
    #endregion

    #region SinkStage
    // IEnumerator -> async UniTask
    private async UniTask SinkStageInit(CancellationToken token)
    {
        Managers.UI.ShowPopUpUI<SinkStage_PopUp>();
        await UniTask.WaitForSeconds(0.2f);
        isSinkStage = true;
        gaugeImage.transform.parent.gameObject.SetActive(false);
        var firstPhase = chapters[_chapterIdx].Phases[0];

        // BPM 및 비트 간격 설정
        beatInterval = 60.0f / firstPhase.bpm;
        IngameData.GameBpm = (int)firstPhase.bpm;
        IngameData.BeatInterval = beatInterval;

        // 싱크 스테이지에서는 굳이 누적 틱을 계속 쌓을 필요 없이, 
        // 매 루프마다 0부터 시작하는 것이 오차 누적을 막는 데 유리합니다.

        while (!token.IsCancellationRequested)
        {
            Managers.Sound.StopBGM();

            // 1. 매 루프마다 새로운 기준 시간 설정 (현재 시간 + 1초 여유)
            //    이 1초 동안 오디오 엔진과 비트 클락을 준비시킵니다.
            double loopStartTime = AudioSettings.dspTime + 1.0;

            // 2. [핵심] BeatClock을 오디오 시간과 강제로 동기화 (RunChapter와 동일한 환경 조성)
            beatClock.SetStartTime(loopStartTime);

            // 3. 로직상(Visual) 몬스터 스폰 타이밍 계산
            //    DelayPaddingTick은 "노래를 늦게 트는" 값이므로, 상대적으로 몬스터가 먼저 나오는 효과입니다.
            //    여기서는 RunChapter와 환경을 맞추기 위해 Tick은 0부터 시작한다고 가정하고 계산합니다.

            var gameEvent = chapters[_chapterIdx].Phases[0];
            long moveBeat = 0;
            if (gameEvent is PhaseEvent phaseEvent)
            {
                // 몬스터 데이터 가져오기
                moveBeat = (long)phaseEvent.monsterDatas[0].moveBeat;

                // 몬스터 스폰 (Visual)
                // 루프 시작 시점(Tick 0)을 기준으로 Spawn하되, 
                // RunChapter와 동일하게 Extension이나 Delay가 있다면 적용
                long spawnTick = 0; // 시작하자마자 스폰 (필요시 delayBeats 추가)

                // 주의: loopStartTime이 될 때까지 대기했다가 스폰해야 정확함
                await UniTask.WaitUntil(() => AudioSettings.dspTime >= loopStartTime, cancellationToken: token);

                // 실제 스폰 실행
                SpawnMonsters(phaseEvent, (long)firstPhase.durationBeat + phaseEvent.extensionCreateBeat);
            }

            // 4. 오디오 예약 (Audio)
            //    RunChapter의 공식: StartTime + DelayPadding + SinkTimer
            //    싱크 테스트에서는 몬스터가 '판정선'에 닿을 때 '소리'가 나야 하므로
            //    몬스터 이동 시간(moveBeat)만큼 기다린 후 소리가 나도록 설정해야 할 수도 있습니다.
            //    하지만 "BGM"을 켜는 것이라면 RunChapter와 똑같이 계산해야 합니다.

            // RunChapter와 동일한 딜레이 적용 (이게 빠져서 위치가 달랐던 것)
            double delayPaddingSec = (long)chapters[_chapterIdx].DelayPaddingTick * IngameData.BeatInterval;

            // 싱크 조절 값
            double syncAdjustment = IngameData.sinkTimer;

            // 최종 예약 시간 = 기준시간 + (몬스터 이동시간 or 딜레이패딩) + 싱크보정
            // * 테스트용 짧은 음원이라면: 몬스터가 도착할 때 소리가 나야 하므로 'moveBeat' 시간 뒤에 재생
            // * 배경음악(BGM)이라면: RunChapter처럼 'delayPaddingSec'를 사용
            // 아래는 '몬스터가 도착하는 순간 소리가 나게'하여 싱크를 맞추는 일반적인 캘리브레이션 방식입니다.
            double targetAudioTime = moveBeat * IngameData.BeatInterval;

            // 만약 RunChapter와 똑같은 배경음악을 재생하는 거라면 위 targetAudioTime 대신 delayPaddingSec를 사용하세요.
            // double finalAudioTime = loopStartTime + delayPaddingSec + syncAdjustment;  // (BGM 방식)
            double finalAudioTime = loopStartTime + targetAudioTime + syncAdjustment;     // (타격음 방식)

            Managers.Sound.PlayScheduled("BGM/TestPlay", finalAudioTime, Define.Sound.BGM);


            // 5. 루프 종료 대기 (몬스터가 지나가고 다음 루프까지의 여유 시간)
            //    몬스터 이동시간 + 여유분(2비트) 만큼 대기
            long endTick = moveBeat + (long)(firstPhase.durationBeat) + 1; // 4는 여유 버퍼

            await UniTask.WaitUntil(() => beatClock._tick >= endTick, cancellationToken: token);

            // 6. 다음 루프를 위해 몬스터 정리 (필요하다면)
            // ClearMonsters(); 
        }
    }

    /// <summary>
    /// 게임 시작 전 초기화 및 안정화 로딩
    /// </summary>
    private async UniTask GameInitLoading(CancellationToken token)
    {
        // 시작 전 메모리 정리 (렉 방지)
        // 씬 로드 직후 쌓인 가비지정리
        System.GC.Collect();
        await Resources.UnloadUnusedAssets().ToUniTask(cancellationToken: token);

        // 오디오/물리 엔진이 안정화될 때까지 짧게 대기 
        //    ignoreTimeScale: true로 설정하여 퍼즈 상태에서도 흐르게 함
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: true, cancellationToken: token);

        PauseManager.ControlTime(false);

    }

    #endregion
}