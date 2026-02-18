using System;
using static GamePlayDefine;
using UnityEngine;

public static class IngameData 
{
    public static Action ChangeBpm;
    public static Action<RankType> OnRankUpdate;
    public static Action<GamePlayDefine.WASDType> OnPerfectEffect;
    public static int GameBpm;
    public static bool boolPracticeMode;
    [Range(-0.2f, 0.2f)] public static float sinkTimer =0; // 노래를 빨리 재생시키거나 늦게재생시키는 타이머, 늦게 재생시키면 몬스터가 더 빨리옴.


    public static bool Pause { set; get; }
    private static double beatInterval;
    public static bool IsStart = false;

    public static int StageProgress = 0; // 스테이지 레벨과 관련도니 변수, "" 장을 한번만 띄우게끔 조절
    public static int _defeatEnemyCount = 0;
    /// <summary>
    /// 현재 플레이어가 클리어한 최대 스테이지 (스토리기준)
    /// </summary>
    public static int _clearStageIndex = 0;

    /// <summary>
    /// 현재 플레이어가 진입중인 스테이지
    /// </summary>
    public static int _nowStageIndex = 0; 

    static IngameData()
    {
        // 배열 초기화
        ClearMemoryData();

        // 데이터 로드 시도
        LoadGameData();
    }
    // 저장할 데이터
    [System.Serializable]
    public class SaveDataContainer
    {
        public int DefeatEnemyCount;
        public float[] BestChapterScore;
        public Define.Rank[] BestChapterRanks;
        public int ClearStoryStageIndex;
        public int NowStoryStageIndex;
        // 필요한 변수가 더 있다면 여기에 추가 (public이어야 저장됨)
    }

    

    public static double BeatInterval 
    {
        set
        {
            beatInterval = value;
            ChangeBpm?.Invoke();
        }
        get
        {
            return beatInterval;
        } 
    }

    public static int ChapterIdx { set; get; }

    //저장할 랭크의 개수에 따라 달라짐
    private const int TOTAL_CHAPTERS = 11;
    public static Define.Rank[] _chapterRanks;
    public static Define.Rank[] _bestChapterRanks;
    public static float[] _bestChapterScore;
    public static float BestChapterScore
    {
        get
        {
            // 인덱스 범위 체크
            if (ChapterIdx < 0 || ChapterIdx >= TOTAL_CHAPTERS)
            {
                return 0;
            }
            return _bestChapterScore[ChapterIdx];
        }
        set
        {
            // 인덱스 범위 체크 (안전 장치)
            if (ChapterIdx >= 0 && ChapterIdx < TOTAL_CHAPTERS)
            {
                if (_bestChapterScore[ChapterIdx] < value)
                {
                    _bestChapterScore[ChapterIdx] = value;
                }
            }
        }
    }


