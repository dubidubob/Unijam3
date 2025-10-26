using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCountUI : MonoBehaviour
{
    [SerializeField] List<Sprite> sprites = new List<Sprite>();
    private SpriteRenderer sp;

    // OnEnable, OnDisable, Show123Start 함수는 모두 삭제합니다.

    public IEnumerator Play123Coroutine()
    {
        sp = GetComponent<SpriteRenderer>();
        // 게임 오브젝트가 비활성화 상태일 수 있으므로 활성화합니다.
        gameObject.SetActive(true);

        // beatInterval을 한 번만 가져옵니다.
        float beatInterval = (float)IngameData.BeatInterval;

        for (int i = 0; i < sprites.Count; i++)
        {
            // 1. 해당 순서의 스프라이트를 보여줍니다.
            sp.sprite = sprites[i];

            // 2. 정확히 한 비트만큼 기다립니다.
            yield return new WaitForSeconds(beatInterval);
        }

        // 3. 카운트다운이 끝나면 스스로를 비활성화합니다.
        gameObject.SetActive(false);

        // (옵션) 게임 상태를 변경하는 로직이 필요하다면 여기에 추가할 수 있습니다.
        if (!IngameData.boolPracticeMode)
        {
            Managers.Game.currentPlayerState = GameManager.PlayerState.Normal;
        }
    }
}