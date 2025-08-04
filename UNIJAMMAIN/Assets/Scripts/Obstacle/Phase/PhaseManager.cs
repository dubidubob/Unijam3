using System.Collections;
using UnityEngine;

/// <summary>
/// Phase�� ������� �����Ű�� Ŭ����
/// </summary>
[RequireComponent(typeof(SpawnController))]
public class PhaseManager : MonoBehaviour
{
    [SerializeField] IllustController illustController;
    [SerializeField] ChapterSO chapter;
    SpawnController spawnController;

    private void Start()
    {
        spawnController = GetComponent<SpawnController>();
        StartCoroutine(RunPhase());
    }
    
    // TODO : �߰��߰� �ƾ�
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
    private IEnumerator RunPhase()
    {
        for (int i = 0; i < chapter.Phases.Count; i++)
        {
            yield return new WaitForSeconds(chapter.Phases[i].startDelay);
            spawnController.SpawnMonsterInPhase(chapter.Phases[i].MonsterDatas);
            yield return new WaitForSeconds(chapter.Phases[i].duration);
        }
        EndPhase();
    }

    private void EndPhase()
    {
        spawnController.StopMonsterInPhase();
    }
}
