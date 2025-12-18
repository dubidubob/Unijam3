using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask
using System.Threading; // CancellationToken

[RequireComponent(typeof(WASDMonsterSpawner))]
[RequireComponent(typeof(DiagonalMonsterSpawner))]
[RequireComponent(typeof(MouseClickMonsterSpawner))]

public class SpawnController : MonoBehaviour
{
    private Dictionary<Define.MonsterType, ISpawnable> _spawnerMap;
    private List<ISpawnable> _spawnables;

    // 이 스크립트가 파괴될 때 실행 중인 태스크를 취소하기 위한 토큰 소스
    private CancellationTokenSource _cts;

    private void Awake()
    {
        InitSpawnableDic();
        _cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 실행 중인 모든 태스크 취소
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
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
    * 몬스터를 스폰하고, 생성된 '스폰 인스턴스'의 리스트를 반환합니다.
    */
    public List<ISpawnable.ISpawnInstance> SpawnMonsterInPhase(IReadOnlyList<MonsterData> monsterDatas)
    {
        // Debug.Log($"<color=yellow>SpawnController: Received spawn request! Monster count: {monsterDatas.Count}</color>");
        List<ISpawnable.ISpawnInstance> spawnedInstances = new List<ISpawnable.ISpawnInstance>();

        if (monsterDatas == null || monsterDatas.Count == 0)
        {
            Debug.LogWarning("SpawnController: MonsterDatas is NULL or EMPTY! Cannot spawn.");
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
                // Spawn()을 호출하고 반환된 인스턴스를 리스트에 추가합니다.
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
    * 모든 스포너의 모든 스폰 작업을 중단시킵니다.
    */
    public void StopAllMonsters()
    {
        for (int i = 0; i < _spawnables.Count; i++)
        {
            _spawnables[i].UnSpawnAll();
        }
    }

    /**
    * 전달받은 특정 스폰 인스턴스들만 중단시킵니다.
    */
    public void StopSpecificInstances(List<ISpawnable.ISpawnInstance> instances)
    {
        if (instances == null) return;

        // Debug.Log($"<color=orange>Stopping {instances.Count} specific spawn instances.</color>");
        foreach (ISpawnable.ISpawnInstance instance in instances)
        {
            instance.Stop();
        }
    }

    /// <summary>
    /// 모든스포너 중단 ( 챕터 종료시만 해당 ) 
    /// </summary>
    public void StopMonsterInPhase()
    {
        StopAllMonsters();
    }

    /**
     * [UniTask]
     * 몬스터 스폰을 시작하고, 반환된 인스턴스 핸들(List)을 보관합니다.
     * targetTick에 도달하면, 보관했던 핸들(인스턴스) 만 정확히 중지시킵니다.
     * PhaseController에서 이 메서드를 호출할 때 .Forget()을 붙여서 실행하세요.
     */
    public async UniTaskVoid SpawnUntilTargetTick(IReadOnlyList<MonsterData> monsterDatas, long targetTick)
    {
        // 0. 취소 토큰 획득 (오브젝트 파괴 시 중단용)
        var token = this.GetCancellationTokenOnDestroy();

        // BeatClock 참조 검사
        BeatClock beatClock = Managers.Game.beatClock;
        if (beatClock == null)
        {
            Debug.LogError("SpawnController: BeatClock is NULL! Cannot proceed with tick-based spawn.");
            return;
        }

        // 1. 몬스터 스폰 시작 및 핸들 획득
        List<ISpawnable.ISpawnInstance> activeInstances = SpawnMonsterInPhase(monsterDatas);

        // 2. targetTick까지 대기 (비동기)
        //    WaitUntil을 사용하여 비트가 도달할 때까지 대기합니다.
        //    Cancellation이 발생하면(오브젝트 파괴 등) 자동으로 중단됩니다.
        await UniTask.WaitUntil(() => beatClock._tick >= targetTick, cancellationToken: token);

        // 3. targetTick 도달 시 해당 인스턴스들만 종료
        StopSpecificInstances(activeInstances);
    }
}