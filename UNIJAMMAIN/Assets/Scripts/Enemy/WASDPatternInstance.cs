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
    private double _lastSpawnTime;
    private string _spawnPointString;
    private int _count;
    private double _pauseStartTime;
    private float _threshold = 0.1f;

    // 이 패턴의 DoSpawn()에 필요한 설정값
    private int _maxCnt;
    private int[] _idx;

    public WASDPatternInstance(WASDMonsterSpawner parent, MonsterData data, double startDsp)
    {
        _parent = parent;
        _data = data;
        _startDsp = startDsp;
        _spawning = true;
        _tick = 0;
        _count = 0;

        _spawnInterval = (IngameData.BeatInterval * data.spawnBeat) / data.speedUpRate;
        _spawnPointString = data.WASD_Pattern;

        // QA 설정값 (부모로부터 복사)
        _maxCnt = parent.GetMaxCnt();
        _idx = parent.GetIdx();

        // 마지막 스폰 시간 계산 (기존 SetLastSpawnTime 로직)
        if (IngameData.PhaseDurationSec == 0)
            Debug.LogWarning("Set Up Phase Duration!");

        _lastSpawnTime = _startDsp + IngameData.PhaseDurationSec - (IngameData.BeatInterval * (float)data.moveBeat) + _threshold;
    }

    private double ScheduledTime(long tickIndex)
        => _startDsp + tickIndex * _spawnInterval;

    /**
     * 부모 스포너의 Update()에서 매 프레임 호출됩니다.
     */
    public void Tick(double dspTime)
    {
        if (!_spawning) return;

        // 이 패턴의 생존 시간이 다 되면 스스로를 중지
        if (dspTime > _lastSpawnTime)
        {
            Stop();
            return;
        }

        // 스폰 타이밍 체크
        while (dspTime >= ScheduledTime(_tick))
        {
            _tick++;
            DoSpawn();
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
                _lastSpawnTime += pausedDuration; // 마지막 시간도 함께 밀어줌
                _pauseStartTime = 0;
            }
        }
    }

    // ==================================================
    // [MOVED] DoSpawn 로직 (WASDMonsterSpawner에서 이동)
    // ==================================================
    private void DoSpawn()
    {
        int cnt = UnityEngine.Random.Range(1, _maxCnt + 1);

        for (int i = 0; i < cnt; i++)
        {
            WASDType enemyType = WASDType.None;
            if (Managers.Game.CurrentState == GameManager.GameState.Battle && _spawnPointString != null)
            {
                while (_count < _spawnPointString.Length)
                {
                    char peekChar = _spawnPointString[_count]; // 현재 문자 확인
                    if (peekChar == ',' || peekChar == '/')
                    {
                        _count++; // 구분자이므로 인덱스만 증가시키고 다음 문자 확인
                    }
                    else
                    {
                        break; // 유효한 문자(W,A,S,D,R,() 등)이므로 스킵 루프 종료
                    }
                }

                if (_count < _spawnPointString.Length)// 출력가능하다면
                {
                    if (_spawnPointString[_count] == '(') // 동시출력
                    {
                        _count++;
                        while (_spawnPointString[_count] != ')')
                        {
                            enemyType = SettingWASD_Type(_spawnPointString[_count]);
                            if (enemyType == WASDType.None)
                                continue;

                            // [수정] 부모의 PoolEnemySpawn 호출 시 _data 전달
                            _parent.PoolEnemySpawn(enemyType, _data);
                            _count++;
                        }
                        _count++;
                        return;
                    }

                    enemyType = SettingWASD_Type(_spawnPointString[_count++]);
                    if (enemyType == WASDType.None)
                    {
                        continue;
                    }
                    else if (enemyType == WASDType.Random) // 랜덤이라면
                    {
                        enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)]; //enemyType랜덤으로
                        _parent.PoolEnemySpawn(enemyType, _data);
                    }
                    else
                    {
                        _parent.PoolEnemySpawn(enemyType, _data);
                    }
                }
                else
                {
                    enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)]; //enemyType랜덤으로.
                    _parent.PoolEnemySpawn(enemyType, _data);
                }
            }
            else
            {
                enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)]; //enemyType랜덤으로.
                _parent.PoolEnemySpawn(enemyType, _data);
            }
        }
    }

    private WASDType SettingWASD_Type(char type)
    {
        if (type == 'A') return WASDType.A;
        else if (type == 'W') return WASDType.W;
        else if (type == 'S') return WASDType.S;
        else if (type == 'D') return WASDType.D;
        else if (type == 'R') return WASDType.Random;
        return WASDType.None;
    }
}
