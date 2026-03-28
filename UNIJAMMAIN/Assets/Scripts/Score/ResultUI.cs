using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading; // CancellationToken 사용을 위해 필요
using DG.Tweening;

[Serializable]
struct RankUI
{
    public float cutline;
    public Sprite img;
    [Header("출력할 텍스트 번호들 (예: 0, 1)")]
    public List<int> mentIndices; // 이 커트라인에서 사용할 텍스트 번호들

    // 현재 챕터 인덱스를 매개변수로 받습니다.
    public string GetRandomMent(int chapterIdx)
    {
        // 등록된 멘트 번호가 없다면 빈 문자열 반환
        if (mentIndices == null || mentIndices.Count == 0) return "";

        // 등록된 번호 중 랜덤으로 하나를 뽑습니다.
        int randIdx = mentIndices[UnityEngine.Random.Range(0, mentIndices.Count)];

        // 로컬라이제이션 Key 조합 (예: GameClear_Stage0_Text0)
        string key = $"GameClear_Stage{ChapterStringKeyReturn(chapterIdx)}_Text{randIdx}"; 

        // LocalizationManager에서 번역된 텍스트 가져오기 (키가 없을 경우 대비 fallback 추가)
        return LocalizationManager.Get(key, "멘트를 찾을 수 없습니다.");
    }

    private string ChapterStringKeyReturn(int chapterIdx)
    {
        switch (chapterIdx)
        {
            case 9: return "Christmas";
            case 10: return "EDM";
            case 11: return "Night";
            default: return chapterIdx.ToString();
        }
    }
}

public class ResultUI : MonoBehaviour
{
    [Header("UI 자리")]
    [SerializeField] private Image resultImg;
    [SerializeField] private TMP_Text resultTxt;
    [SerializeField] private TMP_Text resultScore;
    [SerializeField] private TMP_Text newEventUnlockTxt;
    [SerializeField] private TMP_Text smallInfo_Text;
    [SerializeField] private Image resultNewRecordImg;

    [Header("UI 정보: 최상,상,중상,중,하 순으로")]
    [SerializeField] List<RankUI> rankUI;
    [SerializeField] private CanvasGroup lateUpCanvasGroup;

    private void Awake()
    {
        if (rankUI.Count < 5)
            Debug.LogWarning("rankui 정보 채워넣으셈");

        resultTxt.text = "";
        newEventUnlockTxt.text = "";
        newEventUnlockTxt.alpha = 0;
        lateUpCanvasGroup.alpha = 0;
    }

    public void OnClick()
    {
        if (isAnimationPlaying) return;

        Time.timeScale = 1f; // 혹시 일시정지 풀기

        if (IngameData.ChapterIdx == 7 && !IngameData.boolPracticeMode) // 마지막이라면 엔딩씬으로 이동, 연습모드도 아니어야 함
        {
            SceneLoadingManager.Instance.LoadScene("EndingScene");
        }
        else
        {
            SceneLoadingManager.Instance.LoadScene("StageScene");
        }
    }

    // 1. 비동기 작업 관리용 필드 추가
    private bool isAnimationPlaying = false;

