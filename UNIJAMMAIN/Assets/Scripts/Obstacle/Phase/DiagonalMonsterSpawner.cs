using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

/**
 * [������]
 * ISpawnable �������̽��� ����� �ñ״�ó�� �����մϴ�.
 * �� Ŭ������ ���� ���� ���� 'DiagonalPatternInstance'�� �����ϴ� �Ŵ��� ������ �մϴ�.
 */
public class DiagonalMonsterSpawner : MonoBehaviour, ISpawnable
{
    // --- (���� RankState enum, Dictionary ���� �״�� ����) ---
    enum RankState { Spawned, Success, Fail };
    public Define.MonsterType MonsterType => Define.MonsterType.Diagonal;
    private Dictionary<DiagonalType, GameObject> diagonalDict;
    private List<int> activatedDiagonalIdx = new List<int>();
    private List<int> deactivatedDiagonalIdx = new List<int>();
    private int attackValue = 20;
    private int spawnedDiagonalMobCnt = 0; // Ʃ�丮��� ī��Ʈ

    // [NEW] ���� Ȱ��ȭ�� ��� ���� ����(������)�� �����ϴ� ����Ʈ
    private List<DiagonalPatternInstance> _activePatterns = new List<DiagonalPatternInstance>();
    // [NEW] ����Ʈ ��ȸ �� �����ϰ� �����ϱ� ���� ���
    private List<DiagonalPatternInstance> _patternsToRemove = new List<DiagonalPatternInstance>();


    private void OnEnable()
    {
        InitialDict();
        Managers.Input.InputDiagonal -= DeactivateDiagonal;
        Managers.Input.InputDiagonal += DeactivateDiagonal;
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;
    }

    private void OnDestroy()
    {
        Managers.Input.InputDiagonal -= DeactivateDiagonal;
        PauseManager.IsPaused -= PauseForWhile;
    }

    // --- (UpdateRankCnt, InitialDict, DeactivateDiagonal �� ���� ������ ��κ� ����) ---
    #region Existing Logic
    private void UpdateRankCnt(RankState state)
    {
        switch (state)
        {
            case RankState.Success: IngameData.IncPerfect(); break;
            case RankState.Fail: IngameData.IncAttacked(); break;
            case RankState.Spawned: IngameData.TotalMobCnt++; break;
        }
    }

    private void InitialDict()
    {
        DiagonalMonster[] dm = GetComponentsInChildren<DiagonalMonster>(true);
        diagonalDict = new Dictionary<DiagonalType, GameObject>();
        if (dm.Length == 0) Debug.LogWarning("Place LU, LD, RU, RD in Inspector");
        int i = 0;
        foreach (var m in dm)
        {
            diagonalDict[m.DiagonalT] = m.gameObject;
            deactivatedDiagonalIdx.Add(i++);
        }
    }

    // [MODIFIED] �� �޼���� ���� ��� �ν��Ͻ��� ���� �����˴ϴ�.
    public void ActivateEnemy(float moveBeat)
    {
        UpdateRankCnt(RankState.Spawned);

        int idx = Random.Range(0, deactivatedDiagonalIdx.Count);
        if (IngameData.ChapterIdx == 0 && spawnedDiagonalMobCnt < 2)
        {
            idx = (spawnedDiagonalMobCnt == 0) ? (int)DiagonalType.RightUp : (int)DiagonalType.LeftDown;
            spawnedDiagonalMobCnt++;
        }

        if (deactivatedDiagonalIdx.Count == 0) return;
        int mIdx = deactivatedDiagonalIdx[idx];
        deactivatedDiagonalIdx.Remove(mIdx);

        diagonalDict[(DiagonalType)mIdx].GetComponent<DiagonalMonster>().SetMovebeat(moveBeat);
        diagonalDict[(DiagonalType)mIdx].SetActive(true);
        activatedDiagonalIdx.Add(mIdx);
    }

    private void DeactivateDiagonal(DiagonalType attackType)
    {
        if (activatedDiagonalIdx.Contains((int)attackType))
        {
            activatedDiagonalIdx.Remove((int)attackType);
            deactivatedDiagonalIdx.Add((int)attackType);
            UpdateRankCnt(RankState.Success);
            diagonalDict[attackType].GetComponent<DiagonalMonster>().SetDead();
        }
        else
        {
            Managers.Game.PlayerAttacked(attackValue);
        }
    }
    #endregion

    // ==================================================
    // [REFACTORED] ISpawnable ����
    // ==================================================

    /**
     * [MODIFIED]
     * �� ���� ���� �ν��Ͻ��� �����ϰ� ����Ʈ�� �߰��� �� ��ȯ�մϴ�.
     */
    public ISpawnable.ISpawnInstance Spawn(MonsterData data)
    {
        // 1. �� ���� �ν��Ͻ� ����
        DiagonalPatternInstance instance = new DiagonalPatternInstance(this, data);

        // 2. ���� ����Ʈ�� �߰�
        _activePatterns.Add(instance);

        // 3. �ν��Ͻ��� ���� �ڷ�ƾ ����
        instance.StartSpawning();

        // 4. ���� �ڵ�(�ν��Ͻ�) ��ȯ
        return instance;
    }

