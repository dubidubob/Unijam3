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
    // --- (기존 Enum 정의 참고용) ---
    // LeftUp=0, LeftDown=1, RightUp=2, RightDown=3

    enum RankState { Spawned, Success, Fail };
    public Define.MonsterType MonsterType => Define.MonsterType.Diagonal;
    private Dictionary<DiagonalType, GameObject> diagonalDict;
    private List<int> activatedDiagonalIdx = new List<int>();
    private List<int> deactivatedDiagonalIdx = new List<int>();
    private int attackValue = 20;
    private int spawnedDiagonalMobCnt = 0; // 튜토리얼용 카운트

    // [NEW] 성능 최적화를 위한 룩업 테이블 (Switch문 대체)
    // 인덱스: 입력값(0~4) -> 값: Enum Int값
    // 0:쉼, 1:RU(2), 2:LU(0), 3:LD(1), 4:RD(3)
    public readonly int[] PatternToEnumMap = { -1, 2, 0, 1, 3 };

    private List<DiagonalPatternInstance> _activePatterns = new List<DiagonalPatternInstance>();
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

    // --- (기존 로직 유지) ---
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

        foreach (var m in dm)
        {
            diagonalDict[m.DiagonalT] = m.gameObject;
            deactivatedDiagonalIdx.Add((int)m.DiagonalT);
        }
    }

    // =================================================================
    // [MODIFIED] ActivateEnemy
    // targetEnumIdx: 매핑이 완료된 실제 Enum의 int값 (0~3). null이면 랜덤.
    // =================================================================
    public void ActivateEnemy(float moveBeat, MonsterData data, int? targetEnumIdx = null)
    {
        UpdateRankCnt(RankState.Spawned);

        int mIdx = -1;

        // 1. 지정된 패턴(Enum Index)이 있는 경우
        if (targetEnumIdx.HasValue)
        {
            int reqIdx = targetEnumIdx.Value;

            // 이미 사용 중(activated)이거나 없는 인덱스라면 이번 턴은 스킵 (겹침 방지)
            if (deactivatedDiagonalIdx.Contains(reqIdx))
            {
                mIdx = reqIdx;
                deactivatedDiagonalIdx.Remove(mIdx);
            }
            else
            {
                // 소환 불가(이미 나와있음) -> Rest
                return;
            }
        }
        // 2. 랜덤 로직 (기존)
        else
        {
            int idx = Random.Range(0, deactivatedDiagonalIdx.Count);

            if (IngameData.ChapterIdx == 0 && spawnedDiagonalMobCnt < 2)
            {
                idx = (spawnedDiagonalMobCnt == 0) ? (int)DiagonalType.RightUp : (int)DiagonalType.LeftDown;
                spawnedDiagonalMobCnt++;
            }

            if (deactivatedDiagonalIdx.Count == 0) return;

            mIdx = deactivatedDiagonalIdx[idx];
            deactivatedDiagonalIdx.Remove(mIdx);
        }

        // 실제 오브젝트 활성화
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

    // --- (ISpawnable 구현) ---
    public ISpawnable.ISpawnInstance Spawn(MonsterData data)
    {
        DiagonalPatternInstance instance = new DiagonalPatternInstance(this, data);
        _activePatterns.Add(instance);
        instance.StartSpawning(data);
        return instance;
    }

    public void UnSpawnAll()
    {
        StopAllCoroutines();
        _activePatterns.Clear();
        _patternsToRemove.Clear();

        foreach (var idx in activatedDiagonalIdx)
        {
            diagonalDict[(DiagonalType)idx].GetComponent<DiagonalMonster>().SetDead(false);
        }
        activatedDiagonalIdx.Clear();
        deactivatedDiagonalIdx.Clear();
        InitialDict();
    }

    public void PauseForWhile(bool isStop)
    {
        double dspTime = AudioSettings.dspTime;
        foreach (var pattern in _activePatterns)
        {
            pattern.PauseForWhile(isStop, dspTime);
        }
    }

    public void RemovePattern(DiagonalPatternInstance pattern)
    {
        _patternsToRemove.Add(pattern);
    }

    private void Update()
    {
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
// [MODIFIED] DiagonalPatternInstance
// =======================================================================
public class DiagonalPatternInstance : ISpawnable.ISpawnInstance
{
    private DiagonalMonsterSpawner _parent;
    private MonsterData _data;
    private bool _spawning;
    private double _lastSpawnTime;
    private float _moveBeat;
    private Coroutine _spawnCoroutine;
    private double _pauseStartTime;
    private float threshold = 0.4f;
    private int _patternIdx = 0;

    public DiagonalPatternInstance(DiagonalMonsterSpawner parent, MonsterData data)
    {
        _parent = parent;
        _data = data;
        _moveBeat = data.moveBeat;
        _spawning = true;
        _patternIdx = 0;

        if (IngameData.PhaseDurationSec == 0) Debug.LogWarning("Set Up Phase Duration!");

        float moveBeatDuration = 4 * (float)data.moveBeat;
        _lastSpawnTime = AudioSettings.dspTime + IngameData.PhaseDurationSec - (IngameData.BeatInterval * moveBeatDuration) + threshold;
    }

    public void StartSpawning(MonsterData data)
    {
        float spawnDuration = (float)IngameData.BeatInterval * _data.spawnBeat;
        _spawnCoroutine = _parent.StartCoroutine(DoSpawn(spawnDuration, data));
    }

    public void Stop()
    {
        _spawning = false;
        if (_spawnCoroutine != null && _parent != null) _parent.StopCoroutine(_spawnCoroutine);
        _parent.RemovePattern(this);
    }

    public void PauseForWhile(bool isStop, double dspTime)
    {
        if (isStop) _pauseStartTime = dspTime;
        else if (_pauseStartTime > 0)
        {
            _lastSpawnTime += (dspTime - _pauseStartTime);
            _pauseStartTime = 0;
        }
    }

    // =================================================================
    // [핵심 수정] DoSpawn: 배열 참조(O(1))로 분기문 제거
    // =================================================================
    private IEnumerator DoSpawn(float spawnDuration, MonsterData data)
    {
        yield return new WaitForSeconds((float)IngameData.BeatInterval * 0.45f);

        while (_spawning)
        {
            if (AudioSettings.dspTime > _lastSpawnTime)
            {
                Stop();
                yield break;
            }

            // 패턴 문자열이 있을 때만 동작
            if (!string.IsNullOrEmpty(data.WASD_Pattern))
            {
                if (_patternIdx >= data.WASD_Pattern.Length) _patternIdx = 0;

                // 1. char를 int(0~9)로 변환
                int inputNum = data.WASD_Pattern[_patternIdx] - '0';

                // 2. 유효 범위(0~4) 체크
                // 배열 인덱스 접근이므로 범위 체크는 필수지만, 조건문 1회라 매우 빠름
                if (inputNum >= 0 && inputNum < _parent.PatternToEnumMap.Length)
                {
                    // 3. 룩업 테이블에서 실제 Enum Index 가져오기
                    int mappedEnumIdx = _parent.PatternToEnumMap[inputNum];

                    // mappedEnumIdx가 -1이면 '쉼(0)'이므로 아무것도 안함
                    if (mappedEnumIdx != -1)
                    {
                        _parent.ActivateEnemy(_moveBeat, data, mappedEnumIdx);
                    }
                }

                _patternIdx++;
            }
            else
            {
                // 패턴 없으면 랜덤 모드 (null 전달)
                _parent.ActivateEnemy(_moveBeat, data, null);
            }

            yield return new WaitForSeconds(spawnDuration);
        }
    }
}