    // ChangeUI의 매개변수를 가져와서 통합했습니다.
    public async UniTask RankAnimation(float score)
    {
        isAnimationPlaying = true;

        // 🚨 1. 오브젝트 파괴 시 취소를 위한 토큰 발급
        CancellationToken ct = this.GetCancellationTokenOnDestroy();

        try
        {
            string formattedScore = score.ToString("F0");

            // 2. 번역 시트에서 키(Key) 값으로 현재 언어에 맞는 텍스트를 가져옴
            string localizedTemplate = LocalizationManager.Get("GameClear_ScoreExPression_Text");

            // 3. 가져온 텍스트 안의 "{Score}" 부분을 실제 점수로 바꿔치기
            resultScore.text = localizedTemplate.Replace("{Score}", formattedScore);

            int idx;
            Define.Rank rank;

            if (score == rankUI[0].cutline) { idx = 0; rank = Define.Rank.Perfect; } // 100점
            else if (score >= rankUI[1].cutline) { idx = 1; rank = Define.Rank.Perfect; } // 최상
            else if (score >= rankUI[2].cutline) { idx = 2; rank = Define.Rank.Good; } // 상
            else if (score >= rankUI[3].cutline) { idx = 3; rank = Define.Rank.NormalGood; } // 중상
            else if (score >= rankUI[4].cutline) { idx = 4; rank = Define.Rank.Normal; } // 중
            else { idx = 5; rank = Define.Rank.Bad; } // 하

            // 2. 초기 상태 설정 (애니메이션을 위해 UI 숨기거나 제자리 배치)
            // 🚨 2. 모든 DOTween에 SetLink(gameObject) 추가
            resultImg.DOFade(0, 0).SetLink(gameObject);
            Managers.Sound.Play("SFX/Results/1Result");

            // 🚨 3. Delay에 cancellationToken 추가
            await UniTask.Delay(TimeSpan.FromSeconds(2f), ignoreTimeScale: true, cancellationToken: ct);

            resultImg.DOFade(1f, 0).SetLink(gameObject);
            RankImpactSoundPlay(rank);

            resultImg.sprite = rankUI[idx].img;
            resultImg.SetNativeSize();

            RectTransform imgRect = resultImg.GetComponent<RectTransform>();
            imgRect.anchoredPosition = new Vector2(0f, imgRect.anchoredPosition.y); // resultImg PosX=0에 두기

            await UniTask.Delay(TimeSpan.FromSeconds(2f), ignoreTimeScale: true, cancellationToken: ct);

            resultTxt.text = ""; // DOText 타이핑 효과를 위해 텍스트 비우기
            lateUpCanvasGroup.alpha = 0f; // CanvasGroup 투명하게 대기

            string targetMent = rankUI[idx].GetRandomMent(IngameData.ChapterIdx); // 출력할 멘트 미리 뽑아두기

            // 3. DOTween 애니메이션 시퀀스 실행
            Managers.Sound.Play("SFX/Results/3RankMove");
            await imgRect.DOAnchorPosX(-222.6f, 1f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject).AsyncWaitForCompletion();

            await UniTask.Delay(TimeSpan.FromSeconds(0.4f), ignoreTimeScale: true, cancellationToken: ct);

            // resultTxt 출력 (DOText를 이용해 0.5초 동안 타자 치듯 출력)
            Managers.Sound.Play("SFX/Results/4Description_V2");
            await resultTxt.DOText(targetMent, 0.5f).SetUpdate(true).SetLink(gameObject).AsyncWaitForCompletion();

            // 아주 약간 대기 (0.3초 -> 주석은 0.3초인데 실제 0.7초로 되어있음. 유지함)
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f), ignoreTimeScale: true, cancellationToken: ct);

            // lateUpCanvasGroup 등장 (0.5초 동안 페이드 인 -> 실제값 0.2초 유지)
            Managers.Sound.Play("SFX/Results/5Info_V2");
            await lateUpCanvasGroup.DOFade(1f, 0.2f).SetUpdate(true).SetLink(gameObject).AsyncWaitForCompletion();

            if (IngameData.ChapterIdx == 7 && !IngameData.boolPracticeMode) // 마지막이라면 엔딩씬으로 이동, 연습모드도 아니어야 함
            {
                smallInfo_Text.text = LocalizationManager.Get("GameClear_Stage7_SmallInfo_text", "[ESC]로 퇴장 시, 제 7장을 다시 완료해야 합니다.");
                smallInfo_Text.DOFade(1f, 0.2f).SetLink(smallInfo_Text.gameObject);

                Managers.Game.blur.gameOverDownText.text = LocalizationManager.Get("GameClear_Stage7_GoEndingInfo_Text", "<화면을 눌러 최종장으로 넘어가시오.>");
            }
            else
            {
                Managers.Game.blur.gameOverDownText.text = LocalizationManager.Get("Default_GameOverDownGuideText", "클릭하면 스테이지 선택창으로 이동합니다.");
            }
            Managers.Game.blur.gameOverDownText.DOFade(1f, 0.2f).SetLink(Managers.Game.blur.gameOverDownText.gameObject);

