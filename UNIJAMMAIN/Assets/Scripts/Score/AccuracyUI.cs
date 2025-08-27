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

        HitJudge.OnRankUpdate -= UpdateAccuracyUI;
        HitJudge.OnRankUpdate += UpdateAccuracyUI;
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
                break;
            case RankType.Perfect:
                txt.text = "Perfect!";
                txt.color = Color.yellow;
                break;
        }
    }
}
