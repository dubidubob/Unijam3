using System.Collections;
using UnityEngine;

/// <summary>
/// Phase�� ������� �����Ű�� Ŭ����
/// </summary>
[RequireComponent(typeof(SpawnController))]
public class PhaseManager : MonoBehaviour
{
    [SerializeField] ChapterSO chapter;
    [SerializeField] AudioSource music;

    SpawnController spawnController;

    [SerializeField] double startDelay = 0.2f; // ����� ���������� ���־�
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

    // ���� ���� �ð� ���� + ���� Play
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
    // TODO : �߰��߰� �ƾ�
    [SerializeField] IllustController illustController;
    private IEnumerator ShowCutScene()
    {
        Managers.Pause.Pause();
        if (illustController != null)
        {
            Managers.UI.ShowPopUpUI<S1_PopUp>();
            yield return new WaitForSecondsRealtime(6f);
            yield return illustController.ShowIllust(GamePlayDefine.IllustType.Num);//���� ����
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
