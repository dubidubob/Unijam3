using System;
using static GamePlayDefine;
using UnityEngine;

public static class IngameData 
{
    public static Action ChangeBpm;
    public static Action<RankType> OnRankUpdate;

    public static bool Pause { set; get; }
    private static double beatInterval;
    public static bool IsStart = false;

    static IngameData()
    {
        _chapterRanks = new Define.Rank[TOTAL_CHAPTERS];
        for (int i = 0; i < TOTAL_CHAPTERS; i++)
        {
            _chapterRanks[i] = Define.Rank.Unknown;
        }
        Debug.Log("IngameData�� ���۵ɶ� �ʱ�ȭ�Ǿ����ϴ�.");
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

    private static Define.Rank _chapterRank = Define.Rank.Unknown;
    //������ ��ũ�� ������ ���� �޶���
    private const int TOTAL_CHAPTERS = 8;
    public static Define.Rank[] _chapterRanks = new Define.Rank[TOTAL_CHAPTERS];
   
   

    // ChapterRank ������Ƽ�� ���� ���� ���õ� ChapterIdx�� �ش��ϴ� �迭�� ���� �ٷ�
    public static Define.Rank ChapterRank
    {
        // get: ���� ChapterIdx�� �´� ��ũ�� �迭���� ������
        get
        {
            // �ε��� ���� üũ
            if (ChapterIdx < 0 || ChapterIdx >= TOTAL_CHAPTERS)
            {
                return Define.Rank.Unknown;
            }
            return _chapterRanks[ChapterIdx];
        }
        // set: ���� ChapterIdx�� �´� ��ġ�� ��ũ�� ������
        set
        {
            // �ε��� ���� üũ (���� ��ġ)
            if (ChapterIdx >= 0 && ChapterIdx < TOTAL_CHAPTERS)
            {
                _chapterRanks[ChapterIdx] = value;
            }
        }
    }
    /// <summary>
    /// Ư�� é���� ��ũ ���� ��������
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