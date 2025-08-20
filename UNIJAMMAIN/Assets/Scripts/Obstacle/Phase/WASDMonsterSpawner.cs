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
    [SerializeField] SpriteRenderer sp;
    
    private Dictionary<WASDType, Vector3> _spawnPosition;
    private Dictionary<WASDType, Vector3> _targetPosition;

    private MovingEnemy movingEnemy;

    Define.MonsterType ISpawnable.MonsterType => Define.MonsterType.WASD;
    private void Start() => Init();
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

    
    private double _intervalSec; // 기본 스폰 간격
    private double _leadSec; // 이동할 시간
    private double _startDsp; // 기준선(dsp Time)
    private int _nextIndex = -1; // 다음 히트 인덱스(0부터 시작). -1은 미초기화.
    private static double DspNow => AudioSettings.dspTime;

    // 디버그
    int maxCnt = 1;
    int[] idx = { 0, 1, 2, 3 };
    bool isRed = false;

    public void Spawn(MonsterData data)
    {
        // interval/lead 설정
        if (_nextIndex < 0)
        {
            _intervalSec = data.Interval;           // 한 박자(또는 스폰) 간격
            _leadSec = data.MovingToHolderTime;    // 노트 이동 시간(등장~히트)

            // "바로 스폰"을 원하므로: 지금 스폰하면 _leadSec 뒤에 첫 히트가 오도록 기준선 설정
            // 첫 히트시각(hit(0)) = _startDsp + 1 * _intervalSec = now + _leadSec
            // => _startDsp = now + _leadSec - _intervalSec
            double now = DspNow;
            _startDsp = now + _leadSec - _intervalSec;
            _nextIndex = 0;
        }

        double nowDsp = DspNow;

        // 프레임 드랍 보정: 도달한 히트는 while로 모두 처리
        while (nowDsp + _leadSec >= HitTime(_nextIndex))
        {
            double hit = HitTime(_nextIndex);   // 이 시각에 "히트"해야 함
            DoSpawn(data, hit);                 // 지금 스폰하면 lead 뒤에 딱 맞음
            _nextIndex++;
        }
    }

    private double HitTime(int index) => _startDsp + (index + 1) * _intervalSec;

    private void DoSpawn(MonsterData data, double hitDspTime)
    {
        // 시각 확인용 색상 토글
        if (sp) { sp.color = isRed ? Color.red : Color.green; isRed = !isRed; }

        // 1~maxCnt까지 스폰
        int cnt = UnityEngine.Random.Range(1, maxCnt + 1);
        for (int i = 0; i < cnt; i++)
        {
            WASDType enemyType = (WASDType)idx[UnityEngine.Random.Range(0, idx.Length)];
            EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);

            GameObject go = Managers.Pool.Pop(enemy.go).gameObject;
            Vector3 spawnPos = _spawnPosition[enemyType];
            Vector3 targetPos = _targetPosition[enemyType];

            // dsp 이동 기반으로 세팅 (히트 시각 & 리드타임 & 궤적)
            movingEnemy = go.GetComponent<MovingEnemy>();
            movingEnemy.SetupDspMovement(
                spawnPos, targetPos,
                hitDspTime, _leadSec,
                sizeDiffRate, enemyType
            );

            // 넉백 모드 적용
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
