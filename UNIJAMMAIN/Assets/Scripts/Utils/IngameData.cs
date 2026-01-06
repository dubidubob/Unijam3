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

    public static bool Pause { set; get; }
    private static double beatInterval;
    public static bool IsStart = false;
    public static int StageProgress = 0;
    public static int DefeatEnemyCount = 0;

    static IngameData()
    {
        _chapterRanks = new Define.Rank[TOTAL_CHAPTERS];
        _bestChapterRanks = new Define.Rank[TOTAL_CHAPTERS];
        _bestChapterScore = new float[TOTAL_CHAPTERS];
        for (int i = 0; i < TOTAL_CHAPTERS; i++)
        {
            _chapterRanks[i] = Define.Rank.Unknown;
            _bestChapterRanks[i] = Define.Rank.Unknown;
            _bestChapterScore[i] = 0;
        }
        Debug.Log("IngameData이 시작될때 초기화되었습니다.");
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
}