    /**
     * [MODIFIED]
     * ��� Ȱ�� ������ �����ϰ� ����Ʈ�� ���ϴ�.
     */
    public void UnSpawnAll()
    {
        StopAllCoroutines(); // �� �����ʰ� ������ ��� �ڷ�ƾ ����
        _activePatterns.Clear();
        _patternsToRemove.Clear();

        // (������) Ȱ��ȭ�� ��� ���͸� ���� ��Ȱ��ȭ
        foreach (var idx in activatedDiagonalIdx)
        {
            diagonalDict[(DiagonalType)idx].GetComponent<DiagonalMonster>().SetDead(false); // ���� ���� ��� ���
        }
        activatedDiagonalIdx.Clear();
        deactivatedDiagonalIdx.Clear();
        InitialDict(); // �ε��� ����Ʈ �缳��
    }

    /**
     * [MODIFIED]
     * ��� Ȱ�� ���Ͽ� ���� �Ͻ������� �����մϴ�.
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
     * [NEW]
     * �ν��Ͻ��� ������ ������ �� ���� ����Ʈ���� �����ϱ� ���� ȣ��˴ϴ�.
     */
    public void RemovePattern(DiagonalPatternInstance pattern)
    {
        // Update()���� ��ȸ �� ����Ʈ�� ����Ǵ� ���� ���� ����
        // ���� ��Ͽ� �߰��ߴٰ� ���߿� ó���մϴ�.
        _patternsToRemove.Add(pattern);
    }

    /**
     * [NEW]
     * �����ʴ� ���� Update������ �������� �ν��Ͻ��� �����մϴ�.
     */
    private void Update()
    {
        // ���� ��û�� ���ϵ�(������ Stop()�� ȣ����)�� ����Ʈ���� ����
        if (_patternsToRemove.Count > 0)
        {
            foreach (var pattern in _patternsToRemove)
            {
                _activePatterns.Remove(pattern);
            }
            _patternsToRemove.Clear();
        }
    }
}


// =======================================================================
// [NEW] ���� ����(������)�� ���� �ν��Ͻ��� �����ϴ� ���� Ŭ����
// =======================================================================
public class DiagonalPatternInstance : ISpawnable.ISpawnInstance
{
    private DiagonalMonsterSpawner _parent; // �ڽ��� ������ ������
    private MonsterData _data;
    private bool _spawning;
    private double _lastSpawnTime;
    private float _moveBeat;
    private Coroutine _spawnCoroutine;
    private double _pauseStartTime;
    private float threshold = 0.4f;

    public DiagonalPatternInstance(DiagonalMonsterSpawner parent, MonsterData data)
    {
        _parent = parent;
        _data = data;
        _moveBeat = data.moveBeat;
        _spawning = true;

        // [MOVED] SetLastSpawnTime ������ �ν��Ͻ� ���� �� ó��
        if (IngameData.PhaseDurationSec == 0)
        {
            Debug.LogWarning("Set Up Phase Duration!");
        }
        float moveBeatDuration = 4 * (float)data.moveBeat; // 4�� �ٴµ�, �� �� �� ������ moveBeat ����
        _lastSpawnTime = AudioSettings.dspTime + IngameData.PhaseDurationSec - (IngameData.BeatInterval * moveBeatDuration) + threshold;
    }

    /**
     * �θ� ������(MonoBehaviour)�� ���� �ڷ�ƾ�� �����մϴ�.
     */
    public void StartSpawning()
    {
        float spawnDuration = (float)IngameData.BeatInterval * _data.spawnBeat;
        _spawnCoroutine = _parent.StartCoroutine(DoSpawn(spawnDuration));
    }

    /**
     * [IMPLEMENTS] ISpawnable.ISpawnInstance.Stop
     * �� �ν��Ͻ��� �����մϴ�.
     */
    public void Stop()
    {
        _spawning = false;

        if (_spawnCoroutine != null && _parent != null)
        {
            // �θ��� �ڷ�ƾ�� ����
            _parent.StopCoroutine(_spawnCoroutine);
        }

        // �θ��� ���� ����Ʈ���� �ڽ��� �����ϵ��� ��û
        _parent.RemovePattern(this);
    }

    /**
     * �Ͻ����� ó�� (dspTime ����)
     */
    public void PauseForWhile(bool isStop, double dspTime)
    {
        // �ڷ�ƾ�� Time.timeScale�� ���� �ڵ����� ���߹Ƿ�,
        // ���⼭�� dspTime ����� _lastSpawnTime�� �������ݴϴ�.
        if (isStop)
        {
            _pauseStartTime = dspTime;
        }
        else
        {
            if (_pauseStartTime > 0)
            {
                double pausedDuration = dspTime - _pauseStartTime;
                _lastSpawnTime += pausedDuration;
                _pauseStartTime = 0;
            }
        }
    }

    /**
     * [MOVED]
     * ���� �ڷ�ƾ ���� (������ ���� ����)
     */
    private IEnumerator DoSpawn(float spawnDuration)
    {
        yield return new WaitForSeconds((float)IngameData.BeatInterval * 0.5f);
        while (_spawning)
        {
            // dspTime�� �� �ν��Ͻ��� lastSpawnTime�� ������ �ڷ�ƾ ����
            if (AudioSettings.dspTime > _lastSpawnTime)
            {
                Stop(); // UnSpawn() ��� �� �ν��Ͻ��� Stop() ȣ��
                yield break;
            }

            _parent.ActivateEnemy(_moveBeat); // �θ��� ���� �޼��� ȣ��

            // Time.timeScale�� ������ �޴� WaitForSeconds ���
            yield return new WaitForSeconds(spawnDuration);
        }
    }
}