using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading; // CancellationToken 사용을 위해 필요

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
    [SerializeField] private Image resultNewRecordImg;
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
            if (score < IngameData.BestChapterScore) // 최고기록 달성 완료!
            {
                BestRecordAchieve();
            }
            IngameData.BestChapterScore = score; // 자등으로 최고기록 저장
           
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

    /// <summary>
    /// 최고 기록을 달성했을때, 신기록 이미지를 띄워주는 코드
    /// </summary>
    public void BestRecordAchieve()
    {
        resultNewRecordImg.color = new Color(1, 1, 1, 1); // 색깔 활성화
        ImageAlphaBrightning(resultNewRecordImg).Forget();
    }

    // ==================================================================================
    // ▼ 구현된 부분 ▼
    // ==================================================================================
    private async UniTask ImageAlphaBrightning(Image targetImg)
    {
        // 방어 코드: 타겟 이미지가 없으면 실행 취소
        if (targetImg == null) return;

        // 1. 중요: 이 객체(ResultUI)가 파괴될 때 루프를 멈추기 위한 토큰 획득
        CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

        // 2. 초기 상태 저장 (기준점)
        Vector3 initialLocalPos = targetImg.transform.localPosition;
        Color initialColor = targetImg.color; // 원래 색상(RGB) 유지용

        // 3. 애니메이션 설정값 (취향에 맞게 조절하세요)
        float alphaSpeed = 3f;       // 깜빡임 속도
        float minAlpha = 0.7f;       // 최소 투명도
        float maxAlpha = 1.0f;       // 최대 투명도

        float moveSpeed = 2f;        // 둥둥 떠다니는 속도
        float moveRangeY = 10f;      // 위아래 이동 범위

        // 무한 루프를 돌며 애니메이션 수행
        while (true)
        {
            // [Alpha 애니메이션]
            // PingPong: 0에서 1 사이를 지정된 속도로 왕복
            float t = Mathf.PingPong(Time.unscaledTime * alphaSpeed, 1f);
            // Lerp: t값에 따라 minAlpha와 maxAlpha 사이를 보간
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            targetImg.color = new Color(initialColor.r, initialColor.g, initialColor.b, currentAlpha);

            // [위치 애니메이션]
            // Sin 그래프를 이용해 -1 ~ 1 사이 값을 얻고 범위(moveRangeY)를 곱함
            float yOffset = Mathf.Sin(Time.unscaledTime * moveSpeed) * moveRangeY;
            targetImg.transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y + yOffset, initialLocalPos.z);

            // 다음 프레임까지 대기 (cancellationToken을 전달하여 파괴 시 루프 탈출)
            // PlayerLoopTiming.Update: 매 프레임 업데이트 시점
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
    }
    private void OnDestroy()
    {
        
    }
}