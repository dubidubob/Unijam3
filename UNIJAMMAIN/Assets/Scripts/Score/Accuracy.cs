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
                SetGoodSound();
                break;
            case AccuracyState.Perfect:
                tmp.text = "PERFECT!";
                tmp.color = Color.yellow;
                SetPerfectSound();
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

    #region Sound

    private void SetPerfectSound()
    {
        // Generates a random integer from 0 (inclusive) to 4 (exclusive), so the result is 0, 1, 2, or 3.
        int random = Random.Range(0, 4);

        Debug.Log("�Ҹ��� �鸮�°�?");
        switch (random)
        {
            case 0:
                Managers.Sound.Play("SFX/Accuracy/Perfect1_V1",Define.Sound.SFX);
                break;
            case 1:
                Managers.Sound.Play("SFX/Accuracy/Perfect2_V1", Define.Sound.SFX);
                break;
            case 2:
                Managers.Sound.Play("SFX/Accuracy/Perfect3_V1", Define.Sound.SFX);
                break;
            case 3:
                Managers.Sound.Play("SFX/Accuracy/Perfect4_V1", Define.Sound.SFX);
                break;
        }
    }

    private void SetGoodSound()
    {
        Managers.Sound.Play("SFX/Accuracy/Good_V1", Define.Sound.SFX);
    }
    #endregion
}
