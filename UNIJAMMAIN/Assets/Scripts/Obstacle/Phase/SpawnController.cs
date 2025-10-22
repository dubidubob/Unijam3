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
    * [������]
    * ���͸� �����ϰ�, ������ '���� �ν��Ͻ�'�� ����Ʈ�� ��ȯ�մϴ�.
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
                // WASD �����ʷ� ��ü(Fallback)
                _spawnerMap.TryGetValue(Define.MonsterType.WASD, out spawner);
            }

            if (spawner != null)
            {
                // [����] Spawn()�� ȣ���ϰ� ��ȯ�� �ν��Ͻ��� ����Ʈ�� �߰��մϴ�.
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
        return spawnedInstances; // �� ������� ������ ��� ���� �ν��Ͻ� �ڵ� ��ȯ
    }

    /**
    * [�̸� ���� �� ����]
    * ��� �������� ��� ���� �۾��� �ߴܽ�ŵ�ϴ�. (���� StopMonsterInPhase)
    */
    public void StopAllMonsters()
    {
        for (int i = 0; i < _spawnables.Count; i++)
        {
            _spawnables[i].UnSpawnAll(); // [����] UnSpawnAll() ȣ��
        }
    }

    /**
    * [NEW]
    * ���޹��� Ư�� ���� �ν��Ͻ��鸸 �ߴܽ�ŵ�ϴ�.
    * (PhaseController�� SpawnUntilTargetTick �ڷ�ƾ���� ���˴ϴ�.)
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
    * [���ŵ�]
    * public void StopSpecificSpawners(HashSet<ISpawnable> spawnersToStop)
    * {
    * // ...
    * }
    *
    * -> �� �޼���� �� �̻� �ʿ����� ������, ISpanwable.UnSpawn() �޼��尡
    * ��������Ƿ� ������ ������ �߻��մϴ�.
    * 'StopSpecificInstances'�� �� ������ ����մϴ�.
    */


    /// <summary>
    /// ��罺���� �ߴ� ( é�� ����ø� �ش� ) 
    /// </summary>
    public void StopMonsterInPhase()
    {
        StopAllMonsters();
    }

    /**
    * [RE-ADDED]
     * ���� ������ �����ϰ�, ��ȯ�� �ν��Ͻ� �ڵ�(List)�� �����մϴ�.
     * targetTick�� �����ϸ�, �����ߴ� �ڵ�(�ν��Ͻ�) �� ��Ȯ�� ������ŵ�ϴ�.
     */
    public IEnumerator SpawnUntilTargetTick(IReadOnlyList<MonsterData> monsterDatas, long targetTick)
    {
        // BeatClock ������ �����ɴϴ�. (Managers.Game.beatClock�� null�� �ƴϾ�� ��)
        BeatClock beatClock = Managers.Game.beatClock;
        if (beatClock == null)
        {
            Debug.LogError("SpawnController: BeatClock is NULL! Cannot proceed with tick-based spawn.");
            yield break;
        }

        // 1. (��û����) ���� ������ "�߰�"�ϰ�, �� �۾��� �ڵ�(�ν��Ͻ�) ����� �޽��ϴ�.
        //    ���� ���Ϳ� �����ϰ� �˴ϴ�.
        List<ISpawnable.ISpawnInstance> activeInstances = SpawnMonsterInPhase(monsterDatas);

        // 2. �� ����� ������ �ϴ� targetTick���� ��ٸ��ϴ�.
        yield return new WaitUntil(() => beatClock._tick >= targetTick);

        // 3. (��û����) targetTick�� �����߽��ϴ�.
        //    ���� *�� �ڷ�ƾ(������)�� �����ߴ� ���͵鸸* �ߴܽ�ŵ�ϴ�.
        Debug.Log($"<color=orange>Target tick {targetTick} reached. Stopping monsters from this phase.</color>");
        StopSpecificInstances(activeInstances);
    }

}