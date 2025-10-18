using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageScene : BaseScene
{
    public bool Test;

    // ▼▼▼ 수정된 Start 메서드 ▼▼▼
    private void Start()
    {
        Managers.Sound.Play("BGM/MainScene_V2", Define.Sound.BGM);
        // 1. 씬에 필요한 모든 초기화를 먼저 실행합니다.
        Init();

        // 2. UI를 화면에 띄웁니다.
        Managers.UI.ShowPopUpUI<StageSceneUI>();

        // 3. 모든 준비가 끝났다고 LoadingManager에게 알립니다.
        StartCoroutine(NotifyManagerWhenReady());
    }

    // ▼▼▼ "준비 완료" 신호를 보내는 코루틴 추가 ▼▼▼
    private IEnumerator NotifyManagerWhenReady()
    {
        // UI가 생성되고 첫 프레임을 그릴 시간을 안전하게 확보하기 위해 한 프레임 기다립니다.
        yield return null;

        // SceneLoadingManager에게 "이제 문 열어도 돼!" 라고 신호를 보냅니다.
        if (SceneLoadingManager.Instance != null)
        {
            SceneLoadingManager.Instance.NotifySceneReady();
        }
    }

    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();
        if (Test) { Managers.Game.GameStage = 8; }
        Managers.Game.Init();
    }
}