using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Image uiFill;
    public Image BackGround;
    public int Duration;  // Duration in seconds
    private float elapsedTime = 0f;
    private int StageCount = 0;

    private void Start()
    {
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (elapsedTime < Duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / Duration);
            uiFill.fillAmount = progress;

            yield return null;  // Wait until the next frame
        }
        OnEnd();
    }

    private void OnEnd()
    {
        elapsedTime = 0;
        StageCount++;
        switch(StageCount)
        {
            case 1:

                BackGround.color = Color.red;
                uiFill.color = Color.blue;
                StartCoroutine(UpdateTimer());
                break;
            case 2:
                break;
            case 3:
                break;
        }
         
    }
}