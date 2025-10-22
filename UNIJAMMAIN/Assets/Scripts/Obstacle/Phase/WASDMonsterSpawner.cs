﻿using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

public class WASDMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] WASDPosition[] positions;
    [SerializeField] Vector2 sizeDiffRate = new Vector2(0.8f, 1.2f);

    [SerializeField] Collider2D holder;

    public BeatClock beatClock;
    private Dictionary<WASDType, Vector2> _spawnPosition;
    private Dictionary<WASDType, Vector2> _targetPosition;
    private HitJudge _rank;
    private Vector3 _playerPos;

    // [MOVED] 패턴별 상태 변수들이 WASDPatternInstance로 이동했습니다.
    // (e.g., _data, _tick, _spawning, _startDsp, _lastSpawnTime, _spawnPointString, _count, _pauseStartTime)

    // [NEW] 현재 활성화된 모든 스폰 패턴(페이지)을 관리하는 리스트
    private List<WASDPatternInstance> _activePatterns = new List<WASDPatternInstance>();
    // [NEW] 리스트 순회 중 안전하게 제거하기 위한 목록
    private List<WASDPatternInstance> _patternsToRemove = new List<WASDPatternInstance>();

    // [MOVED] QA용 변수들은 스포너에 남아있되, 자식 인스턴스가 참조할 수 있도록 getter 제공
    [SerializeField] private int _maxCnt = 1;
    [SerializeField] private int[] _idx = { 0, 1, 2, 3 };
    public int GetMaxCnt() => _maxCnt;
    public int[] GetIdx() => _idx;


    Define.MonsterType ISpawnable.MonsterType => Define.MonsterType.WASD;

    private void Start()
    {
        Init();
        _rank = new HitJudge(holder.bounds.size.x, holder.bounds.size.y);
        _playerPos = GameObject.FindWithTag("Player").transform.position;

        Managers.Game.RankUpdate -= UpdateRankCnt;
        Managers.Game.RankUpdate += UpdateRankCnt;
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;
    }

    private void OnDestroy()
    {
        Managers.Game.RankUpdate -= UpdateRankCnt;
        PauseManager.IsPaused -= PauseForWhile;
    }

    private void Init() // (기존과 동일)
    {
        _spawnPosition = new Dictionary<WASDType, Vector2>();
        _targetPosition = new Dictionary<WASDType, Vector2>();
        for (int i = 0; i < positions.Length; i++)
        {
            var p = positions[i];
            _spawnPosition[p.WASDType] = p.spawnPos.transform.position;
            _targetPosition[p.WASDType] = p.targetPos.transform.position;
        }
    }

    private void UpdateRankCnt(RankNode rankNode) // (기존과 동일)
    {
        Vector2 target = _targetPosition[rankNode.WASDT];
        _rank.UpdateRankCnt(rankNode, target);
    }

    /**
     * [REFACTORED] ISpawnable.Spawn
     * 새 스폰 패턴 인스턴스를 생성하고 리스트에 추가한 뒤 반환합니다.
     */
    public ISpawnable.ISpawnInstance Spawn(MonsterData data)
    {
        // 1. 현재 dspTime을 기준으로 새 패턴 인스턴스 생성
        double startDsp = beatClock.GetScheduledDspTimeForTick(beatClock._tick);
        WASDPatternInstance newPattern = new WASDPatternInstance(this, data, startDsp);

        // 2. 활성 리스트에 추가
        _activePatterns.Add(newPattern);

        // 3. 제어 핸들(인스턴스) 반환
        return newPattern;
    }

    /**
     * [REFACTORED] ISpawnable.UnSpawnAll (기존 UnSpawn)
     * 모든 활성 패턴을 중지하고 리스트를 비웁니다.
     */
    public void UnSpawnAll()
    {
        _activePatterns.Clear();
    }

    /**
     * [NEW] 특정 패턴 인스턴스가 스스로 중지할 때 호출됩니다.
     */
    public void RemovePattern(WASDPatternInstance pattern)
    {
        // Update()에서 순회 중 리스트가 변경되는 것을 막기 위해
        // 제거 목록에 추가했다가 나중에 처리합니다.
        _patternsToRemove.Add(pattern);
    }

    /**
     * [REFACTORED] PauseForWhile
     * 모든 활성 패턴에 대해 일시정지를 적용합니다.
     */
    public void PauseForWhile(bool isStop)
    {
        double dspTime = AudioSettings.dspTime;
        foreach (var pattern in _activePatterns)
        {
            pattern.PauseForWhile(isStop, dspTime);
        }
    }

    /**
     * [REFACTORED] Update
     * 모든 활성 패턴의 Tick()을 호출합니다.
     */
    private void Update()
    {
        if (_activePatterns.Count == 0) return;

        double now = AudioSettings.dspTime;

        // 모든 활성 패턴을 순회하며 Tick 실행
        // (리스트 순회 중 삭제가 발생할 수 있으므로 역방향 순회 또는 임시 리스트 사용)
        for (int i = _activePatterns.Count - 1; i >= 0; i--)
        {
            _activePatterns[i].Tick(now);
        }

        // 중지 요청된 패턴들(스스로 Stop()을 호출한)을 리스트에서 제거
        if (_patternsToRemove.Count > 0)
        {
            foreach (var pattern in _patternsToRemove)
            {
                _activePatterns.Remove(pattern);
            }
            _patternsToRemove.Clear();
        }
    }

    // [MOVED] DoSpawn, SettingWASD_Type 등은 WASDPatternInstance로 이동했습니다.
    // [MOVED] ScheduledTime, SetLastSpawnTime 등도 WASDPatternInstance로 이동했습니다.


    /**
     * [MODIFIED]
     * 이제 MonsterData를 인스턴스로부터 전달받습니다.
     * 이 메서드는 모든 인스턴스가 공유합니다.
     */
    public void PoolEnemySpawn(WASDType enemyType, MonsterData data)
    {
        IngameData.TotalMobCnt++;
        EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);
        GameObject go = Managers.Pool.Pop(enemy.go).gameObject;
        go.transform.position = _spawnPosition[enemyType];

        VariableSetting(go.GetComponent<MovingEnemy>(), enemyType, data);
    }

    /**
     * [MODIFIED]
     * MonsterData를 인스턴스로부터 전달받습니다.
     */
    private void VariableSetting(MovingEnemy movingEnemy, WASDType type, MonsterData data)
    {
        float distance = Vector3.Distance(_spawnPosition[type], _targetPosition[type]);
        // [수정] _data 대신 매개변수 data 사용
        movingEnemy.SetVariance(distance, data, sizeDiffRate, _playerPos, type, data.monsterType);
    }

    #region QA
    public void QAUpdateVariables(Vector2 sizeDiffRate, int[] idx, int maxCnt)
    {
        this.sizeDiffRate = sizeDiffRate;
        this._maxCnt = Mathf.Clamp(maxCnt, 1, 4);
        this._idx = idx;

        // [NEW] 이미 실행 중인 패턴에도 QA 변경사항을 적용할 수 있습니다 (선택적)
        // foreach(var pattern in _activePatterns)
        // {
        //     pattern.UpdateQASettings(_maxCnt, _idx);
        // }
    }
    #endregion
}