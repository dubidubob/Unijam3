using System.Collections;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;

[RequireComponent(typeof(WASDMonsterSpawner))]
[RequireComponent(typeof(DiagonalMonsterSpawner))]
[RequireComponent(typeof(MouseClickMonsterSpawner))]

public class SpawnController : MonoBehaviour
{

    private Dictionary<Define.MonsterType, ISpawnable> _spawnerMap;

    private readonly Queue<ScheduledSpawn> _queue = new();
    private readonly List<SpawnTrack> _tracks = new();
    private bool _phaseRunning = false;
    private double _phaseDspEnd = 0;

    private void Awake()
    {
        InitSpawnableDic();
    }

    private void InitSpawnableDic()
    {
        var spawnables = GetComponents<ISpawnable>();
        _spawnerMap = new Dictionary<Define.MonsterType, ISpawnable>();
        foreach (var s in spawnables)
            _spawnerMap[s.MonsterType] = s;
    }

    public void BeginPhase(IReadOnlyList<MonsterData> monsterDatas, double baseBeat, double phaseDspStart, double phaseDspEnd)
    {
        EndPhase(); // 이전 것 정리

        foreach (var m in monsterDatas)
        {
            if (!m.isIn) continue;
            if (!_spawnerMap.TryGetValue(m.monsterType, out var spawner)) continue;

            double interval = baseBeat * m.speedUpRate;
            double movingToHolderTime = m.MovingToHolderTime;
            _phaseDspEnd = phaseDspEnd;

            var track = new SpawnTrack
            {
                spawner = spawner,
                interval = interval,
                movingToHolderTime = movingToHolderTime,
                // 이 트랙의 “첫 히트 타임” (절대 음악 시간)
                nextHitDsp = phaseDspStart 
                             + (m.startBeatOffset * baseBeat) // 몇 번째 박자부터 시작?
            };
            _tracks.Add(track);
        }
    }

    private void Update()
    {
        if (!_phaseRunning) return;

        double dspNow = AudioSettings.dspTime;

        // 1) lookahead 창 안으로 들어오는 스폰을 미리 큐잉
        foreach (var t in _tracks)
        {
            // 히트시각에서 leadTime 만큼 앞당긴 시각에 spawn
            while (true)
            {
                double spawnDsp = t.nextHitDsp - t.movingToHolderTime;
                if (spawnDsp > _phaseDspEnd) break; // 페이즈 끝 넘으면 중지
                if (spawnDsp <= dspNow + _lookahead)
                {
                    _queue.Enqueue(new ScheduledSpawn
                    {
                        spawner = t.spawner,
                        spawnDsp = spawnDsp,
                        leadTime = t.leadTime,
                        hitDsp = t.nextHitDsp
                    });
                    t.nextHitDsp += t.interval; // 다음 히트시각
                }
                else break;
            }
        }

        // 2) 시간이 된 이벤트 소비 (놓친 건 while로 몰아 처리)
        while (_queue.Count > 0 && _queue.Peek().spawnDsp <= dspNow)
        {
            var ev = _queue.Dequeue();
            SpawnWithCorrection(ev, dspNow);
        }

        // 3) 페이즈 종료
        if (dspNow >= _phaseDspEnd && _queue.Count == 0)
        {
            EndPhase();
        }
    }

    public void EndPhase()
    {
        _phaseRunning = false;
        _queue.Clear();
        _tracks.Clear();
    }

    // ===== 내부 구조체 =====
    private class SpawnTrack
    {
        public ISpawnable spawner;
        public double interval; // bpm*scale 반영된 히트 간격
        public double movingToHolderTime; // 이동 시간
        public double nextHitDsp; // 다음 히트(충돌/판정) 절대 시각
    }

    private struct ScheduledSpawn
    {
        public ISpawnable spawner;
        public double spawnDsp; // 실제로 풀에서 꺼내는 시각
        public double leadTime; // 이동 시간 (보정에 사용)
        public double hitDsp;   // 도착(판정) 시각
    }
}