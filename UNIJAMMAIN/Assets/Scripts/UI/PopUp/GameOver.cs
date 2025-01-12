using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
public class GameOver : UI_Popup
{
    public Image blackPanel,goodSleepMan,WhyAmI;
    public TMP_Text text;
    private float duration = 4f;  // 암전까지 걸리는 시간
    enum Buttons
    {
        Why_Am_I_Here
    }
   
    private void Start()
    {
        Init();
        StartCoroutine(GoingBlack());
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.Why_Am_I_Here).gameObject.AddUIEvent(gohome);
        
    }
    void gohome(PointerEventData eventData)
    {
        Managers.Scene.LoadScene(Define.Scene.MainTitle);
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
        elapsedTime = 0f;
        text.DOFade(0f, 4f);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        goodSleepMan.DOFade(1f, 2f);
        Managers.Sound.Play("/Sounds/SFX/Crowd_Noise_SFX");
        elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            yield return null;  // 다음 프레임까지 대기
        }
        WhyAmI.DOFade(1f, 4f);
    }
}

