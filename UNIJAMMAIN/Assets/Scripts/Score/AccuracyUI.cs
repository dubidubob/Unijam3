using TMPro;
using UnityEngine;
using static GamePlayDefine;
public class AccuracyUI : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab; // TextMeshProPrefab
    private TMP_Text txt;

    private void Start()
    {
        txt = textPrefab.GetComponent<TMP_Text>();

        IngameData.OnRankUpdate -= UpdateAccuracyUI;
        IngameData.OnRankUpdate += UpdateAccuracyUI;
    }

    private void OnDestroy()
    {
        IngameData.OnRankUpdate -= UpdateAccuracyUI;
    }

    private void UpdateAccuracyUI(RankType rank)
    {
        switch (rank) 
        {
            case RankType.Miss:
                txt.text = "MISS!";
                txt.color = Color.red;
                break;
            case RankType.Good:
                txt.text = "Good!";
                txt.color = Color.white;
                SetGoodSound();
                break;
            case RankType.Perfect:
                txt.text = "Perfect!";
                txt.color = Color.yellow;
                SetPerfectSound();
                break;
        }
    }

    #region Sound

    private void SetPerfectSound()
    {
        // Generates a random integer from 0 (inclusive) to 4 (exclusive), so the result is 0, 1, 2, or 3.
        int random = Random.Range(0, 4);

        Debug.Log("소리가 들리는가?");
        switch (random)
        {
            case 0:
                Managers.Sound.Play("SFX/Accuracy/Perfect1_V1", Define.Sound.SFX);
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
