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
    private float numDuration = 0.5f;

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
            yield return new WaitForSecondsRealtime(0.1f);

            for (int i = 0; i < spriteLists.Count; i++)
            {
                if (i > 0)
                {
                    images[i - 1].gameObject.SetActive(false);
                }

                images[i].gameObject.SetActive(true);

                images[i].transform.localScale = Vector3.one * 2;
                // 애니메이션 시작
                yield return images[i].transform
                    .DOScale(Vector3.one * 0.5f, numDuration)
                    .SetUpdate(true)
                    .OnComplete(() => { images[i].transform.DOScale(Vector3.one * 1.2f, 0.2f); })
                    .WaitForCompletion();

                yield return new WaitForSecondsRealtime(1 - numDuration);
            }
            images[spriteLists.Count-1].gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < spriteLists.Count; i++)
            {
                Color color = images[i].color;
                color.a = 0f;
                images[i].color = color;

                images[i].gameObject.SetActive(true);

                // Fade 애니메이션
                yield return images[i]
                    .DOFade(1f, duration)
                    .SetUpdate(true)
                    .OnComplete(() => { images[i].gameObject.SetActive(false); })
                    .WaitForCompletion();
            }

            // Success/Fail 처리
            if (illustType == GamePlayDefine.IllustType.Success || illustType == GamePlayDefine.IllustType.Fail)
            {
                gotoTitle.SetActive(true);
            }
        }
    }
}