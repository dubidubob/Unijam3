using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;       // CancellationToken 사용을 위해 필요
using Cysharp.Threading.Tasks; // UniTask 필수 네임스페이스

public class StageScene : BaseScene
{
    public bool Test;

    // ▼▼▼ 수정된 Start 메서드 ▼▼▼
    private void Start()
    {
        Managers.Sound.Play("BGM/MainTitle_V3", Define.Sound.BGM);

        // 1. 씬에 필요한 모든 초기화를 먼저 실행합니다.
        Init();

        // 2. UI를 화면에 띄웁니다.
        Managers.UI.ShowPopUpUI<StageSceneUI>();

        // 3. 모든 준비가 끝났다고 LoadingManager에게 알립니다. (UniTask 실행)
        NotifyManagerWhenReady().Forget();

        // 4. [신규] 3분 대기 업적 체크 시작 (UniTask 실행)
        CheckStayAchievement().Forget();
    }

    // ▼▼▼ 기존 코루틴 대체: async UniTaskVoid ▼▼▼
    private async UniTaskVoid NotifyManagerWhenReady()
    {
        // UI가 생성되고 첫 프레임을 그릴 시간을 안전하게 확보하기 위해 한 프레임 기다립니다.
        // (yield return null 과 동일)
        await UniTask.Yield(PlayerLoopTiming.Update);

        // SceneLoadingManager에게 "이제 문 열어도 돼!" 라고 신호를 보냅니다.
        if (SceneLoadingManager.Instance != null)
        {
            SceneLoadingManager.Instance.NotifySceneReady();
        }
    }

    // ▼▼▼ [신규] 3분 이상 대기 시 업적 달성 로직 ▼▼▼
    private async UniTaskVoid CheckStayAchievement()
    {
        // 이 씬(GameObject)이 파괴되거나 씬이 넘어가면 작업을 취소하기 위한 토큰
        var cancellationToken = this.GetCancellationTokenOnDestroy();

        try
        {
            // 3분(180초) 동안 대기합니다.
            // 3분이 되기 전에 씬을 이동하면 cancellationToken에 의해 작업이 즉시 중단되어 안전합니다.
            await UniTask.Delay(TimeSpan.FromMinutes(3), cancellationToken: cancellationToken);

            // --- 3분이 지났을 때 실행될 코드 ---
            Debug.Log("[StageScene] 3분 경과! 대기 업적 달성.");

            // TODO: 스팀웍스에 등록한 실제 업적 ID로 변경하세요.
            Managers.Steam.UnlockAchievement("ACH_STAGE_WAIT_3MIN");
        }
        catch (OperationCanceledException)
        {
            // 3분이 되기 전에 씬을 나갔을 경우 실행됨 (아무 작업 안 함)
        }
    }

    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();
        if (Test) { 
            IngameData._nowStageIndex = 0;
            IngameData._clearStageIndex = 7;
        }
        Managers.Game.Init();
    }
}