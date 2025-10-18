using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageScene : BaseScene
{
    public bool Test;

    // ���� ������ Start �޼��� ����
    private void Start()
    {
        Managers.Sound.Play("BGM/MainScene_V2", Define.Sound.BGM);
        // 1. ���� �ʿ��� ��� �ʱ�ȭ�� ���� �����մϴ�.
        Init();

        // 2. UI�� ȭ�鿡 ���ϴ�.
        Managers.UI.ShowPopUpUI<StageSceneUI>();

        // 3. ��� �غ� �����ٰ� LoadingManager���� �˸��ϴ�.
        StartCoroutine(NotifyManagerWhenReady());
    }

    // ���� "�غ� �Ϸ�" ��ȣ�� ������ �ڷ�ƾ �߰� ����
    private IEnumerator NotifyManagerWhenReady()
    {
        // UI�� �����ǰ� ù �������� �׸� �ð��� �����ϰ� Ȯ���ϱ� ���� �� ������ ��ٸ��ϴ�.
        yield return null;

        // SceneLoadingManager���� "���� �� ��� ��!" ��� ��ȣ�� �����ϴ�.
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