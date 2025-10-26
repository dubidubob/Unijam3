using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCountUI : MonoBehaviour
{
    [SerializeField] List<Sprite> sprites = new List<Sprite>();
    private SpriteRenderer sp;

    // OnEnable, OnDisable, Show123Start �Լ��� ��� �����մϴ�.

    public IEnumerator Play123Coroutine()
    {
        sp = GetComponent<SpriteRenderer>();
        // ���� ������Ʈ�� ��Ȱ��ȭ ������ �� �����Ƿ� Ȱ��ȭ�մϴ�.
        gameObject.SetActive(true);

        // beatInterval�� �� ���� �����ɴϴ�.
        float beatInterval = (float)IngameData.BeatInterval;

        for (int i = 0; i < sprites.Count; i++)
        {
            // 1. �ش� ������ ��������Ʈ�� �����ݴϴ�.
            sp.sprite = sprites[i];

            // 2. ��Ȯ�� �� ��Ʈ��ŭ ��ٸ��ϴ�.
            yield return new WaitForSeconds(beatInterval);
        }

        // 3. ī��Ʈ�ٿ��� ������ �����θ� ��Ȱ��ȭ�մϴ�.
        gameObject.SetActive(false);

        // (�ɼ�) ���� ���¸� �����ϴ� ������ �ʿ��ϴٸ� ���⿡ �߰��� �� �ֽ��ϴ�.
        if (!IngameData.boolPracticeMode)
        {
            Managers.Game.currentPlayerState = GameManager.PlayerState.Normal;
        }
    }
}