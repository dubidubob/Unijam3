using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WASDMonsterSpawner))]
[RequireComponent(typeof(DiagonalMonsterSpawner))]
[RequireComponent(typeof(MouseClickMonsterSpawner))]

public class SpawnController : MonoBehaviour
{
    private Dictionary<Define.MonsterType, ISpawnable> _spawnerMap;
    
    private void Awake()
    {
        InitSpawnableDic();
    }

    private void InitSpawnableDic()
    {
        ISpawnable[] spawnables = GetComponents<ISpawnable>();
        foreach (var s in spawnables)
        {
            _spawnerMap[s.MonsterType] = s;
        }
    }

    public void SpawnMonsterInPhase(IReadOnlyList<MonsterData> monsterDatas)
    {
        foreach (var m in monsterDatas)
        {
            if (_spawnerMap.TryGetValue(m.monsterType, out var spawner))
            {
                Spawn(spawner, m);
            }
            else 
            {
                Debug.LogWarning($"No spawner for {m.monsterType}");
            }
        }
    }

    private IEnumerator Spawn(ISpawnable spawner, MonsterData monsterData)
    {

        while (true)
        {
            yield return new WaitForSeconds(monsterData.GetSpawnDuration());
            spawner.Spawn(monsterData);
        }
    }

    public void StopMonsterInPhase()
    {
        StopAllCoroutines();
    }
}