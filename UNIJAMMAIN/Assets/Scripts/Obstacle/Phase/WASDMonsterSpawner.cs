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
    [SerializeField] Vector2 sizeDiffRate;
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

    public void Spawn(MonsterData data)
    {
        int cnt = UnityEngine.Random.Range(1, 3);
        // int cnt = 1;
        for (int i = 0; i < cnt; i++)
        {
            // WASDType enemyType = (WASDType)UnityEngine.Random.Range(0, (int)WASDType.MaxCnt);
            WASDType enemyType = (WASDType)UnityEngine.Random.Range(2, (int)WASDType.MaxCnt);

            EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);

            poolable = Managers.Pool.Pop(enemy.go);
            poolable.gameObject.transform.position = _spawnPosition[enemyType];

            movingEnemy = poolable.gameObject.GetComponent<MovingEnemy>();

            float distance = Vector3.Distance(_spawnPosition[enemyType], _targetPosition[enemyType]);
            movingEnemy.SetVariance(distance, data.moveToHolderDuration, data.numInRow, sizeDiffRate, enemyType);
            if (data.monsterType == Define.MonsterType.Knockback)
            {
                movingEnemy.SetKnockback(true);
            }
            else
            {
                movingEnemy.SetKnockback(false);
            }
        }
    }
}
