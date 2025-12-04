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
        // [안전장치] Interval 0 방지
        if (_spawnInterval <= 0.001) _spawnInterval = 0.1;

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
                _lastSpawnTime += pausedDuration; // 마지막 시간도 함께 밀어줌
                _pauseStartTime = 0;
            }
        }
    }

    // [수정] timeOffset 파라미터 추가
    private void DoSpawn(float timeOffset)
    {
        int cnt = UnityEngine.Random.Range(1, _maxCnt + 1);

        for (int i = 0; i < cnt; i++)
        {
            WASDType enemyType = WASDType.None;
            if (Managers.Game.CurrentState == GameManager.GameState.Battle && _spawnPointString != null)
            {
                while (_count < _spawnPointString.Length)
                {
                    char peekChar = _spawnPointString[_count];
                    if (peekChar == ',' || peekChar == '/') _count++;
                    else break;
                }

                if (_count < _spawnPointString.Length)
                {
                    if (_spawnPointString[_count] == '(')
                    {
                        _count++;
                        while (_spawnPointString[_count] != ')')
                        {
                            enemyType = SettingWASD_Type(_spawnPointString[_count]);
                            if (enemyType == WASDType.None) continue;

                            // [수정] offset 전달
                            _parent.PoolEnemySpawn(enemyType, _data, timeOffset);
                            _count++;
                        }
                        _count++;
                        return;
                    }

                    enemyType = SettingWASD_Type(_spawnPointString[_count++]);
                    if (enemyType == WASDType.None) continue;

                    if (enemyType == WASDType.Random)
                    {
                        enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)];
                        _parent.PoolEnemySpawn(enemyType, _data, timeOffset);
                    }
                    else
                    {
                        _parent.PoolEnemySpawn(enemyType, _data, timeOffset);
                    }
                }
                else
                {
                    enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)];
                    _parent.PoolEnemySpawn(enemyType, _data, timeOffset);
                }
            }
            else
            {
                enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)];
                _parent.PoolEnemySpawn(enemyType, _data, timeOffset);
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
