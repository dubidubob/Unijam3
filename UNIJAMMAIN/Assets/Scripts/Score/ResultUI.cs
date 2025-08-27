using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
struct RankUI {
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
        SceneManager.LoadScene("StageScene");
    }

    public void ChangeUI()
    {
        float score = CalculateScore();
        resultScore.text = score.ToString("0.##");

        int idx = score switch
        {
            >= 95f => 0, // 최상
            >= 85f => 1,   // 상
            >= 70f => 2,   // 중상
            >= 50f => 3,   // 중
            _ => 4    // 하
        };
        
        resultImg.sprite = rankUI[idx].img;
        resultImg.SetNativeSize();
        resultTxt.text = rankUI[idx].GetRandomMent();
    }

    private float perfectWeight = 1.0f;
    private float goodWeight = 0.5f;
    private float CalculateScore()
    {
        float perfectCnt = IngameData.PerfectMobCnt;
        float goodCnt = IngameData.GoodMobCnt;
        float rate = (perfectCnt * perfectWeight + goodCnt * goodWeight);

        float totalCnt = IngameData.TotalMobCnt;
        float missedInput = IngameData.WrongInputCnt;
        float total = totalCnt + missedInput;
        
        return (rate / total) * 100f;
    }
}