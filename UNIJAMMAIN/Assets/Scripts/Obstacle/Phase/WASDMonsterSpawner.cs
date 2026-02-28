using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

public class WASDMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] WASDPosition[] positions;
    [SerializeField] Vector2 sizeDiffRate = new Vector2(0.8f, 1.2f);

    [SerializeField] Collider2D holder;

    public BeatClock beatClock;
    private Dictionary<WASDType, Transform> _spawnPosition;
    private Dictionary<WASDType, Vector2> _targetPosition;
    private HitJudge _rank;
    private Vector3 _playerPos;

    // [MOVED] 패턴별 상태 변수들이 WASDPatternInstance로 이동했습니다.
    // (e.g., _data, _tick, _spawning, _startDsp, _lastSpawnTime, _spawnPointString, _count, _pauseStartTime)

    // [NEW] 현재 활성화된 모든 스폰 패턴(페이지)을 관리하는 리스트
    private List<WASDPatternInstance> _activePatterns = new List<WASDPatternInstance>();
    // [NEW] 리스트 순회 중 안전하게 제거하기 위한 목록
    private List<WASDPatternInstance> _patternsToRemove = new List<WASDPatternInstance>();

    // [MOVED] QA용 변수들은 스포너에 남아있되, 자식 인스턴스가 참조할 수 있도록 getter 제공
    [SerializeField] private int _maxCnt = 1;
    [SerializeField] private int[] _idx = { 0, 1, 2, 3 };
    public int GetMaxCnt() => _maxCnt;
    public int[] GetIdx() => _idx;


    Define.MonsterType ISpawnable.MonsterType => Define.MonsterType.WASD;

    // 패턴 캐싱 저장소(Key: 원본 문자열, Value: 파싱된 커맨드 리스트)
    private Dictionary<string, List<SpawnCommand>> _patternCache = new Dictionary<string, List<SpawnCommand>>();


    private void Start()
    {
        Init();
        _rank = new HitJudge(holder.bounds.size.x, holder.bounds.size.y);
        _playerPos = GameObject.FindWithTag("Player").transform.position;

        Managers.Game.RankUpdate -= UpdateRankCnt;
        Managers.Game.RankUpdate += UpdateRankCnt;
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;

    }

    private void OnDestroy()
    {
        Managers.Game.RankUpdate -= UpdateRankCnt;
        PauseManager.IsPaused -= PauseForWhile;
        _patternCache.Clear(); // 메모리 정리
    }

    private void Init() // (기존과 동일)
    {
        _spawnPosition = new Dictionary<WASDType, Transform>();
        _targetPosition = new Dictionary<WASDType, Vector2>();
        for (int i = 0; i < positions.Length; i++)
        {
            var p = positions[i];
            _spawnPosition[p.WASDType] = p.spawnPos.transform;
            _targetPosition[p.WASDType] = p.targetPos.transform.position;
        }
    }

    private void UpdateRankCnt(RankNode rankNode) // (기존과 동일)
    {
        Vector2 target = _targetPosition[rankNode.WASDT];
        _rank.UpdateRankCnt(rankNode, target);
    }

    /**
     * [REFACTORED] ISpawnable.Spawn
     * 새 스폰 패턴 인스턴스를 생성하고 리스트에 추가한 뒤 반환합니다.
     */
    public ISpawnable.ISpawnInstance Spawn(MonsterData data)
    {
        // 1. 현재 dspTime을 기준으로 새 패턴 인스턴스 생성
        double startDsp = beatClock.GetScheduledDspTimeForTick(beatClock._tick);
        WASDPatternInstance newPattern = new WASDPatternInstance(this, data, startDsp);

        // 2. 활성 리스트에 추가
        _activePatterns.Add(newPattern);

        // 3. 제어 핸들(인스턴스) 반환
        return newPattern;
    }

    /**
     * [REFACTORED] ISpawnable.UnSpawnAll (기존 UnSpawn)
     * 모든 활성 패턴을 중지하고 리스트를 비웁니다.
     */
    public void UnSpawnAll()
    {
        _activePatterns.Clear();
    }

    /**
     * [NEW] 특정 패턴 인스턴스가 스스로 중지할 때 호출됩니다.
     */
    public void RemovePattern(WASDPatternInstance pattern)
    {
        // Update()에서 순회 중 리스트가 변경되는 것을 막기 위해
        // 제거 목록에 추가했다가 나중에 처리합니다.
        _patternsToRemove.Add(pattern);
    }

    /**
     * [REFACTORED] PauseForWhile
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
     * [REFACTORED] Update
     * 모든 활성 패턴의 Tick()을 호출합니다.
     */
    private void Update()
    {
        if (_activePatterns.Count == 0) return;

        double now = AudioSettings.dspTime;

        // 모든 활성 패턴을 순회하며 Tick 실행
        // (리스트 순회 중 삭제가 발생할 수 있으므로 역방향 순회 또는 임시 리스트 사용)
        for (int i = _activePatterns.Count - 1; i >= 0; i--)
        {
            _activePatterns[i].Tick(now);
        }

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

    // [MOVED] DoSpawn, SettingWASD_Type 등은 WASDPatternInstance로 이동했습니다.
    // [MOVED] ScheduledTime, SetLastSpawnTime 등도 WASDPatternInstance로 이동했습니다.


    /**
     * [MODIFIED]
     * 이제 MonsterData를 인스턴스로부터 전달받습니다.
     * 이 메서드는 모든 인스턴스가 공유합니다.
     */
    public void PoolEnemySpawn(WASDType enemyType, MonsterData data,float timeOffset)
    {
        IngameData.TotalMobCnt++;
        EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);

        // 풀에서 가져오기
        var poolable = Managers.Pool.Pop(enemy.go);
        GameObject go = poolable.gameObject;

        // 실시간 위치(.position)를 가져옴으로써 DOTween으로 이동된 좌표가 반영됨
        Vector3 currentMovePos = _spawnPosition[enemyType].position;
        go.transform.position = currentMovePos;


        // [수정] timeOffset 전달
        VariableSetting(go.GetComponent<MovingEnemy>(), enemyType, data, timeOffset, currentMovePos);
    }
    // [수정] timeOffset 파라미터 추가 MonsterData를 인스턴스로부터 전달받습니다.
    private void VariableSetting(MovingEnemy movingEnemy, WASDType type, MonsterData data, float timeOffset,Vector3 currentSpawnPos)
    {
        float distance = Vector3.Distance(currentSpawnPos, _targetPosition[type]);
        // [수정] SetVariance에 timeOffset 전달
        movingEnemy.SetVariance(distance, data, sizeDiffRate, _playerPos, type, data.monsterType, timeOffset);
    }


    #region 데이터 캐싱

    public void InitiallizePatternData()
    {

    }
    public List<SpawnCommand> GetOrParsePattern(string patternString)
    {
        // 1. 이미 파싱해둔 적이 있는지 확인
        if (_patternCache.ContainsKey(patternString))
        {
            return _patternCache[patternString];
        }

        // 2. 없으면 새로 파싱 (아까 수정한 탭 구분 로직 적용)
        List<SpawnCommand> newPatternList = ParsePatternStringInternal(patternString);

        // 3. 캐시에 저장 후 리턴
        _patternCache.Add(patternString, newPatternList);
        return newPatternList;
    }
    //내 부 파싱 로직
    private List<SpawnCommand> ParsePatternStringInternal(string pattern)
    {
        List<SpawnCommand> result = new List<SpawnCommand>();
        if (string.IsNullOrEmpty(pattern)) return result;

        // 탭(\t) 기준으로 쪼개서 빈 셀 처리 확실하게 하기
        string[] cells = pattern.Split('\t');

        foreach (string rawCell in cells)
        {
            string cellContent = rawCell.Trim();

            // 빈 셀(블랭크) 처리
            if (string.IsNullOrEmpty(cellContent))
            {
                result.Add(new SpawnCommand { IsEmpty = true });
                continue;
            }

            // 셀 내부 파싱
            int len = cellContent.Length;
            int i = 0;
            while (i < len)
            {
                char c = cellContent[i];

                if (c == ',' || c == '/' || c == ' ')
                {
                    i++; continue;
                }

                if (c == '(') // 그룹 패턴
                {
                    i++;
                    List<WASDType> groupTypes = new List<WASDType>();
                    while (i < len && cellContent[i] != ')')
                    {
                        WASDType type = SettingWASD_Type(cellContent[i]);
                        if (type != WASDType.None) groupTypes.Add(type);
                        i++;
                    }

                    if (groupTypes.Count > 0)
                        result.Add(new SpawnCommand { Types = groupTypes.ToArray(), IsRandom = false, IsEmpty = false });
                    else
                        result.Add(new SpawnCommand { IsEmpty = true });

                    i++;
                }
                else // 단일 패턴
                {
                    WASDType type = SettingWASD_Type(cellContent[i]);

                    if (type == WASDType.Random)
                        result.Add(new SpawnCommand { IsRandom = true, IsEmpty = false });
                    else if (type != WASDType.None)
                        result.Add(new SpawnCommand { Types = new WASDType[] { type }, IsRandom = false, IsEmpty = false });
                    else
                        result.Add(new SpawnCommand { IsEmpty = true }); // X, N 등

                    i++;
                }
            }
        }
        return result;
    }

    private WASDType SettingWASD_Type(char c)
    {
        switch (char.ToUpper(c))
        {
            case 'W': return WASDType.W;
            case 'A': return WASDType.A;
            case 'S': return WASDType.S;
            case 'D': return WASDType.D;
            case 'R': return WASDType.Random;
            case 'O': return WASDType.Random;
            default: return WASDType.None;
        }
    }

    #endregion

    #region QA
    public void QAUpdateVariables(Vector2 sizeDiffRate, int[] idx, int maxCnt)
    {
        this.sizeDiffRate = sizeDiffRate;
        this._maxCnt = Mathf.Clamp(maxCnt, 1, 4);
        this._idx = idx;

        // [NEW] 이미 실행 중인 패턴에도 QA 변경사항을 적용할 수 있습니다 (선택적)
        // foreach(var pattern in _activePatterns)
        // {
        //     pattern.UpdateQASettings(_maxCnt, _idx);
        // }
    }
    #endregion
}