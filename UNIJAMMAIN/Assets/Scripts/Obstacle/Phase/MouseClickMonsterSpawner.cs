using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * [수정됨]
 * ISpawnable 인터페이스의 변경된 시그니처를 구현합니다.
 * 이 클래스는 이제 여러 개의 'MouseClickPatternInstance'를 관리하는 매니저 역할을 합니다.
 */
public class MouseClickMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;

    public Define.MonsterType MonsterType => Define.MonsterType.MouseClick;

    // [NEW] 현재 활성화된 모든 스폰 패턴(페이지)을 관리하는 리스트
    private List<MouseClickPatternInstance> _activePatterns = new List<MouseClickPatternInstance>();
    // [NEW] 리스트 순회 중 안전하게 제거하기 위한 목록
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

    // --- (DeactivateMouse, ActivateEnemy 등 기존 로직은 대부분 유지) ---
    #region Existing Logic
    private void DeactivateMouse(GamePlayDefine.MouseType mouseType)
    {
        GameObject deactivateGo = mouseType == GamePlayDefine.MouseType.Left ? LeftOne : RightOne;
        deactivateGo.SetActive(false);
    }

    // [MODIFIED] 이 메서드는 이제 모든 인스턴스에 의해 공유됩니다.
    public void ActivateEnemy()
    {
        var first = (Random.Range(0, 2) == 0) ? LeftOne : RightOne;
        var second = (first == LeftOne) ? RightOne : LeftOne;

        if (!first.activeSelf) first.SetActive(true);
        else if (!second.activeSelf) second.SetActive(true);
    }
    #endregion

    // ==================================================
    // [REFACTORED] ISpawnable 구현
    // ==================================================

    /**
     * [MODIFIED]
     * 새 스폰 패턴 인스턴스를 생성하고 리스트에 추가한 뒤 반환합니다.
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
     * 모든 활성 패턴을 중지하고 리스트를 비웁니다.
     */
    public void UnSpawnAll()
    {
        StopAllCoroutines();
        _activePatterns.Clear();
        _patternsToRemove.Clear();

        // (선택적) 활성화된 몬스터 강제 비활성화
        LeftOne.SetActive(false);
        RightOne.SetActive(false);
    }

    /**
     * [MODIFIED]
     * 모든 활성 패턴에 대해 일시정지를 적용합니다.
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
     * 인스턴스가 스스로 중지할 때 관리 리스트에서 제거하기 위해 호출됩니다.
     */
    public void RemovePattern(MouseClickPatternInstance pattern)
    {
        _patternsToRemove.Add(pattern);
    }

    /**
     * [NEW]
     * 스포너는 이제 Update문에서 실행중인 인스턴스를 관리합니다.
     */
    private void Update()
    {
        // 중지 요청된 패턴들(스스로 Stop()을 호출한)을 리스트에서 제거
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
// [NEW] 스폰 패턴(페이지)의 개별 인스턴스를 관리하는 헬퍼 클래스
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

        // [MOVED] SetLastSpawnTime 로직을 인스턴스 생성 시 처리
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
     * 이 인스턴스를 중지합니다.
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
     * 스폰 코루틴 로직 (기존과 거의 동일)
     */
    private IEnumerator DoSpawn(float spawnDuration)
    {
        var wait = new WaitForSeconds(spawnDuration);
        while (_spawning)
        {
            if (AudioSettings.dspTime > _lastSpawnTime)
            {
                Stop(); // 이 인스턴스만 중지
                yield break;
            }

            _parent.ActivateEnemy(); // 부모의 공용 메서드 호출
            yield return wait;
        }
    }
}