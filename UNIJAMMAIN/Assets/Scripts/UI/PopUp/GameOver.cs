using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOver : UI_Popup
{
    public Image blackPanel;
    public TMP_Text text;
    private float duration = 4f;  // 암전까지 걸리는 시간
    private void Start()
    {
        Init();
        StartCoroutine(GoingBlack());
        
    }
    public override void Init()
    {
        base.Init();
    }

    private IEnumerator GoingBlack()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Debug.Log("몇번실행?");
            // alpha 값이 0에서 1로 점진적으로 변하도록 설정
            blackPanel.color = new Color(0, 0, 0, Mathf.Lerp(0, 1f, elapsedTime / duration));
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        elapsedTime = 0f;
        while (elapsedTime < duration)
            {
                text.color = new Color(1, 1, 1, Mathf.Lerp(0, 1f, elapsedTime / duration));
                elapsedTime += Time.deltaTime;  // 경과 시간 증가
                yield return null;  // 다음 프레임까지 대기
            }
        while (elapsedTime < duration)
        {
            text.color = new Color(255, 255, 255, Mathf.Lerp(1, 0f, elapsedTime / duration));
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }

        // 마지막에 정확히 1로 설정
        text.color = new Color(255, 255, 255, 1);
        blackPanel.color = new Color(0, 0, 0, 1);
    }
}

