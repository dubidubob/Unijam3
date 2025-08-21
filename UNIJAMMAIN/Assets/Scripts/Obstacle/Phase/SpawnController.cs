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
        var spawnables = GetComponents<ISpawnable>();
        _spawnerMap = new Dictionary<Define.MonsterType, ISpawnable>();
        foreach (var s in spawnables)
        {
            _spawnerMap[s.MonsterType] = s;
        }
    }

    public void SpawnMonsterInPhase(IReadOnlyList<MonsterData> monsterDatas)
    {
        StopMonsterInPhase();
        
        foreach (var m in monsterDatas)
        {
            if (!m.isIn) continue;
            if (_spawnerMap.TryGetValue(m.monsterType, out var spawner))
            {
                StartCoroutine(Spawn(spawner, m));
            }
            else 
            {                
                if (m.monsterType == Define.MonsterType.Knockback)
                {
                    StartCoroutine(Spawn(_spawnerMap[Define.MonsterType.WASD], m));
                }
                else 
                {
                    Debug.LogWarning($"No spawner for {m.monsterType}");
                }
            }
        }
    }

    private IEnumerator Spawn(ISpawnable spawner, MonsterData monsterData)
    {
        while (true)
        {
            float spawnDuration = (float)IngameData.BeatInterval * monsterData.spawnBeat;

            yield return new WaitForSeconds(spawnDuration);

            spawner.Spawn(monsterData);
        }
    }

    public void StopMonsterInPhase()
    {
        StopAllCoroutines();
    }
}