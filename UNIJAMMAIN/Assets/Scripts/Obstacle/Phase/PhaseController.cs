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
    [SerializeField] ChapterSO[] chapters;
    // TODO : tmp!
    [SerializeField] StartMotionUIs startMotions;
    [SerializeField] SpriteRenderer areaBaseInLine;
    [SerializeField] Image gaugeImage;
   

    public static Action<float> ChangeKey;
    public static Action<bool> TutorialStoped;
    private int _chapterIdx;
    private int _lastMonsterHitCnt = 0;
    public float _totalBeat = 0;
    private float _beatCount=0;
    SpawnController spawnController;

    private bool beatSynced = false; // 비트 동기화 신호를 위한 플래그

    private bool isStart = false;
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

    private void HandleBeatSync(double _, long __)
    {
        beatSynced = true;
        BeatClock.OnBeat -= HandleBeatSync; // 한 번만 실행하고 즉시 구독 해지
    }
    private IEnumerator RunChapter()
    {
       
        SetStageBackGround(); // 배경설정
        IngameData.IsStart = true;
        for (int i = 0; i < chapters[_chapterIdx].Phases.Count; i++)
        {
            var gameEvent = chapters[_chapterIdx].Phases[i];
            if (!gameEvent.isIn) continue;

            SetTimeScale(gameEvent.timeScale); // 배속에따라 배속 설정
            

            // 공통 데이터 설정
            float beatInterval = 60.0f / gameEvent.bpm;
            float delaySec = gameEvent.startDelayBeat * beatInterval;
            float durationSec = gameEvent.durationBeat * beatInterval;

            IngameData.PhaseDurationSec = durationSec;
            IngameData.BeatInterval = beatInterval;

           

            if (gameEvent is PhaseEvent phaseEvent)
            {
                // PhaseEvent에 특화된 로직 실행

                Managers.Game.CurrentState = GameManager.GameState.Battle;
                HandleFlipKeyEvent(phaseEvent, delaySec);
                yield return new WaitForSeconds(delaySec);
                SpawnMonsters(phaseEvent);
            }
            else if (gameEvent is TutorialEvent tutorialEvent)
            {
                if (i == 0)
                    TutorialStoped?.Invoke(true);
                // TutorialEvent에 특화된 로직 실행
                Managers.Game.CurrentState = GameManager.GameState.Tutorial;
                HandleTutorialEvent(tutorialEvent);
                yield return new WaitForSeconds(delaySec);
                if (i == 0)
                    TutorialStoped?.Invoke(false);
            }
            
            if (i == 0)
            {
                beatSynced = false;
                BeatClock.OnBeat += HandleBeatSync;

                // 2. 다음 비트가 올 때까지(beatSynced가 true가 될 때까지) 기다립니다.
                yield return new WaitUntil(() => beatSynced);

                // 3. 비트에 정확히 맞춰진 시점에서 카운트다운을 시작합니다.
                yield return StartCoroutine(startMotions.startCountUI.Play123Coroutine());
                startMotions.playerActionUI.StartMonkAnimAfter123Count();
            }
            yield return new WaitForSeconds(durationSec);
        }

        yield return new WaitForSeconds((float)IngameData.BeatInterval*2);
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
        UI_Popup go = Managers.UI.ShowPopUpUI<Tutorial_PopUp>();
        var tuto = go.GetComponent<Tutorial_PopUp>();
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
        float total = totalCnt + missedInput;

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
        _beatCount++;

        if (!isStart)
        {
            Managers.Sound.Play(chapters[_chapterIdx].MusicPath, Define.Sound.BGM);
            isStart = true;
        }

        // 1. _beatCount와 totalCount를 float으로 변환하여 진행 비율(0.0 ~ 1.0)을 계산합니다.
        // (float)을 붙이지 않으면 정수 나눗셈이 되어 결과가 0 또는 1만 나오게 됩니다.
        float progress = (float)_beatCount / _totalBeat;

        // 2. 1에서 진행 비율을 빼서 값을 뒤집어 줍니다. (1.0 -> 0.0)
        gaugeImage.fillAmount = 1.0f - progress;

        // (옵션) _beatCount가 totalCount를 넘어가지 않도록 값을 보정해줄 수 있습니다.
        gaugeImage.fillAmount = Mathf.Clamp01(gaugeImage.fillAmount);
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

    
    #endregion
}
