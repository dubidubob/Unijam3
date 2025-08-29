using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Phase를 순서대로 재생시키는 클래스
/// </summary>
[RequireComponent(typeof(SpawnController))]
public class PhaseManager : MonoBehaviour
{
    [SerializeField] IllustController illustController;
    [SerializeField] ResultUI Scoreboard;
    [SerializeField] ChapterSO[] chapters;
    
    public static Action<float> ChangeKey;

    SpawnController spawnController;
    bool isQA = false;
    private void Start()
    {
        spawnController = GetComponent<SpawnController>();
        Scoreboard.gameObject.SetActive(false);
        
        IngameData.RankInit();
        if (!isQA)
        {
            StartCoroutine(RunPhase());
        }
    }
    
    private IEnumerator RunPhase()
    {
        int idx = Mathf.Min(IngameData.ChapterIdx, chapters.Count()-1);
        for (int i = 0; i < chapters[idx].Phases.Count; i++)
        {
            var phase = chapters[idx].Phases[i];
            if (phase.isFlipAD)
            {
                Managers.Game.SetADReverse(true);
                ChangeKey?.Invoke(phase.startDelay);
            }
            else
            {
                Managers.Game.SetADReverse(false);
                ChangeKey?.Invoke(-1f);
            }
            yield return new WaitForSeconds(phase.startDelay);
            IngameData.BeatInterval = 60.0/ phase.bpm;
            IngameData.PhaseDuration = phase.duration;
            spawnController.SpawnMonsterInPhase(phase.MonsterDatas);
            yield return new WaitForSeconds(phase.duration);
        }
        EndPhase();
    }

    float qa_startDelay, qa_phaseDuration;
    MonsterData[] monsters = new MonsterData[1];
    private IEnumerator RunQAPhase()
    {
        yield return new WaitForSeconds(qa_startDelay);
        spawnController.SpawnMonsterInPhase(monsters);
        yield return new WaitForSeconds(qa_phaseDuration);
        EndPhase();
    }

    public void QAPhaseVariance(float startDelay, float phaseDuration, MonsterData monster)
    {
        qa_startDelay = startDelay;
        qa_phaseDuration = phaseDuration;
        monsters[0] = monster;
    }

    public void Play()
    {
        if (!isQA) return;
        StopAllCoroutines();
        StartCoroutine(RunQAPhase());
    }
    private void EndPhase()
    {
        // TODO : 더 정돈하기
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
}
