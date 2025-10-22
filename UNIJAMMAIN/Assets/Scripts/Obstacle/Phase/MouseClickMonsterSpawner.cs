using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * [������]
 * ISpawnable �������̽��� ����� �ñ״�ó�� �����մϴ�.
 * �� Ŭ������ ���� ���� ���� 'MouseClickPatternInstance'�� �����ϴ� �Ŵ��� ������ �մϴ�.
 */
public class MouseClickMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;

    public Define.MonsterType MonsterType => Define.MonsterType.MouseClick;

    // [NEW] ���� Ȱ��ȭ�� ��� ���� ����(������)�� �����ϴ� ����Ʈ
    private List<MouseClickPatternInstance> _activePatterns = new List<MouseClickPatternInstance>();
    // [NEW] ����Ʈ ��ȸ �� �����ϰ� �����ϱ� ���� ���
    private List<MouseClickPatternInstance> _patternsToRemove = new List<MouseClickPatternInstance>();

    private void Awake()
    {
        LeftOne.SetActive(false);
        RightOne.SetActive(false);

        Managers.Input.InputMouse -= DeactivateMouse;
        Managers.Input.InputMouse += DeactivateMouse;
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;
    }

    private void OnDestroy()
    {
        Managers.Input.InputMouse -= DeactivateMouse;
        PauseManager.IsPaused -= PauseForWhile;
    }

    // --- (DeactivateMouse, ActivateEnemy �� ���� ������ ��κ� ����) ---
    #region Existing Logic
    private void DeactivateMouse(GamePlayDefine.MouseType mouseType)
    {
        GameObject deactivateGo = mouseType == GamePlayDefine.MouseType.Left ? LeftOne : RightOne;
        deactivateGo.SetActive(false);
    }

    // [MODIFIED] �� �޼���� ���� ��� �ν��Ͻ��� ���� �����˴ϴ�.
    public void ActivateEnemy()
    {
        var first = (Random.Range(0, 2) == 0) ? LeftOne : RightOne;
        var second = (first == LeftOne) ? RightOne : LeftOne;

        if (!first.activeSelf) first.SetActive(true);
        else if (!second.activeSelf) second.SetActive(true);
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
        MouseClickPatternInstance instance = new MouseClickPatternInstance(this, data);
        _activePatterns.Add(instance);
        instance.StartSpawning();
        return instance;
    }

    /**
     * [MODIFIED]
     * ��� Ȱ�� ������ �����ϰ� ����Ʈ�� ���ϴ�.
     */
    public void UnSpawnAll()
    {
        StopAllCoroutines();
        _activePatterns.Clear();
        _patternsToRemove.Clear();

        // (������) Ȱ��ȭ�� ���� ���� ��Ȱ��ȭ
        LeftOne.SetActive(false);
        RightOne.SetActive(false);
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
    public void RemovePattern(MouseClickPatternInstance pattern)
    {
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
public class MouseClickPatternInstance : ISpawnable.ISpawnInstance
{
    private MouseClickMonsterSpawner _parent;
    private MonsterData _data;
    private bool _spawning;
    private double _lastSpawnTime;
    private Coroutine _spawnCoroutine;
    private double _pauseStartTime;
    private float threshold = 2f;

    public MouseClickPatternInstance(MouseClickMonsterSpawner parent, MonsterData data)
    {
        _parent = parent;
        _data = data;
        _spawning = true;

        // [MOVED] SetLastSpawnTime ������ �ν��Ͻ� ���� �� ó��
        if (IngameData.PhaseDurationSec == 0)
        {
            Debug.LogWarning("Set Up Phase Duration!");
        }
        _lastSpawnTime = AudioSettings.dspTime + IngameData.PhaseDurationSec - threshold;
    }

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
            _parent.StopCoroutine(_spawnCoroutine);
        }
        _parent.RemovePattern(this);
    }

    public void PauseForWhile(bool isStop, double dspTime)
    {
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
        var wait = new WaitForSeconds(spawnDuration);
        while (_spawning)
        {
            if (AudioSettings.dspTime > _lastSpawnTime)
            {
                Stop(); // �� �ν��Ͻ��� ����
                yield break;
            }

            _parent.ActivateEnemy(); // �θ��� ���� �޼��� ȣ��
            yield return wait;
        }
    }
}