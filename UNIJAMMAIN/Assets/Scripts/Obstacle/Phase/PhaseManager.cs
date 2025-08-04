using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase를 순서대로 재생시키는 클래스
/// </summary>
public class PhaseManager : MonoBehaviour
{
    [SerializeField] IllustController illustController;
    [SerializeField] PhaseInfo phase;
    SpawnController spawnController;

    private void Start()
    {
        StartCoroutine(RunPhase());
    }
    
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
        for (int i = 0; i < phases.Length; i++)
        {
            yield return new WaitForSeconds(phases[i].GetStartTime());
            spawnController.SpawnMonsterInPhase(phases[i].MonsterDatas);
            yield return new WaitForSeconds(phases[i].GetDuration());
        }
        EndPhase();
    }

    private void EndPhase()
    {
        spawnController.StopMonsterInPhase();
    }
}
