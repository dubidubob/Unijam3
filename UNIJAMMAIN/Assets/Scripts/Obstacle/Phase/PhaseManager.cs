using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase�� ������� �����Ű�� Ŭ����
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
