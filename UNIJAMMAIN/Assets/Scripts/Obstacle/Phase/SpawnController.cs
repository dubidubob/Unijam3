using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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

    /**
    * [수정됨]
    * 몬스터를 스폰하고, 생성된 '스폰 인스턴스'의 리스트를 반환합니다.
    */
    public List<ISpawnable.ISpawnInstance> SpawnMonsterInPhase(IReadOnlyList<MonsterData> monsterDatas)
    {
        Debug.Log($"<color=yellow>SpawnController: Received spawn request! Monster count: {monsterDatas.Count}</color>");
        List<ISpawnable.ISpawnInstance> spawnedInstances = new List<ISpawnable.ISpawnInstance>();

        if (monsterDatas == null || monsterDatas.Count == 0)
        {
            Debug.LogError("SpawnController: MonsterDatas is NULL or EMPTY! Cannot spawn.");
            return spawnedInstances;
        }

        foreach (var m in monsterDatas)
        {
            if (!m.isIn) continue;

            ISpawnable spawner = null;
            if (_spawnerMap.TryGetValue(m.monsterType, out var directSpawner))
            {
                spawner = directSpawner;
            }
            else if (m.monsterType == Define.MonsterType.Knockback ||
                     m.monsterType == Define.MonsterType.WASDHiding ||
                     m.monsterType == Define.MonsterType.WASDDash ||
                     m.monsterType == Define.MonsterType.WASDFIFO)
            {
                // WASD 스포너로 대체(Fallback)
                _spawnerMap.TryGetValue(Define.MonsterType.WASD, out spawner);
            }

            if (spawner != null)
            {
                // [수정] Spawn()을 호출하고 반환된 인스턴스를 리스트에 추가합니다.
                ISpawnable.ISpawnInstance instance = spawner.Spawn(m);
                if (instance != null)
                {
                    spawnedInstances.Add(instance);
                }
            }
            else
            {
                Debug.LogWarning($"No spawner or fallback for {m.monsterType}");
            }
        }
        return spawnedInstances; // 이 페이즈에서 생성된 모든 스폰 인스턴스 핸들 반환
    }

    /**
    * [이름 변경 및 수정]
    * 모든 스포너의 모든 스폰 작업을 중단시킵니다. (기존 StopMonsterInPhase)
    */
    public void StopAllMonsters()
    {
        for (int i = 0; i < _spawnables.Count; i++)
        {
            _spawnables[i].UnSpawnAll(); // [수정] UnSpawnAll() 호출
        }
    }

    /**
    * [NEW]
    * 전달받은 특정 스폰 인스턴스들만 중단시킵니다.
    * (PhaseController의 SpawnUntilTargetTick 코루틴에서 사용됩니다.)
    */
    public void StopSpecificInstances(List<ISpawnable.ISpawnInstance> instances)
    {
        if (instances == null) return;

        Debug.Log($"<color=orange>Stopping {instances.Count} specific spawn instances.</color>");
        foreach (ISpawnable.ISpawnInstance instance in instances)
        {
            instance.Stop();
        }
    }


    /**
    * [제거됨]
    * public void StopSpecificSpawners(HashSet<ISpawnable> spawnersToStop)
    * {
    * // ...
    * }
    *
    * -> 이 메서드는 더 이상 필요하지 않으며, ISpanwable.UnSpawn() 메서드가
    * 사라졌으므로 컴파일 오류가 발생합니다.
    * 'StopSpecificInstances'가 이 역할을 대신합니다.
    */


    /// <summary>
    /// 모든스포너 중단 ( 챕터 종료시만 해당 ) 
    /// </summary>
    public void StopMonsterInPhase()
    {
        StopAllMonsters();
    }

    /**
    * [RE-ADDED]
     * 몬스터 스폰을 시작하고, 반환된 인스턴스 핸들(List)을 보관합니다.
     * targetTick에 도달하면, 보관했던 핸들(인스턴스) 만 정확히 중지시킵니다.
     */
    public IEnumerator SpawnUntilTargetTick(IReadOnlyList<MonsterData> monsterDatas, long targetTick)
    {
        // BeatClock 참조를 가져옵니다. (Managers.Game.beatClock이 null이 아니어야 함)
        BeatClock beatClock = Managers.Game.beatClock;
        if (beatClock == null)
        {
            Debug.LogError("SpawnController: BeatClock is NULL! Cannot proceed with tick-based spawn.");
            yield break;
        }

        // 1. (요청사항) 몬스터 스폰을 "추가"하고, 이 작업의 핸들(인스턴스) 목록을 받습니다.
        //    이전 몬스터와 공존하게 됩니다.
        List<ISpawnable.ISpawnInstance> activeInstances = SpawnMonsterInPhase(monsterDatas);

        // 2. 이 페이즈가 끝나야 하는 targetTick까지 기다립니다.
        yield return new WaitUntil(() => beatClock._tick >= targetTick);

        // 3. (요청사항) targetTick에 도달했습니다.
        //    이제 *이 코루틴(페이즈)이 스폰했던 몬스터들만* 중단시킵니다.
        Debug.Log($"<color=orange>Target tick {targetTick} reached. Stopping monsters from this phase.</color>");
        StopSpecificInstances(activeInstances);
    }

}