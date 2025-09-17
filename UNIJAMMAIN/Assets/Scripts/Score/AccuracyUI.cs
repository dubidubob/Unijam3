using TMPro;
using UnityEngine;
using static GamePlayDefine;
using DG.Tweening;
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
        string hexCode;
        Color newColor;
        switch (rank) 
        {
            case RankType.Miss:
                txt.text = "MISS!";

                hexCode = "#9C8994";

                break;
            case RankType.Good:
                txt.text = "Good!";
 
                hexCode = "#AFE26E";

                SetGoodSound();
                break;
            case RankType.Perfect:
                txt.text = "Perfect!";
                hexCode = "#FFD98C";
                SetPerfectSound();
                break;
            default:
                hexCode = "#FFFFFF"; // 기본값으로 흰색을 할당
                Debug.LogWarning("정의되지 않은 RankType이 들어왔습니다: " + rank);
                break;
        }

        // ColorUtility를 사용하여 문자열을 Color 타입으로 변환합니다.

        if (ColorUtility.TryParseHtmlString(hexCode, out newColor))
        {
            // 변환에 성공했다면 이미지의 색상으로 적용합니다.
            txt.color = newColor;
        }
        else
        {
            Debug.LogError("Invalid hex color code: " + hexCode);
        }

        // rank 강조 효과 추가
        txt.transform
           .DOScale(Vector3.one * (1.2f), 0.2f)
           .OnComplete(() =>
               txt.transform.DOScale(Vector3.one, 0.1f)
           );
    }

    #region Sound

    private void SetPerfectSound()
    {
        // Generates a random integer from 0 (inclusive) to 4 (exclusive), so the result is 0, 1, 2, or 3.
        int random = Random.Range(0, 4);

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
