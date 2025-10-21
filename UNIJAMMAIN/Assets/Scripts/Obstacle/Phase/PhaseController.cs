using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private MonsterDatabaseSO monsterDatabase;

    [SerializeField] Image backGround;
    [SerializeField] Image backGroundGray;
    [SerializeField] ResultUI Scoreboard;
    [SerializeField] BeatClock beatClock;
    [SerializeField] UI_Popup tutorialPopUp;
    
    [SerializeField] public ChapterSO[] chapters;
    // TODO : tmp!
    [SerializeField] StartMotionUIs startMotions;
    [SerializeField] SpriteRenderer areaBaseInLine;
    [SerializeField] Image gaugeImage;
    

  
    public static Action<float> ChangeKey;
    public static Action<bool> TutorialStoped;
    public int _chapterIdx;
    private int _lastMonsterHitCnt = 0;
    public float _totalBeat = 0;
    private float _beatCount=0;
    SpawnController spawnController;

    private bool beatSynced = false; // 비트 동기화 신호를 위한 플래그
    private bool isMonsterGoStart = false;

    float beatInterval;
    float delaySec;
    float durationSec;
    
    private void Start()
    {
        IngameData.IsStart = false;
        spawnController = GetComponent<SpawnController>();
        Scoreboard.gameObject.SetActive(false);
        monsterDatabase.Init();
        

        IngameData.RankInit();
        Managers.Game.monster = monsterDatabase;
        Managers.Game.GameStart();
        

        PauseManager.ControlTime(false);

        _chapterIdx = Mathf.Min(IngameData.ChapterIdx, chapters.Count() - 1);
        Debug.Log(_chapterIdx);
        
        Debug.Log(chapters[_chapterIdx].MusicPath);
        Color tmpColor = chapters[_chapterIdx].colorPalette;
        tmpColor.a = 0.7f;
        areaBaseInLine.color = tmpColor;

        SetStageTimerInitialize();
        StartCoroutine(RunChapter());
    }
    private void HandleBeatSync(long __)
    {
        beatSynced = true;
        BeatClock.OnBeat -= HandleBeatSync; // 한 번만 실행하고 즉시 구독 해지
    }
    private IEnumerator RunChapter()
    {
        SetStageBackGround();
        IngameData.IsStart = true;

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

        // 코루틴 내에서 진행될 이벤트의 목표 틱(Beat)을 관리하는 변수
        long targetTick = 0;

        for (int i = 0; i < chapters[_chapterIdx].Phases.Count; i++)
        {
            var gameEvent = chapters[_chapterIdx].Phases[i];
            if (!gameEvent.isIn) continue;

            float durationSec = gameEvent.durationBeat * beatInterval;
            IngameData.PhaseDurationSec = durationSec;

            SetTimeScale(gameEvent.timeScale);

            // --- 1. Delay 구간 처리 ---
            long delayBeats = Mathf.RoundToInt(gameEvent.startDelayBeat);

            // [기존 로직 복원] Delay가 시작되기 전에 필요한 이벤트들을 먼저 호출합니다.
            if (gameEvent is PhaseEvent phaseEvent)
            {
                Managers.Game.CurrentState = GameManager.GameState.Battle;
                // HandleFlipKeyEvent가 delay 시간을 필요로 하므로, 비트 기반으로 다시 계산해줍니다.
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

            // 이제 Delay 시간만큼 비트를 누적하고 기다립니다.
            targetTick += delayBeats;
            yield return new WaitUntil(() => beatClock._tick >= targetTick); // WaitForSeconds(delaySec) 대체

            // [기존 로직 복원] Delay가 끝난 직후에 실행되어야 할 로직들을 호출합니다.
            if (gameEvent is PhaseEvent phaseEventAfterDelay)
            {
                Debug.Log($"<color=cyan>PhaseEvent [{i}] Identified! Attempting to spawn monsters. MonsterData count: {phaseEventAfterDelay.MonsterDatas.Count}</color>");
                SpawnMonsters(phaseEventAfterDelay);
            }
            else if (gameEvent is TutorialEvent tutorialEventAfterDelay)
            {
                if (i == 0)
                    TutorialStoped?.Invoke(false);
            }

            // --- 2. Duration 구간 처리 ---
            long durationBeats = Mathf.RoundToInt(gameEvent.durationBeat);
            targetTick += durationBeats;
            // 미리 생성 가능하게끔
            if (i + 1 < chapters[_chapterIdx].Phases.Count)
            {
                var next = chapters[_chapterIdx].Phases[i + 1];
                if (next.bpm == chapters[_chapterIdx].Phases[i].bpm)
                {
                    targetTick -= (int)next.preGenerateBeat;
                }
            }
            yield return new WaitUntil(() => beatClock._tick >= targetTick); // WaitForSeconds(durationSec) 대체

            // --- 3. 다음 페이즈 준비 ---
            if (i + 1 < chapters[_chapterIdx].Phases.Count)
            {
                var nextPhase = chapters[_chapterIdx].Phases[i + 1];
                // 다음 페이즈의 BPM으로 BeatClock을 업데이트하도록 신호를 보냅니다.
                if(!(nextPhase.bpm==chapters[_chapterIdx].Phases[i].bpm))
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

        // 모든 페이즈가 끝난 후, 2비트만큼 여유를 두고 챕터를 종료합니다.
        targetTick += 2;
        yield return new WaitUntil(() => beatClock._tick >= targetTick); // WaitForSeconds(...) 대체

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
    private void SpawnMonsters(PhaseEvent phaseEvent)
    {        
        spawnController.SpawnMonsterInPhase(phaseEvent.MonsterDatas);
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

        Scoreboard.ChangeUI(CalculateScore());
        Scoreboard.gameObject.SetActive(true);
    }

    private float perfectWeight = 1.0f;
    private float goodWeight = 0.5f;
    private float CalculateScore()
    {
        float perfectCnt = IngameData.PerfectMobCnt;
        float goodCnt = IngameData.GoodMobCnt;
        float rate = (perfectCnt * perfectWeight + goodCnt * goodWeight);

        float totalCnt = IngameData.TotalMobCnt;
        float missedInput = IngameData.WrongInputCnt;
        //float total = totalCnt + missedInput;

        // miss 자체에 콤보 단절, HP 감소가 있기 때문에, 일단 분모에 missedInput은 뺏음
        // 넣는다면, playerState가 Ready 상태일 때 누르는 miss 값들은 분모에 들어가지 않게 해야함.

        float total = totalCnt;

        SetStageIndex(_chapterIdx);

        return (rate / total) * 100f;
    }

    #region Setting

    private void SetTimeScale(float time)
    {
        Managers.Sound.ChangeBGMPitch(time);
        Time.timeScale = time;
    }

    private void SetStageIndex(int index)
    {
        Managers.Game.GameStage = Mathf.Max(Managers.Game.GameStage,index+1);
    }
    private void SetStageBackGround()
    {
        backGround.overrideSprite = chapters[_chapterIdx].backGroundSprite;
        backGroundGray.overrideSprite = chapters[_chapterIdx].backGroundGraySprite;
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
        // 호환성 유지: BeatClock이 아직 인자로 안 준다면 기존 동작을 하게 함
        // (이 경우는 단순히 1틱으로 처리)
        double now = AudioSettings.dspTime;
        long inferredTick = _lastScheduledTick >= 0 ? _lastScheduledTick + 1 : (_lastScheduledTick = 1);
        SetStageTimerGoScheduled(inferredTick, now);
    }


    public IEnumerator GoStart()
    {
        beatSynced = false;
        BeatClock.OnBeat += HandleBeatSync;
        yield return new WaitUntil(() => beatSynced);

        yield return StartCoroutine(startMotions.startCountUI.Play123Coroutine());
        startMotions.playerActionUI.StartMonkAnimAfter123Count();

    }
    #endregion

    #region QA
    float qa_startDelay, qa_phaseDuration;
    MonsterData[] monsters = new MonsterData[1];
    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(RunQAPhase());
    }
    private IEnumerator RunQAPhase()
    {
        yield return new WaitForSeconds(qa_startDelay);
        spawnController.SpawnMonsterInPhase(monsters);
        yield return new WaitForSeconds(qa_phaseDuration);
        EndChapter();
    }

    public void QAPhaseVariance(float startDelay, float phaseDuration, MonsterData monster)
    {
        qa_startDelay = startDelay;
        qa_phaseDuration = phaseDuration;
        monsters[0] = monster;
    }


    // 추가한 필드: scheduled 기반 타이밍 추적용
    private double _lastScheduledDspTime = double.NaN;
    private long _lastScheduledTick = -1;

    public void SetStageTimerGoScheduled(long scheduledTick, double scheduledDspTime)
    {
        // 1) 몇 틱이 지났는지 계산
        long deltaTicks;
        if (_lastScheduledTick >= 0)
        {
            deltaTicks = scheduledTick - _lastScheduledTick;
            if (deltaTicks < 1) deltaTicks = 1; // 최소 1틱은 진행된 것으로 간주
        }
        else
        {
            // 첫 호출: scheduledTick 자체를 기준으로 삼아 증분을 계산하거나 1로 처리
            // 만약 scheduledTick==0이라면 1로 올리기
            deltaTicks = Math.Max(1, scheduledTick);
        }

        // 2) 업데이트 (가볍게)
        _beatCount += deltaTicks;
        _lastScheduledTick = scheduledTick;
        _lastScheduledDspTime = scheduledDspTime;

        // 3) 기존 SetStageTimerGo의 내부 로직(스타트判定, 게이지 등)을 그대로 적용하되
        // deltaTicks > 1인 경우에도 적절히 처리하도록 함.
        // 스타트 비트 실행
        if (!isMonsterGoStart && _beatCount >= chapters[_chapterIdx].StartBeat)
        {
            isMonsterGoStart = true;
            StartCoroutine(GoStart());
        }

        // 진행도 계산 (gauge)
        float progress = (float)_beatCount / _totalBeat;
        gaugeImage.fillAmount = 1.0f - progress;
        gaugeImage.fillAmount = Mathf.Clamp01(gaugeImage.fillAmount);

        // (선택) 필요하면 여기서 다른 lightweight 콜백 호출
        // e.g. ChangeKey?.Invoke(...) 같은 것들이 있다면 아주 가볍게 호출 가능
    }


    #endregion
}
