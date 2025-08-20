using System;
using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

[Serializable]
public struct WASDPosition
{
    public WASDType WASDType;
    public GameObject spawnPos;
    public GameObject targetPos;
    public Vector3 playerPos;
}

public class WASDMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] WASDPosition[] positions;
    [SerializeField] Vector2 sizeDiffRate = new Vector2 (0.8f, 1.2f);
    private Dictionary<WASDType, Vector3> _spawnPosition;
    private Dictionary<WASDType, Vector3> _targetPosition;
    private MovingEnemy movingEnemy;
    private Poolable poolable;

    Define.MonsterType ISpawnable.MonsterType => Define.MonsterType.WASD;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _spawnPosition = new Dictionary<WASDType, Vector3>();
        _targetPosition = new Dictionary<WASDType, Vector3>();

        for (int i = 0; i < positions.Length; i++)
        {
            var p = positions[i];

            _spawnPosition[p.WASDType] = p.spawnPos.transform.position;
            _targetPosition[p.WASDType] = p.targetPos.transform.position;
        }
    }

    private double _spawnInterval; // 기본 스폰 간격
    private double _nextSpawnTime = 0f;   // 다음 스폰 예정 시각
    private bool _isFirstSpawn = true;   // 첫 스폰 여부
    private double _lastSpawnTime = -1.0; // 디버그용
    private static double Now => Time.realtimeSinceStartupAsDouble;

    public void Spawn(MonsterData data)
    {
        double currentTime = Now;

        if (_isFirstSpawn)
        {
            _spawnInterval = data.spawnDuration;
            _nextSpawnTime = currentTime;
            DoSpawn(data, currentTime);
            _nextSpawnTime += _spawnInterval;
            _isFirstSpawn = false;
            return;
        }

        bool spawnedThisCall = false;
        while (currentTime >= _nextSpawnTime)
        {
            DoSpawn(data, currentTime);
            _nextSpawnTime += _spawnInterval;
            spawnedThisCall = true;
        }
        // 디버그용: 실제 경과 시간 출력
        if (spawnedThisCall && _lastSpawnTime > 0.0)
        {
            double actualInterval = currentTime - _lastSpawnTime;
            double err = actualInterval - _spawnInterval;
            // Debug.Log($"[Spawn] 목표: {_spawnInterval:F3}s, 실제: {actualInterval:F3}s, 오차: {err:+0.000;-0.000;0.000}s");
        }
    }

    int maxCnt = 1;
    int[] idx = { 0, 1, 2, 3 };
    private void DoSpawn(MonsterData data, double currentTime)
    {
        _lastSpawnTime = currentTime;

        // int Random.Range는 상한 배제이므로 +1 해서 포함 범위 맞춤
        int cnt = UnityEngine.Random.Range(1, maxCnt + 1);

        for (int i = 0; i < cnt; i++)
        {
            WASDType enemyType = (WASDType)idx[UnityEngine.Random.Range(0, idx.Length)];
            EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);
            GameObject go = Managers.Pool.Pop(enemy.go).gameObject;
            
            go.transform.position = _spawnPosition[enemyType];
            movingEnemy = go.GetComponent<MovingEnemy>();

            float distance = Vector3.Distance(_spawnPosition[enemyType], _targetPosition[enemyType]);
            movingEnemy.SetVariance(distance, data.MovingToHolderTime, data.numInRow, sizeDiffRate, enemyType);
            movingEnemy.SetKnockback(data.monsterType == Define.MonsterType.Knockback);
        }
    }

    public void QAUpdateVariables(Vector2 sizeDiffRate, int[] idx, int maxCnt)
    { 
        this.sizeDiffRate = sizeDiffRate;
        this.maxCnt = Mathf.Clamp(maxCnt, 1, 4);
        this.idx = idx;
    }
}
