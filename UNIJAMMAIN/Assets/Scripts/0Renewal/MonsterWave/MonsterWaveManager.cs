using System;
using System.Collections;
using UnityEngine;

public class MonsterWaveManager : MonoBehaviour
{
    [SerializeField] private BaseMonsterSpawner[] monsterWaves;

    private float elapsedTime = 0f;
    
    private void Start()
    {
        SortByStartTime();
        DeactivateAllWaves();
        StartCoroutine(StartWavesRoutine());
    }

    private void SortByStartTime()
    {
        if (monsterWaves?.Length > 1)
            Array.Sort(monsterWaves, (a, b) => a.AppearTiming.CompareTo(b.AppearTiming));
    }

    private void DeactivateAllWaves()
    {
        foreach (var wave in monsterWaves)
            wave.enabled = false;
    }

    private IEnumerator StartWavesRoutine()
    {
        float lastTime = 0f;
        foreach(var wave in monsterWaves)
        {
            float delay = Mathf.Max(0f, wave.AppearTiming - lastTime);
            yield return new WaitForSeconds(delay);
            wave.enabled = true;
            lastTime = wave.AppearTiming;
        }
    }
}
