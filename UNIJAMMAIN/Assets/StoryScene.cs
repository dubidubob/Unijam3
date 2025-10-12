using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryScene : BaseScene
{
    private void Start()
    {
        Debug.LogWarning("====== StoryScene Start! ��ũ��Ʈ �����. ======");

        // ���� ��� �غ� �����ٰ� LoadingManager���� �˸��ϴ�.
        StartCoroutine(NotifyManagerWhenReady());
    }

    private IEnumerator NotifyManagerWhenReady()
    {
        // ���� ��� Start �Լ��� ����ǰ� ù �������� �׸� �ð��� �����ϰ� Ȯ���մϴ�.
        yield return null;

        // SceneLoadingManager���� "���� �� ��� ��!" ��� ��ȣ�� �����ϴ�.
        if (SceneLoadingManager.Instance != null)
        {
            Debug.Log("SceneLoadingManager �߰�! �غ� �Ϸ� ��ȣ�� �����ϴ�.");
            SceneLoadingManager.Instance.NotifySceneReady();
        }
        else
        {
            Debug.LogError("ġ���� ����: SceneLoadingManager.Instance�� �����ϴ�! ���� �� �� �����ϴ�.");
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