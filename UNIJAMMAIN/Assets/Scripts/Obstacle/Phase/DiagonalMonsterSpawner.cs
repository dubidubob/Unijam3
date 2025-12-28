using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

/**
 * [수정됨]
 * ISpawnable 인터페이스의 변경된 시그니처를 구현합니다.
 * 이 클래스는 이제 여러 개의 'DiagonalPatternInstance'를 관리하는 매니저 역할을 합니다.
 */
public class DiagonalMonsterSpawner : MonoBehaviour, ISpawnable
{
    // --- (기존 RankState enum, Dictionary 등은 그대로 유지) ---
    enum RankState { Spawned, Success, Fail };
    public Define.MonsterType MonsterType => Define.MonsterType.Diagonal;
    private Dictionary<DiagonalType, GameObject> diagonalDict;
    private List<int> activatedDiagonalIdx = new List<int>();
    private List<int> deactivatedDiagonalIdx = new List<int>();
    private int attackValue = 20;
    private int spawnedDiagonalMobCnt = 0; // 튜토리얼용 카운트

    // [NEW] 현재 활성화된 모든 스폰 패턴(페이지)을 관리하는 리스트
    private List<DiagonalPatternInstance> _activePatterns = new List<DiagonalPatternInstance>();
    // [NEW] 리스트 순회 중 안전하게 제거하기 위한 목록
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

    // --- (UpdateRankCnt, InitialDict, DeactivateDiagonal 등 기존 로직은 대부분 유지) ---
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

