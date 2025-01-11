using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IllustController : MonoBehaviour
{
    [SerializeField] IllustPrefabSO illustPrefabSO;
    [SerializeField] List<Image> images;
    [SerializeField] GameObject gotoTitle;
    private float duration = 2f;
    private float numDuration = 0.9f;

    private void Start()
    {
        foreach (var go in images)
        { 
            go.gameObject.SetActive(false);
        }


    }

    //private IEnumerator 
    public IEnumerator ShowIllust(GamePlayDefine.IllustType illustType)
    {
        var go = illustPrefabSO.GetIllust(illustType);
        List<Sprite> spriteLists = go.illustLists;
        Debug.Log($"sprite 개수 : {spriteLists.Count}");
        // 이미지 초기화
        for (int i = 0; i < spriteLists.Count; i++)
        {
            if (images[i] != null && spriteLists[i] != null)
            {
                images[i].gameObject.SetActive(false);
                images[i].sprite = spriteLists[i];
                images[i].SetNativeSize();
            }
            else
            {
                Debug.LogWarning($"Null reference detected at index {i}: Image or Sprite is null.");
            }
        }

        // 애니메이션 처리
        if (illustType == GamePlayDefine.IllustType.Num)
        {
            for (int i = 0; i < spriteLists.Count; i++)
            {
                if (i > 0)
                {
                    images[i - 1].gameObject.SetActive(false);
                }

                images[i].gameObject.SetActive(true);

                // 애니메이션 시작
                yield return images[i].transform
                    .DOScale(Vector3.one * 1.5f, numDuration)
                    .SetUpdate(true)
                    .WaitForCompletion();
            }
        }
        else
        {
            for (int i = 0; i < spriteLists.Count; i++)
            {
                images[i].gameObject.SetActive(true);

                // Fade 애니메이션
                yield return images[i]
                    .DOFade(1f, duration)
                    .SetUpdate(true)
                    .WaitForCompletion();

                images[i].gameObject.SetActive(false);
            }

            // Success/Fail 처리
            if (illustType == GamePlayDefine.IllustType.Success || illustType == GamePlayDefine.IllustType.Fail)
            {
                gotoTitle.SetActive(true);
            }
        }
    }
}
