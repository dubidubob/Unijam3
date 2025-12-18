using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
struct RankUI {
    public float cutline;
    public Sprite img;
    public string ment1;
    public string ment2;

    public string GetRandomMent()
    {
        // 0 또는 1 중 랜덤으로 뽑기
        int rand = UnityEngine.Random.Range(0, 2);
        return (rand == 0) ? ment1 : ment2;
    }
}
public class ResultUI : MonoBehaviour
{
    [Header("UI 자리")]
    [SerializeField] private Image resultImg;
    [SerializeField] private TMP_Text resultTxt;
    [SerializeField] private TMP_Text resultScore;
    [Header("UI 정보 : 최상, 상, 중상, 중, 하 순으로")]
    [SerializeField] List<RankUI> rankUI;

    private void Awake()
    {
        if (rankUI.Count < 5)
            Debug.LogWarning("rank ui 정보 채워넣으셈");
    }

    public void OnClick()
    {
        Time.timeScale = 1f;                 // 혹시 일시정지 풀기
        SceneLoadingManager.Instance.LoadScene("StageScene");
    }

    public void ChangeUI(float score)
    {
        // TODO : 이거 UI 따리에 있으면 안 됨...
        resultScore.text = score.ToString("0.##")+"점";
        
        int idx;
        Define.Rank rank;

        if (score >= rankUI[0].cutline) { idx = 0; rank = Define.Rank.Perfect; }  // 최상
        else if (score >= rankUI[1].cutline) { idx = 1; rank = Define.Rank.Good; } // 상
        else if (score >= rankUI[2].cutline) { idx = 2; rank = Define.Rank.NormalGood; } // 중상
        else if (score >= rankUI[3].cutline) { idx = 3; rank = Define.Rank.Normal; } // 중
        else { idx = 4; rank = Define.Rank.Bad; }// 하

        if (!IngameData.boolPracticeMode) //연습모드가 아니라면 저장
        {
            IngameData.ChapterRank = rank;
        }
        resultImg.sprite = rankUI[idx].img;
        resultImg.SetNativeSize();
        resultTxt.text = rankUI[idx].GetRandomMent();
    }
}