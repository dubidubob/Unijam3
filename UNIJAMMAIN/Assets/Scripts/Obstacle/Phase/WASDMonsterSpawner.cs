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
            // p.playerPos =
        }
    }

    int maxCnt = 2;
    int[] idx = { 0, 1, 2, 3 };
    private float _lastSpawnTime = -1f; // 처음에는 -1로 초기화해서 첫 호출 시 분기

    public void Spawn(MonsterData data)
    {
        float now = Time.time;
        if (_lastSpawnTime >= 0f) // 첫 호출이 아니면 경과 시간 출력
        {
            float elapsed = now - _lastSpawnTime;
            Debug.Log($"[Spawn Delay] {elapsed:F3}초 경과");
        }
        _lastSpawnTime = now; // 이번 호출 시각 기록

        int cnt = UnityEngine.Random.Range(1, maxCnt);
        for (int i = 0; i < cnt; i++)
        {
            WASDType enemyType = (WASDType)idx[UnityEngine.Random.Range(0, idx.Length)];

            EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);

            poolable = Managers.Pool.Pop(enemy.go);
            poolable.gameObject.transform.position = _spawnPosition[enemyType];

            movingEnemy = poolable.gameObject.GetComponent<MovingEnemy>();

            float distance = Vector3.Distance(_spawnPosition[enemyType], _targetPosition[enemyType]);
            movingEnemy.SetVariance(distance, data.moveToHolderDuration, data.numInRow, sizeDiffRate, enemyType);
            movingEnemy.SetKnockback(data.monsterType == Define.MonsterType.Knockback);
        }
    }

    public void QAUpdateVariables(Vector2 sizeDiffRate, int[] idx, int maxCnt)
    { 
        this.sizeDiffRate = sizeDiffRate;
        this.maxCnt = maxCnt > 4 ? 4 : maxCnt;
        this.idx = idx;
    }
}
