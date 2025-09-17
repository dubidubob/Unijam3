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
    [SerializeField] Image backGround;
    [SerializeField] Image backGroundGray;
    [SerializeField] ResultUI Scoreboard;
    [SerializeField] ChapterSO[] chapters;
    // TODO : tmp!
    [SerializeField] StartMotionUIs startMotions;
    [SerializeField] SpriteRenderer areaBaseInLine;

    public static Action<float> ChangeKey;
    public static Action<bool> TutorialStoped;
    private int _chapterIdx;
    private int _lastMonsterHitCnt = 0;

    SpawnController spawnController;
    private void Start()
    {
        spawnController = GetComponent<SpawnController>();
        Scoreboard.gameObject.SetActive(false);
        
        IngameData.RankInit();

        PauseManager.ControlTime(false);

        _chapterIdx = Mathf.Min(IngameData.ChapterIdx, chapters.Count() - 1);
        Debug.Log(_chapterIdx);
        Managers.Sound.Play(chapters[_chapterIdx].MusicPath, Define.Sound.BGM);
        Debug.Log(chapters[_chapterIdx].MusicPath);
        Color tmpColor = chapters[_chapterIdx].colorPalette;
        tmpColor.a = 0.7f;
        areaBaseInLine.color = tmpColor;

        StartCoroutine(RunChapter());
    }
    
    private IEnumerator RunChapter()
    {
        SetStageBackGround(); // 배경설정
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
                HandleFlipKeyEvent(phaseEvent, delaySec);
                yield return new WaitForSeconds(delaySec);
                SpawnMonsters(phaseEvent);
            }
            else if (gameEvent is TutorialEvent tutorialEvent)
            {
                if (i == 0)
                    TutorialStoped?.Invoke(true);
                // TutorialEvent에 특화된 로직 실행
                HandleTutorialEvent(tutorialEvent);
                yield return new WaitForSeconds(delaySec);
                if (i == 0)
                    TutorialStoped?.Invoke(false);
            }

            if (i == 0)
            {
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

    #region Tool

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
