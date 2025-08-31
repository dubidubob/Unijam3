using System;
using System.Collections;
using System.Linq;
using UnityEngine;

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
    [SerializeField] ResultUI Scoreboard;
    [SerializeField] ChapterSO[] chapters;
    // TODO : tmp!
    [SerializeField] StartMotionUIs startMotions;

    public static Action<float> ChangeKey;
    private int ChapterIdx;

    SpawnController spawnController;
    bool isQA = false;
    private void Start()
    {
        spawnController = GetComponent<SpawnController>();
        Scoreboard.gameObject.SetActive(false);
        
        IngameData.RankInit();

        ChapterIdx = Mathf.Min(IngameData.ChapterIdx, chapters.Count() - 1);
        if (!isQA)
        {
            Managers.Sound.Play(chapters[ChapterIdx].MusicPath, Define.Sound.BGM);
            StartCoroutine(RunChapter());
        }
    }
    
    private IEnumerator RunChapter()
    {
        for (int i = 0; i < chapters[ChapterIdx].Phases.Count; i++)
        {
            var gameEvent = chapters[ChapterIdx].Phases[i];

            // 공통 데이터 설정
            IngameData.BeatInterval = 60.0 / gameEvent.bpm;
            float delaySec = gameEvent.startDelayBeat * (float)IngameData.BeatInterval;
            float durationSec = gameEvent.durationBeat * (float)IngameData.BeatInterval;
            IngameData.PhaseDurationSec = durationSec;

            if (gameEvent is PhaseEvent phaseEvent)
            {
                // PhaseEvent에 특화된 로직 실행
                HandleFlipKeyEvent(phaseEvent, delaySec);
                yield return new WaitForSeconds(delaySec);
                SpawnMonsters(phaseEvent);
            }
            else if (gameEvent is TutorialEvent tutorialEvent)
            {
                // TutorialEvent에 특화된 로직 실행
                HandleTutorialEvent(tutorialEvent);
                yield return new WaitForSeconds(delaySec);
            }

            if (i == 0)
            {
                durationSec += 1.7f + 4 * (float)IngameData.BeatInterval;
                startMotions.startCountUI.Start123();
                startMotions.playerActionUI.StartMonkAnimAfter123Count();
            }

            yield return new WaitForSeconds(durationSec);
        }

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
        tuto.StartTutorial(tutorialEvent.Steps);
    }

    private void EndChapter()
    {
        // TODO : 더 정돈하기
        Managers.Sound.StopBGM();
        spawnController.StopMonsterInPhase();
        IngameData.Pause = true;

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

        return (rate / total) * 100f;
    }

    #region QA
    float qa_startDelay, qa_phaseDuration;
    MonsterData[] monsters = new MonsterData[1];
    public void Play()
    {
        if (!isQA) return;
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
