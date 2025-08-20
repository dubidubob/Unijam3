using System.Collections;
using UnityEngine;

/// <summary>
/// Phase를 순서대로 재생시키는 클래스
/// </summary>
[RequireComponent(typeof(SpawnController))]
public class PhaseManager : MonoBehaviour
{
    [SerializeField] ChapterSO chapter;
    [SerializeField] AudioSource music;

    SpawnController spawnController;

    [SerializeField] double startDelay = 0.2f; // 오디오 파이프라인 워밍업
    public double DspSongStart { get; private set; }


    private void Start()
    {
        spawnController = GetComponent<SpawnController>();
        if (!isQA)
        {
            SetMusicStartTime();
            StartCoroutine(RunPhases());
        }
    }

    // 음악 시작 시간 설정 + 음악 Play
    private void SetMusicStartTime()
    {
        double now = AudioSettings.dspTime;
        DspSongStart = now + startDelay;
        music.PlayScheduled(DspSongStart);

        StartCoroutine(RunPhases());
    }

    private IEnumerator RunPhases()
    {
        for (int i = 0; i < chapter.Phases.Count; i++)
        {
            var phase = chapter.Phases[i];

            yield return new WaitForSeconds(phase.startDelay);
            spawnController.SpawnMonsterInPhase(phase.MonsterDatas);
            yield return new WaitForSeconds(phase.duration);
        }

        spawnController.StopMonsterInPhase();
    }

    #region QA
    float qa_startDelay, qa_phaseDuration;
    MonsterData[] monsters = new MonsterData[1];
    private IEnumerator RunQAPhase()
    {
        yield return new WaitForSeconds(qa_startDelay);
        spawnController.SpawnMonsterInPhase(monsters);
        yield return new WaitForSeconds(qa_phaseDuration);
        spawnController.StopMonsterInPhase();
    }

    [SerializeField] bool isQA = false;
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
    #endregion
    #region CutScene
    // TODO : 중간중간 컷씬
    [SerializeField] IllustController illustController;
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
    #endregion
}
