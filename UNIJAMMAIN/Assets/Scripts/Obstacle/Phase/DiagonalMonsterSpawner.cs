using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

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
        if (deactivatedDiagonalIdx.Count == 0)
        {
            Debug.LogWarning("가용 가능한 대각선 몬스터 오브젝트가 없습니다!");
            return;
        }

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
            int idx = UnityEngine.Random.Range(0, deactivatedDiagonalIdx.Count);

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
            // deactivatedDiagonalIdx.Add((int)attackType);
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


        //  핵심 수정: foreach 도중 리스트가 수정되는 에러를 막기 위해 임시 복사본(tempList)을 만듭니다.
        List<int> tempList = new List<int>(activatedDiagonalIdx);

        foreach (var idx in tempList)
        {
            var mob = diagonalDict[(DiagonalType)idx].GetComponent<DiagonalMonster>();
            mob.SetDead(false, true);
            // [추가] 즉시 비활성화시켜 OnDisable()을 유도하고 잔여 UniTask를 취소시킴
            mob.gameObject.SetActive(false);
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
    // 2-2. RecycleMonsterIndex 함수 수정: 중복 추가 방지
    public void RecycleMonsterIndex(GamePlayDefine.DiagonalType type)
    {
        int t = (int)type;
        if (activatedDiagonalIdx.Contains(t))
        {
            activatedDiagonalIdx.Remove(t);
        }

        // [수정] 대기열에 없을 때만 추가 (안전장치)
        if (!deactivatedDiagonalIdx.Contains(t))
        {
            deactivatedDiagonalIdx.Add(t);
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
    private CancellationTokenSource _spawnCts;

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

        // 기존 실행 중인 Task가 있다면 취소
        _spawnCts?.Cancel();
        _spawnCts?.Dispose();
        _spawnCts = new CancellationTokenSource();

        // 코루틴 대신 UniTask 호출 (Fire and Forget)
        DoSpawn(spawnDuration, data, _spawnCts.Token).Forget();
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

    private async UniTaskVoid DoSpawn(float spawnDuration, MonsterData data, CancellationToken token)
    {
        double nextSpawnTime = AudioSettings.dspTime + ((float)IngameData.BeatInterval * 0.45f);

        try
        {
            while (_spawning)
            {
                // Delay를 쓰지 않고 dspTime을 기준으로 다음 목표 시간까지 매 프레임 대기
                while (AudioSettings.dspTime < nextSpawnTime)
                {
                    // 취소 요청이 들어오면 루프 즉시 탈출
                    token.ThrowIfCancellationRequested();

                    // yield return null 과 완벽히 동일한 역할 (GC 없음)
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                // --- 몬스터 스폰 로직 ---
                if (!string.IsNullOrEmpty(data.WASD_Pattern))
                {
                    if (_patternIdx >= data.WASD_Pattern.Length) _patternIdx = 0;

                    char currentPatternChar = data.WASD_Pattern[_patternIdx];
                    if (currentPatternChar == 'X' || currentPatternChar == 'x')
                    {
                        // 스킵
                    }
                    else
                    {
                        int inputNum = currentPatternChar - '0';
                        if (inputNum >= 0 && inputNum < _parent.PatternToEnumMap.Length)
                        {
                            int mappedEnumIdx = _parent.PatternToEnumMap[inputNum];
                            if (mappedEnumIdx != -1)
                            {
                                _parent.ActivateEnemy(_moveBeat, data, mappedEnumIdx);
                            }
                        }
                    }
                    _patternIdx++;
                }
                else
                {
                    _parent.ActivateEnemy(_moveBeat, data, null);
                }

                // 다음 스폰 목표 시간 갱신 (오차 누적 방지)
                nextSpawnTime += spawnDuration;
            }
        }
        catch (OperationCanceledException)
        {
            // Stop()에 의해 정상적으로 취소된 경우 예외를 잡아줍니다.
            // 필요하다면 여기서 정리 작업을 할 수 있습니다.
        }
    }

}