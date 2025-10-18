using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryScene : BaseScene
{
    private void Start()
    {
        Debug.LogWarning("====== StoryScene Start! 스크립트 실행됨. ======");

        // 씬의 모든 준비가 끝났다고 LoadingManager에게 알립니다.
        StartCoroutine(NotifyManagerWhenReady());
    }

    private IEnumerator NotifyManagerWhenReady()
    {
        // 씬의 모든 Start 함수가 실행되고 첫 프레임을 그릴 시간을 안전하게 확보합니다.
        yield return null;

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