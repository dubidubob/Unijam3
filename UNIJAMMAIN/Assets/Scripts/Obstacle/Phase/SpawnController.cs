using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WASDMonsterSpawner))]
[RequireComponent(typeof(DiagonalMonsterSpawner))]
[RequireComponent(typeof(MouseClickMonsterSpawner))]

public class SpawnController : MonoBehaviour
{
    private Dictionary<Define.MonsterType, ISpawnable> _spawnerMap;
    private List<ISpawnable> _spawnables;

    private void Awake()
    {
        InitSpawnableDic();
    }

    private void InitSpawnableDic()
    {
        var spawnables = GetComponents<ISpawnable>();
        _spawnerMap = new Dictionary<Define.MonsterType, ISpawnable>();
        _spawnables = new List<ISpawnable>();
        foreach (var s in spawnables)
        {
            _spawnerMap[s.MonsterType] = s;
            _spawnables.Add(s);
        }
    }

    public void SpawnMonsterInPhase(IReadOnlyList<MonsterData> monsterDatas)
    {
        Debug.Log($"<color=yellow>SpawnController: Received spawn request! Monster count in list: {monsterDatas.Count}</color>");
        if (monsterDatas == null || monsterDatas.Count == 0)
        {
            Debug.LogError("SpawnController: MonsterDatas is NULL or EMPTY! Cannot spawn.");
            return;
        }
        StopMonsterInPhase();
        
        foreach (var m in monsterDatas)
        {
            if (!m.isIn) continue;
            Debug.Log($"Processing monster type: {m.monsterType}");
            if (_spawnerMap.TryGetValue(m.monsterType, out var spawner))
            {
                spawner.Spawn(m);
            }
            else 
            {

                if (!_spawnerMap.ContainsKey(Define.MonsterType.WASD))
                {
                    Debug.LogError($"CRITICAL: Trying to use WASD spawner, but it's not registered in the _spawnerMap!");
                }
                else
                {
                    _spawnerMap[Define.MonsterType.WASD].Spawn(m);
                }

                if (m.monsterType == Define.MonsterType.Knockback)
                {
                    _spawnerMap[Define.MonsterType.WASD].Spawn(m);
                }
                else if (m.monsterType == Define.MonsterType.WASDHiding)
                {
                    _spawnerMap[Define.MonsterType.WASD].Spawn(m);
                }
                else if (m.monsterType == Define.MonsterType.WASDDash)
                {
                    _spawnerMap[Define.MonsterType.WASD].Spawn(m);
                }
                else if (m.monsterType == Define.MonsterType.WASDFIFO)
                {
                    _spawnerMap[Define.MonsterType.WASD].Spawn(m);
                }
                else
                {
                    Debug.LogWarning($"No spawner for {m.monsterType}");
                }
            }
        }
    }

    public void StopMonsterInPhase()
    {
        for (int i = 0; i < _spawnables.Count; i++)
        {
            _spawnables[i].UnSpawn();
        }
    }
}