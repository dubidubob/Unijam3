using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
struct RankUI {
    public float cutline;
    public Sprite img;
    public string ment1;
    public string ment2;

    public string GetRandomMent()
    {
        // 0 또는 1 중 랜덤으로 뽑기
        int rand = UnityEngine.Random.Range(0, 2);
        return (rand == 0) ? ment1 : ment2;
    }
}
public class ResultUI : MonoBehaviour
{
    [Header("UI 자리")]
    [SerializeField] private Image resultImg;
    [SerializeField] private TMP_Text resultTxt;
    [SerializeField] private TMP_Text resultScore;
    [Header("UI 정보 : 최상, 상, 중상, 중, 하 순으로")]
    [SerializeField] List<RankUI> rankUI;

    private void Awake()
    {
        if (rankUI.Count < 5)
            Debug.LogWarning("rank ui 정보 채워넣으셈");
    }

    public void OnClick()
    {
        Time.timeScale = 1f;                 // 혹시 일시정지 풀기
        SceneLoadingManager.Instance.LoadScene("StageScene");
    }

    public void ChangeUI(float score)
    {
        // TODO : UI 로직과 데이터 처리 로직 분리 권장 (일단 작동하도록 수정함)
        resultScore.text = score.ToString("0.##") + "점";

        int idx;
        Define.Rank rank;

        // 1. 먼저 이번 판의 등급(Rank) 결정
        if (score >= rankUI[0].cutline) { idx = 0; rank = Define.Rank.Perfect; } // 최상
        else if (score >= rankUI[1].cutline) { idx = 1; rank = Define.Rank.Good; }    // 상
        else if (score >= rankUI[2].cutline) { idx = 2; rank = Define.Rank.NormalGood; } // 중상
        else if (score >= rankUI[3].cutline) { idx = 3; rank = Define.Rank.Normal; }  // 중
        else { idx = 4; rank = Define.Rank.Bad; }     // 하

        // UI 업데이트 (결과 보여주기)
        resultImg.sprite = rankUI[idx].img;
        resultImg.SetNativeSize();
        resultTxt.text = rankUI[idx].GetRandomMent();

        // 연습모드가 아닐 때만 저장 및 업적 체크 수행
        if (!IngameData.boolPracticeMode)
        {
            // 데이터 저장 (Best Score, Rank) - 업적 체크보다 먼저 해야 함!
            // (기존 코드에 있던 BestScore 저장은 밖으로 뺐습니다. 연습모드 기록 저장 여부에 따라 위치 조정하세요)
            IngameData.BestChapterScore = score;
            IngameData.ChapterRank = rank; // 이번 판 기록 확정 저장

            // 개별 업적 : 0 챕터가 아닌 곳에서 첫 최상 등급
            if (rank == Define.Rank.Perfect && IngameData.ChapterIdx != 0)
            {
                Managers.Steam.UnlockAchievement("ACH_RANK_HIGHEST_FIRST");
            }

            // 전체 달성 업적 체크 (저장이 끝났으므로 안전하게 검사 가능)
            if (rank == Define.Rank.Perfect) // 이번이 Perfect일 때만 검사하면 됨 (최적화)
            {
                CheckAllPerfectSteamAchievement();
            }

            if (score >= 100) // 이번이 100점일 때만 검사 (최적화)
            {
                CheckAll100SCOREAchievement();
            }
        }
    }

    // Steam : 모든 스테이지의 등급이 최상(Perfect)인지 확인
    private void CheckAllPerfectSteamAchievement()
    {
        for (int i = 0; i < 7; i++)
        {
            // 하나라도 Perfect가 아니면 즉시 종료
            if (IngameData.GetBestRankForChapter(i) != Define.Rank.Perfect)
            {
                return;
            }
        }
        // 루프를 다 통과했다면 모두 Perfect임
        Managers.Steam.UnlockAchievement("ACH_RANK_HIGHEST_ALL");
    }

    // Steam : 모든 스테이지의 점수가 100점인지 확인
    private void CheckAll100SCOREAchievement()
    {
        for (int i = 0; i < 7; i++)
        {
            // 하나라도 100점이 아니면 즉시 종료
            // (float 비교이므로 확실하게 하려면 >= 100 또는 Mathf.Approximately 사용 권장)
            if (IngameData.GetBestRankScoreForChapter(i) < 100)
            {
                return;
            }
        }
        Managers.Steam.UnlockAchievement("ACH_SCORE_100_ALL");
    }

}