using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Phase를 순서대로 재생시키는 클래스
/// </summary>
[RequireComponent(typeof(SpawnController))]
public class PhaseManager : MonoBehaviour
{
    [SerializeField] IllustController illustController;
    [SerializeField] ResultUI Scoreboard;
    [SerializeField] ChapterSO chapter;
    
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
    
    // TODO : 중간중간 컷씬
    private IEnumerator ShowCutScene()
    {
        Managers.Pause.Pause();
        if (illustController != null)
        {
            Managers.UI.ShowPopUpUI<S1_PopUp>();
            yield return new WaitForSecondsRealtime(6f);
            yield return illustController.ShowIllust(GamePlayDefine.IllustType.Num);//숫자 나옴
        }
        Managers.Pause.Resume();

        //Pause();
        //if (i == 4)
        //{
        //    Managers.Scene.LoadScene("GoodEnding");
        //    break;
        //}
        //else if (i == 1)
        //{
        //    Managers.UI.ShowPopUpUI<S2_PopUp>();
        //    yield return new WaitForSecondsRealtime(8f);
        //}
        //else if (i == 3)
        //{
        //    Managers.UI.ShowPopUpUI<S3_PopUp>();
        //    yield return new WaitForSecondsRealtime(8f);
        //}
        //Resume();
    }
    private IEnumerator RunPhase()
    {
        for (int i = 0; i < chapter.Phases.Count; i++)
        {
            var phase = chapter.Phases[i];
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

        Scoreboard.ChangeUI();
        Scoreboard.gameObject.SetActive(true);
    }
}