    // 이 메서드는 이제 모든 인스턴스에 의해 공유됩니다.
    // [수정] targetIndex 인자를 추가하여 특정 위치 소환을 지원합니다. (기본값 null)
    public void ActivateEnemy(float moveBeat, MonsterData data, int? targetIndex = null)
    {
        UpdateRankCnt(RankState.Spawned);

        int mIdx = -1; // 실제 활성화할 몬스터의 Enum Index

        // 1. 지정된 패턴이 있는 경우 (targetIndex가 값이 있음)
        if (targetIndex != null)
        {
            // 원하는 위치가 현재 비활성 리스트(대기중)에 있는지 확인
            if (deactivatedDiagonalIdx.Contains(targetIndex.Value))
            {
                mIdx = targetIndex.Value;
                deactivatedDiagonalIdx.Remove(mIdx);
            }
            else
            {
                // 이미 활성화된 상태라면, 겹치지 않게 이번 스폰은 스킵하거나
                // 필요하다면 여기서 강제 재설정 로직을 넣을 수 있음.
                // 현재는 겹침 방지를 위해 리턴.
                return;
            }
        }
        // 2. 지정된 패턴이 없는 경우 (기존 랜덤/튜토리얼 로직)
        else
        {
            int idx = Random.Range(0, deactivatedDiagonalIdx.Count);

            if (IngameData.ChapterIdx == 0 && spawnedDiagonalMobCnt < 2)
            {
                idx = (spawnedDiagonalMobCnt == 0) ? (int)DiagonalType.RightUp : (int)DiagonalType.LeftDown;
                spawnedDiagonalMobCnt++;
            }

            if (deactivatedDiagonalIdx.Count == 0) return;

            // 리스트에 있는 값(Enum Index)을 가져옴
            mIdx = deactivatedDiagonalIdx[idx];
            deactivatedDiagonalIdx.Remove(mIdx);
        }

        // 몬스터 활성화 로직
        if (mIdx != -1)
        {
            diagonalDict[(DiagonalType)mIdx].GetComponent<DiagonalMonster>().SetMovebeat(moveBeat);
            diagonalDict[(DiagonalType)mIdx].SetActive(true);
            activatedDiagonalIdx.Add(mIdx);
        }
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
    // [REFACTORED] ISpawnable 구현
    // ==================================================

    /**
     * [MODIFIED]
     * 새 스폰 패턴 인스턴스를 생성하고 리스트에 추가한 뒤 반환합니다.
     */
    public ISpawnable.ISpawnInstance Spawn(MonsterData data)
    {
        // 1. 새 패턴 인스턴스 생성
        DiagonalPatternInstance instance = new DiagonalPatternInstance(this, data);

        // 2. 관리 리스트에 추가
        _activePatterns.Add(instance);

        // 3. 인스턴스의 스폰 코루틴 시작
        instance.StartSpawning(data);

        // 4. 제어 핸들(인스턴스) 반환
        return instance;
    }

    /**
     * [MODIFIED]
     * 모든 활성 패턴을 중지하고 리스트를 비웁니다.
     */
    public void UnSpawnAll()
    {
        StopAllCoroutines(); // 이 스포너가 실행한 모든 코루틴 중지
        _activePatterns.Clear();
        _patternsToRemove.Clear();

        // (선택적) 활성화된 모든 몬스터를 강제 비활성화
        foreach (var idx in activatedDiagonalIdx)
        {
            diagonalDict[(DiagonalType)idx].GetComponent<DiagonalMonster>().SetDead(false); // 판정 없이 즉시 사망
        }
        activatedDiagonalIdx.Clear();
        deactivatedDiagonalIdx.Clear();
        InitialDict(); // 인덱스 리스트 재설정
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
    public void RemovePattern(DiagonalPatternInstance pattern)
    {
        // Update()에서 순회 중 리스트가 변경되는 것을 막기 위해
        // 제거 목록에 추가했다가 나중에 처리합니다.
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
public class DiagonalPatternInstance : ISpawnable.ISpawnInstance
{
    private DiagonalMonsterSpawner _parent; // 자신을 생성한 스포너
    private MonsterData _data;
    private bool _spawning;
    private double _lastSpawnTime;
    private float _moveBeat;
    private Coroutine _spawnCoroutine;
    private double _pauseStartTime;
    private float threshold = 0.4f;

    // [NEW] 패턴 순서를 기억할 변수 추가
    private int _patternIdx = 0;

    public DiagonalPatternInstance(DiagonalMonsterSpawner parent, MonsterData data)
    {
        _parent = parent;
        _data = data;
        _moveBeat = data.moveBeat;
        _spawning = true;
        _patternIdx = 0; // 패턴 인덱스 초기화

        // [MOVED] SetLastSpawnTime 로직을 인스턴스 생성 시 처리
        if (IngameData.PhaseDurationSec == 0)
        {
            Debug.LogWarning("Set Up Phase Duration!");
        }
        float moveBeatDuration = 4 * (float)data.moveBeat; // 4번 뛰는데, 한 번 뛸 때마다 moveBeat 박자
        _lastSpawnTime = AudioSettings.dspTime + IngameData.PhaseDurationSec - (IngameData.BeatInterval * moveBeatDuration) + threshold;
    }

    /**
     * 부모 스포너(MonoBehaviour)를 통해 코루틴을 시작합니다.
     */
    public void StartSpawning(MonsterData data)
    {
        float spawnDuration = (float)IngameData.BeatInterval * _data.spawnBeat;
        _spawnCoroutine = _parent.StartCoroutine(DoSpawn(spawnDuration, data));
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
            // 부모의 코루틴을 중지
            _parent.StopCoroutine(_spawnCoroutine);
        }

        // 부모의 관리 리스트에서 자신을 제거하도록 요청
        _parent.RemovePattern(this);
    }

    /**
     * 일시정지 처리 (dspTime 기준)
     */
    public void PauseForWhile(bool isStop, double dspTime)
    {
        // 코루틴은 Time.timeScale에 의해 자동으로 멈추므로,
        // 여기서는 dspTime 기반의 _lastSpawnTime만 보정해줍니다.
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
     * 스폰 코루틴 로직 (기존과 거의 동일 + 패턴 로직 추가)
     */
    private IEnumerator DoSpawn(float spawnDuration, MonsterData data)
    {
        yield return new WaitForSeconds((float)IngameData.BeatInterval * 0.45f);
        while (_spawning)
        {
            // dspTime이 이 인스턴스의 lastSpawnTime을 넘으면 코루틴 종료
            if (AudioSettings.dspTime > _lastSpawnTime)
            {
                Stop(); // UnSpawn() 대신 이 인스턴스의 Stop() 호출
                yield break;
            }

            // [추가됨] WASD_Pattern이 존재하면 파싱하여 targetIndex 결정
            int? targetIndex = null;
            if (!string.IsNullOrEmpty(data.WASD_Pattern))
            {
                if (_patternIdx >= data.WASD_Pattern.Length) _patternIdx = 0; // Loop pattern

                char c = data.WASD_Pattern[_patternIdx];
                if (char.IsDigit(c))
                {
                    targetIndex = (int)char.GetNumericValue(c);
                }

                // 다음 패턴 글자로 이동
                _patternIdx++;
            }

            _parent.ActivateEnemy(_moveBeat, data, targetIndex); // 부모의 공용 메서드 호출 (targetIndex 전달)

            // Time.timeScale의 영향을 받는 WaitForSeconds 사용
            yield return new WaitForSeconds(spawnDuration);
        }
    }
}