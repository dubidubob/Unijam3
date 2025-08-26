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
    private Rank _rank;
    
    Define.MonsterType ISpawnable.MonsterType => Define.MonsterType.WASD;

    

    private void Start()
    {
        Init();

        Managers.Game.RankUpdate -= UpdateRankCnt;
        Managers.Game.RankUpdate += UpdateRankCnt;
    }

    private void UpdateRankCnt(RankNode rankNode)
    {
        Vector3 target = _targetPosition[rankNode.WASDT];
        _rank.UpdateRankCnt(rankNode, target);
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
    private long _tick; // 박자
    private MonsterData _data; 
    private bool _Spawning = false;
    private double _startDsp; 
    public void Spawn(MonsterData data)
    {
        _data = data;
        _spawnInterval = IngameData.BeatInterval * data.spawnBeat;
        _tick = 0;
         _startDsp = AudioSettings.dspTime;
        _Spawning = true;
    }

    public void UnSpawn()
    {
        _Spawning = false;
    }

    private void Update()
    {
        if (!_Spawning) return;
        
        double now = AudioSettings.dspTime;
        while (now >= ScheduledTime(_tick + 1))
        {
            _tick++;
            IngameData.TotalMobCnt++;
            DoSpawn();
        }
    }

    private double ScheduledTime(long tickIndex)
    => _startDsp + tickIndex * _spawnInterval;

    int _maxCnt = 1;
    int[] _idx = { 0, 1, 2, 3 };
    private void DoSpawn()
    {
        int cnt = UnityEngine.Random.Range(1, _maxCnt + 1);

        for (int i = 0; i < cnt; i++)
        {
            WASDType enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)];
            EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);
            GameObject go = Managers.Pool.Pop(enemy.go).gameObject;
            go.transform.position = _spawnPosition[enemyType];

            VariableSetting(go.GetComponent<MovingEnemy>(), enemyType);
        }
    }

    private void VariableSetting(MovingEnemy movingEnemy, WASDType type)
    {
        float distance = Vector3.Distance(_spawnPosition[type], _targetPosition[type]);
        movingEnemy.SetVariance(distance, _data, sizeDiffRate, type);
        movingEnemy.SetKnockback(_data.monsterType == Define.MonsterType.Knockback);
    }

    public void QAUpdateVariables(Vector2 sizeDiffRate, int[] idx, int maxCnt)
    { 
        this.sizeDiffRate = sizeDiffRate;
        this._maxCnt = Mathf.Clamp(maxCnt, 1, 4);
        this._idx = idx;
    }
}
