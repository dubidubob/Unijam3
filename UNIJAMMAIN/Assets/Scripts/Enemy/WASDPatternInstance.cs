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

    private WASDMonsterSpawner _parent; // �ڽ��� ������ ������
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

    // �� ������ DoSpawn()�� �ʿ��� ������
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

        // QA ������ (�θ�κ��� ����)
        _maxCnt = parent.GetMaxCnt();
        _idx = parent.GetIdx();

        // ������ ���� �ð� ��� (���� SetLastSpawnTime ����)
        if (IngameData.PhaseDurationSec == 0)
            Debug.LogWarning("Set Up Phase Duration!");

        _lastSpawnTime = _startDsp + IngameData.PhaseDurationSec - (IngameData.BeatInterval * (float)data.moveBeat) + _threshold;
    }

    private double ScheduledTime(long tickIndex)
        => _startDsp + tickIndex * _spawnInterval;

    /**
     * �θ� �������� Update()���� �� ������ ȣ��˴ϴ�.
     */
    public void Tick(double dspTime)
    {
        if (!_spawning) return;

        // �� ������ ���� �ð��� �� �Ǹ� �����θ� ����
        if (dspTime > _lastSpawnTime)
        {
            Stop();
            return;
        }

        // ���� Ÿ�̹� üũ
        while (dspTime >= ScheduledTime(_tick))
        {
            _tick++;
            DoSpawn();
        }
    }

    /**
     * �� �ν��Ͻ��� �����մϴ�. (ISpawnInstance ����)
     */
    public void Stop()
    {
        _spawning = false;
        // �θ� ����Ʈ���� �ڽ��� �����ϵ��� ��û
        _parent.RemovePattern(this);
    }

    /**
     * �Ͻ����� ó��
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
                _lastSpawnTime += pausedDuration; // ������ �ð��� �Բ� �о���
                _pauseStartTime = 0;
            }
        }
    }

    // ==================================================
    // [MOVED] DoSpawn ���� (WASDMonsterSpawner���� �̵�)
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
                    char peekChar = _spawnPointString[_count]; // ���� ���� Ȯ��
                    if (peekChar == ',' || peekChar == '/')
                    {
                        _count++; // �������̹Ƿ� �ε����� ������Ű�� ���� ���� Ȯ��
                    }
                    else
                    {
                        break; // ��ȿ�� ����(W,A,S,D,R,() ��)�̹Ƿ� ��ŵ ���� ����
                    }
                }

                if (_count < _spawnPointString.Length)// ��°����ϴٸ�
                {
                    if (_spawnPointString[_count] == '(') // �������
                    {
                        _count++;
                        while (_spawnPointString[_count] != ')')
                        {
                            enemyType = SettingWASD_Type(_spawnPointString[_count]);
                            if (enemyType == WASDType.None)
                                continue;

                            // [����] �θ��� PoolEnemySpawn ȣ�� �� _data ����
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
                    else if (enemyType == WASDType.Random) // �����̶��
                    {
                        enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)]; //enemyType��������
                        _parent.PoolEnemySpawn(enemyType, _data);
                    }
                    else
                    {
                        _parent.PoolEnemySpawn(enemyType, _data);
                    }
                }
                else
                {
                    enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)]; //enemyType��������.
                    _parent.PoolEnemySpawn(enemyType, _data);
                }
            }
            else
            {
                enemyType = (WASDType)_idx[UnityEngine.Random.Range(0, _idx.Length)]; //enemyType��������.
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
