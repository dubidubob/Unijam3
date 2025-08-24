using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Accuracy : MonoBehaviour
{
    public enum AccuracyState
    {
        UnKnown,
        Miss,
        Normal,
        Perfect
    }

    [SerializeField] private GameObject textPrefab; // TextMeshProPrefab
    [SerializeField] private Transform spawnPoint;  // 텍스트가 나타날 위치
    [SerializeField] private float moveUpDistance = 50f; // 위로 이동 거리
    [SerializeField] private float duration = 0.8f;       // 텍스트 지속 시간

    void Start()
    {
        Managers.Game.accuracy = this;
    }

    public void ShowAccuracy(AccuracyState state)
    {
        // Text 생성
        GameObject go = Instantiate(textPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
        TMP_Text tmp = go.GetComponent<TMP_Text>();

        // 상태에 따라 텍스트 내용 변경
        switch (state)
        {
            case AccuracyState.Miss:
                tmp.text = "MISS!";
                tmp.color = Color.red;
                break;
            case AccuracyState.Normal:
                tmp.text = "NORMAL";
                tmp.color = Color.white;
                break;
            case AccuracyState.Perfect:
                tmp.text = "PERFECT!";
                tmp.color = Color.yellow;
                break;
        }

        // 팝업 애니메이션 시작
        StartCoroutine(PopupTextCoroutine(go));
    }

    private IEnumerator PopupTextCoroutine(GameObject go)
    {
        TMP_Text tmp = go.GetComponent<TMP_Text>();
        Vector3 startPos = go.transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * moveUpDistance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 위치 보간
            go.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            // 투명도 보간
            tmp.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        Destroy(go);
    }
}
