using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase를 순서대로 재생시키는 클래스
/// </summary>
public class PhaseManager : MonoBehaviour
{
    [SerializeField] PhaseInfo[] phases;
    SpawnController spawnController;

    private void Start()
    {
        StartCoroutine(TurnOnPhase());
    }

    private IEnumerator TurnOnPhase()
    {
        float targetTurnNextPhaseTime;
        for (int i = 0; i < phases.Length; i++)
        {
            targetTurnNextPhaseTime = phases[i].GetDuration();
            spawnController.SpawnMonsterInPhase(phases[i].MonsterDatas);
            yield return new WaitForSeconds(targetTurnNextPhaseTime);
        }
    }
}
