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
    public Vector2 playerPos;
}


public class WASDPatternInstance : ISpawnable.ISpawnInstance
{

    private WASDMonsterSpawner _parent; // 자신을 생성한 스포너
    private MonsterData _data;
    private double _spawnInterval;
    private long _tick;
    private bool _spawning;
    private double _startDsp;
    private string _spawnPointString;
    private int _count;
    private double _pauseStartTime;

    // 이 패턴의 DoSpawn()에 필요한 설정값
    private int _maxCnt;
    private int[] _idx;

    // [최적화] 파싱된 패턴 리스트
    private List<SpawnCommand> _preParsedPatterns;
    private int _patternIndex = 0; // 현재 리스트 인덱스

    public WASDPatternInstance(WASDMonsterSpawner parent, MonsterData data, double startDsp)
    {
        _parent = parent;
        _data = data;
        _startDsp = startDsp;
        _spawning = true;
        _tick = 0;
        _count = 0;

        _spawnInterval = (IngameData.BeatInterval * data.spawnBeat) / data.speedUpRate;
        // [안전장치] Interval 0 방지
        if (_spawnInterval <= 0.0001) _spawnInterval = 0.1;

        _spawnPointString = data.WASD_Pattern;

        // QA 설정값 (부모로부터 복사)
        _maxCnt = parent.GetMaxCnt();
        _idx = parent.GetIdx();

        // 마지막 스폰 시간 계산 (기존 SetLastSpawnTime 로직)
        if (IngameData.PhaseDurationSec == 0)
            Debug.LogWarning("Set Up Phase Duration!");

        _preParsedPatterns = parent.GetOrParsePattern(_spawnPointString);
    }

    private double ScheduledTime(long tickIndex)
        => _startDsp + tickIndex * _spawnInterval;

    /**
     * 부모 스포너의 Update()에서 매 프레임 호출됩니다.
     */
    public void Tick(double dspTime)
    {
        if (!_spawning) return;


        // 스폰 타이밍 체크
        while (dspTime >= ScheduledTime(_tick))
        {
            // 늦은 시간(Offset) 계산 (초 단위)
            double timeOffset = dspTime - ScheduledTime(_tick);

            // DoSpawn에 offset 전달
            DoSpawn((float)timeOffset);

            _tick++;
        }
    }

    /**
     * 이 인스턴스를 중지합니다. (ISpawnInstance 구현)
     */
    public void Stop()
    {
        _spawning = false;
        // 부모 리스트에서 자신을 제거하도록 요청
        _parent.RemovePattern(this);
    }

    /**
     * 일시정지 처리
     */
    public void PauseForWhile(bool isStop, double dspTime)
    {
        _spawning = !isStop;

        if (isStop)
        {
            _pauseStartTime = dspTime;
        }
        else
        {
            if (_pauseStartTime > 0)
            {
                double pausedDuration = dspTime - _pauseStartTime;
                _startDsp += pausedDuration;
                _pauseStartTime = 0;
            }
        }
    }


    private void DoSpawn(float timeOffset)
    {
        // 패턴대로 갈 때는 1번에 1개씩 소비하는 것이 정확함
        // 만약 난이도 조절로 한 번에 여러 개를 소모해야 한다면 cnt 사용
        // 여기서는 패턴의 정확성을 위해 cnt=1로 가정하거나, 루프를 돌림

        int cnt = 1;
        // 만약 랜덤모드일때만 _maxCnt를 쓰고 싶다면 로직 분리가 필요하지만,
        // 패턴 모드에서는 보통 1틱 = 1패턴글자 입니다.

        for (int i = 0; i < cnt; i++)
        {
            // 1. 미리 파싱된 패턴이 남아있다면 그걸 우선 사용
            if (_preParsedPatterns != null && _patternIndex < _preParsedPatterns.Count)
            {
                SpawnCommand cmd = _preParsedPatterns[_patternIndex++];

                // [핵심] 빈 박자(X)면 아무것도 안 하고 리턴 (박자만 소비됨)
                if (cmd.IsEmpty)
                {
                    continue;
                }

                if (cmd.IsRandom)
                {
                    WASDType randomType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)];
                    _parent.PoolEnemySpawn(randomType, _data, timeOffset);
                }
                else if (cmd.Types != null)
                {
                    for (int j = 0; j < cmd.Types.Length; j++)
                    {
                        _parent.PoolEnemySpawn(cmd.Types[j], _data, timeOffset);
                    }
                }
            }
            // 2. 패턴이 다 끝났다면?
            else
            {
                // [수정] 패턴이 끝나면 아무것도 소환하지 않음! (랜덤 생성 방지)
                // 만약 끝나고 랜덤이 나오게 하고 싶으면 아래 주석 해제

                /*
                WASDType randomType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)];
                _parent.PoolEnemySpawn(randomType, _data, timeOffset);
                */
            }
        }
    }


    #region tool
    
    #endregion
}