            // 4. 연출이 모두 끝난 후 데이터 저장 및 업적 체크
            SaveAndCheckAchievements(score, rank);
        }
        catch (OperationCanceledException)
        {
            // 🚨 씬 이동이나 오브젝트 파괴로 인해 Task가 취소되었을 때 에러를 뱉지 않도록 예외 처리
            Debug.Log("RankAnimation이 취소되었습니다. (씬 이동 또는 오브젝트 파괴)");
        }
        finally
        {
            isAnimationPlaying = false;
        }
    }

    private void SaveAndCheckAchievements(float score, Define.Rank rank)
    {
        if (IngameData.boolPracticeMode) return;
        Debug.Log($"{score}과 {IngameData.BestChapterScore}");

        if (score >= IngameData.BestChapterScore)
        {
            BestRecordAchieve();
        }

        if (Managers.Game.blur.isHp10Down_Warning) // 10 이하로 떨어진 적이 잇다면
        {
            Managers.Steam.UnlockAchievement("ACH_LOW_HP_CLEAR");
        }

        IngameData.BestChapterScore = score;
        IngameData.ChapterRank = rank;

        if (rank == Define.Rank.Perfect && IngameData.ChapterIdx != 0)
        {
            Managers.Steam.UnlockAchievement("ACH_RANK_HIGHEST_FIRST");
        }
        if (rank == Define.Rank.Perfect)
        {
            CheckAllPerfectSteamAchievement();
        }
        if (score >= 100)
        {
            CheckAll100SCOREAchievement();
        }

        if (IngameData.ChapterIdx >= 9)
        {
            Managers.Steam.UnlockAchievement("ACH_EVENT_CLEAR");
        }
        Managers.Steam.UnlockAchievement($"ACH_CHAPTER_{IngameData.ChapterIdx}_CLEAR");
    }

    // Steam: 모든 스테이지의 등급이 최상(Perfect)인지 확인
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

    // Steam: 모든 스테이지의 점수가 100점인지 확인
    private void CheckAll100SCOREAchievement()
    {
        for (int i = 0; i < 7; i++)
        {
            if (IngameData.GetBestRankScoreForChapter(i) >= 100) { }
            else
            {
                return;
            }
        }
        Managers.Steam.UnlockAchievement("ACH_SCORE_100_ALL");
    }

    /// <summary>
    /// 최고 기록을 달성했을 때, 신기록 이미지를 띄워주는 코드
    /// </summary>
    public void BestRecordAchieve()
    {
        resultNewRecordImg.SetNativeSize();
        // 🚨 여기도 SetLink 추가
        resultNewRecordImg.DOFade(1f, 0.2f).SetLink(resultNewRecordImg.gameObject);
        ImageAlphaBrightning(resultNewRecordImg).Forget();
    }

    private async UniTask ImageAlphaBrightning(Image targetImg)
    {
        // 방어코드: 타겟 이미지가 없으면 실행 취소
        if (targetImg == null) return;

        // 1. 중요: 이 객체(ResultUI)가 파괴될 때 루프를 멈추기 위한 토큰 획득
        CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

        // 2. 초기 상태 저장 (기준점)
        Vector3 initialLocalPos = targetImg.transform.localPosition;
        Color initialColor = targetImg.color; // 원래 색상(RGB) 유지용

        // 3. 애니메이션 설정값
        float alphaSpeed = 3f; // 깜빡임 속도
        float minAlpha = 0.7f; // 최소 투명도
        float maxAlpha = 1.0f; // 최대 투명도
        float moveSpeed = 2f; // 둥둥 떠다니는 속도
        float moveRangeY = 10f; // 위아래 이동 범위

        try
        {
            // 무한 루프를 돌며 애니메이션 수행
            while (true)
            {
                // [Alpha 애니메이션]
                float t = Mathf.PingPong(Time.unscaledTime * alphaSpeed, 1f);
                float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
                targetImg.color = new Color(initialColor.r, initialColor.g, initialColor.b, currentAlpha);

                // [위치 애니메이션]
                float yOffset = Mathf.Sin(Time.unscaledTime * moveSpeed) * moveRangeY;
                targetImg.transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y + yOffset, initialLocalPos.z);

                // 다음 프레임까지 대기 (cancellationToken을 전달하여 파괴 시 루프 탈출)
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // 정상적인 취소이므로 무시
        }
    }

    public void UI_Setting_UnlockNewEventStageInfo()
    {
        newEventUnlockTxt.alpha = 1; // "새로운 이벤트 스테이지가 해금되었습니다.";
        string key = "GameClear_EventStageInfo_Text"; // TODO: chapterIdx로 받도록 수정할 것.
        newEventUnlockTxt.text = LocalizationManager.Get(key, "멘트를 찾을 수 없습니다.");
    }

    private void RankImpactSoundPlay(Define.Rank rank)
    {
        string impactPath = "";

        // 🚨 보너스 수정: 조건문이 전부 Perfect로 되어 있던 버그 수정!
        if (rank == Define.Rank.Perfect) { impactPath = "4"; } // 100점, 최상
        else if (rank == Define.Rank.Good) { impactPath = "3"; } // 상
        else if (rank == Define.Rank.NormalGood) { impactPath = "2"; } // 중상
        else if (rank == Define.Rank.Normal) { impactPath = "1"; } // 중
        else { impactPath = "0"; } // 하 (Bad)

        Managers.Sound.Play($"SFX/Results/2RankImpact_{impactPath}");
    }

    private void OnDestroy()
    {
        // 필요시 해제 로직 추가 (SetLink와 CancellationToken 덕분에 강제 해제 불필요)
    }
}