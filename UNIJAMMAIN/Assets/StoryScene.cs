using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask 필수 네임스페이스

public class StoryScene : BaseScene
{
    private void Start()
    {
        Debug.LogWarning("====== StoryScene Start! 스크립트 실행됨. ======");

        // 1. 씬 로딩 완료 알림 (기존 로직)
        NotifyManagerWhenReady().Forget();

        // 2. 3분 대기 업적 체크 시작 (새로운 로직)
        CheckStayAchievement().Forget();
    }

    // 기존 코루틴 대체: async UniTaskVoid
    private async UniTaskVoid NotifyManagerWhenReady()
    {
        // yield return null과 동일 (첫 프레임 대기)
        await UniTask.Yield(PlayerLoopTiming.Update);

        // SceneLoadingManager에게 "이제 문 열어도 돼!" 라고 신호를 보냅니다.
        if (SceneLoadingManager.Instance != null)
        {
            Debug.Log("SceneLoadingManager 발견! 준비 완료 신호를 보냅니다.");
            SceneLoadingManager.Instance.NotifySceneReady();
        }
        else
        {
            Debug.LogError("치명적 오류: SceneLoadingManager.Instance가 없습니다! 문을 열 수 없습니다.");
        }
    }

    // [신규] 3분 대기 업적 로직
    private async UniTaskVoid CheckStayAchievement()
    {
        // 이 씬(GameObject)이 파괴되면(다른 씬으로 이동하면) 자동으로 타이머를 취소하기 위한 토큰
        var cancellationToken = this.GetCancellationTokenOnDestroy();

        try
        {
            // 3분(180초) 동안 대기합니다.
            // 만약 3분이 되기 전에 씬을 이동하면 cancellationToken에 의해 이 작업은 즉시 '취소(Cancel)'됩니다.
            await UniTask.Delay(TimeSpan.FromMinutes(3), cancellationToken: cancellationToken);

            // --- 3분이 지났을 때 실행될 코드 ---
            Debug.Log("3분 경과! 대기 업적 달성 시도.");

            // 본인이 설정한 업적 ID를 넣으세요 (예: "ACH_STORY_WAIT_3MIN")
            Managers.Steam.UnlockAchievement("ACH_IDLE_MUSIM");
        }
        catch (OperationCanceledException)
        {
            // 3분이 되기 전에 플레이어가 씬을 나갔습니다.
            // 아무것도 하지 않고 조용히 종료됩니다.
            // Debug.Log("3분 되기 전에 씬 이동함. 타이머 취소.");
        }
    }

    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();
    }

    private void Awake()
    {
        Init();
        Managers.Init();
    }
}