    // ChapterRank 프로퍼티는 이제 현재 선택된 ChapterIdx에 해당하는 배열의 값을 다룸
    public static Define.Rank ChapterRank
    {
        // get: 현재 ChapterIdx에 맞는 랭크를 배열에서 가져옴
        get
        {
            // 인덱스 범위 체크
            if (ChapterIdx < 0 || ChapterIdx >= TOTAL_CHAPTERS)
            {
                return Define.Rank.Unknown;
            }
            return _chapterRanks[ChapterIdx];
        }
        // set: 현재 ChapterIdx에 맞는 위치에 랭크를 저장함
        set
        {
            // 인덱스 범위 체크 (안전 장치)
            if (ChapterIdx >= 0 && ChapterIdx < TOTAL_CHAPTERS)
            {
                _chapterRanks[ChapterIdx] = value;
                if (_bestChapterRanks[ChapterIdx] < value)
                {
                    _bestChapterRanks[ChapterIdx] = value;
                }
            }
        }
    }
    /// <summary>
    /// 특정 챕터의 랭크 직접 가져오기
    /// </summary>
    /// <param name="chapterIndex"></param>
    /// <returns></returns>
    public static Define.Rank GetRankForChapter(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= TOTAL_CHAPTERS)
        {
            return Define.Rank.Unknown;
        }
        return _chapterRanks[chapterIndex];
    }

    public static Define.Rank GetBestRankForChapter(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= TOTAL_CHAPTERS)
        {
            return Define.Rank.Unknown;
        }
        return _bestChapterRanks[chapterIndex];
    }

    public static float GetBestRankScoreForChapter(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= TOTAL_CHAPTERS)
        {
            return 0;
        }
        return _bestChapterScore[chapterIndex];
    }

    public static double PhaseDurationSec { set; get; }
    public static int TotalMobCnt { set; get; }

    public static int PerfectMobCnt { get; private set; }
    public static int GoodMobCnt { get; private set; }
    public static int WrongInputCnt { get; private set; }
    public static int AttackedMobCnt { get; private set; }

    public static void IncPerfect() { PerfectMobCnt++; OnRankUpdate?.Invoke(RankType.Perfect); }
    public static void IncGood() { GoodMobCnt++; OnRankUpdate?.Invoke(RankType.Good); }
    public static void IncWrong() { WrongInputCnt++; OnRankUpdate?.Invoke(RankType.Miss); }
    public static void IncAttacked() { AttackedMobCnt++; OnRankUpdate?.Invoke(RankType.Miss); }

    public static void ActionPerfectEffect(WASDType wasdType) { OnPerfectEffect?.Invoke(wasdType); }

    public static void RankInit()
    {
        Pause = false;
        TotalMobCnt = 0;
        PerfectMobCnt = 0;
        GoodMobCnt = 0;
        WrongInputCnt = 0;
        AttackedMobCnt = 0;
    }

    // 세이브로드

    // 데이터 세이브
    public static void SaveGameData()
    {
        SaveDataContainer data = new SaveDataContainer();
        data.DefeatEnemyCount = _defeatEnemyCount;
        data.BestChapterScore = _bestChapterScore;
        data.BestChapterRanks = _bestChapterRanks;
        data.ClearStoryStageIndex = _clearStageIndex;
        data.NowStoryStageIndex = _nowStageIndex;

        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString("IngameSaveData", json);
        PlayerPrefs.Save();

        Debug.Log($"게임 데이터 저장 완료: 스테이지 {data.ClearStoryStageIndex}");
    }

    // 로드
    public static void LoadGameData()
    {
        if (PlayerPrefs.HasKey("IngameSaveData"))
        {
            string json = PlayerPrefs.GetString("IngameSaveData");
            SaveDataContainer data = JsonUtility.FromJson<SaveDataContainer>(json);

            if (data != null)
            {
                _defeatEnemyCount = data.DefeatEnemyCount;
                _clearStageIndex = data.ClearStoryStageIndex;

                if (data.BestChapterScore != null && data.BestChapterScore.Length == TOTAL_CHAPTERS)
                    _bestChapterScore = data.BestChapterScore;

                if (data.BestChapterRanks != null && data.BestChapterRanks.Length == TOTAL_CHAPTERS)
                    _bestChapterRanks = data.BestChapterRanks;
            }
        }
        else
        {
            Debug.Log("저장된 데이터가 없습니다.");
        }
    }

    public static void ResetData()
    {
        // 1. 파일 삭제
        PlayerPrefs.DeleteKey("IngameSaveData");

        // 2. ★ 수정 5: 메모리 상의 데이터도 반드시 초기화해줘야 함
        // (안 그러면 리셋 버튼 누르고 바로 게임 시작하면 옛날 점수가 그대로 있음)
        ClearMemoryData();

        Debug.Log("데이터 리셋 및 메모리 초기화 완료");
    }

    // ★ 수정 2: 메모리 상의 데이터를 초기화하는 별도 함수 분리
    private static void ClearMemoryData()
    {
        // [핵심 수정] 배열이 비어있다면(null) 새로 생성(new)해주는 코드 추가
        if (_chapterRanks == null) _chapterRanks = new Define.Rank[TOTAL_CHAPTERS];
        if (_bestChapterRanks == null) _bestChapterRanks = new Define.Rank[TOTAL_CHAPTERS];
        if (_bestChapterScore == null) _bestChapterScore = new float[TOTAL_CHAPTERS];

        // 이제 배열이 확실히 존재하므로 안심하고 값을 채워도 됨
        for (int i = 0; i < TOTAL_CHAPTERS; i++)
        {
            _chapterRanks[i] = Define.Rank.Unknown;       // 여기서 오류 안 남
            _bestChapterRanks[i] = Define.Rank.Unknown;
            _bestChapterScore[i] = 0;
        }

        _defeatEnemyCount = 0;
        _nowStageIndex = 0;
        _clearStageIndex = 0;
    }


}