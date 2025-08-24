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
    [SerializeField] private Transform spawnPoint;  // �ؽ�Ʈ�� ��Ÿ�� ��ġ
    [SerializeField] private float moveUpDistance = 50f; // ���� �̵� �Ÿ�
    [SerializeField] private float duration = 0.8f;       // �ؽ�Ʈ ���� �ð�

    void Start()
    {
        Managers.Game.accuracy = this;
    }

    public void ShowAccuracy(AccuracyState state)
    {
        // Text ����
        GameObject go = Instantiate(textPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
        TMP_Text tmp = go.GetComponent<TMP_Text>();

        // ���¿� ���� �ؽ�Ʈ ���� ����
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

        // �˾� �ִϸ��̼� ����
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

            // ��ġ ����
            go.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            // ���� ����
            tmp.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        Destroy(go);
    }
}
