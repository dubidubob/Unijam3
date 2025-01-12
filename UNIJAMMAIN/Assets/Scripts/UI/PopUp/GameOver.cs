using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameOver : UI_Popup
{
    public Image blackPanel,goodSleepMan,WhyAmI;
    public TMP_Text text;
    private float duration = 3f;  // 암전까지 걸리는 시간
    enum Buttons
    {
        Why_Am_I_Here
    }
   
    private void Start()
    {
        Init();
        StartCoroutine(GoingBlack());
        //Bind<Button>(typeof(Buttons));
        //GetButton((int)Buttons.Why_Am_I_Here).gameObject.AddUIEvent(gohome);
        
    }
    void gohome()
    {
        Managers.Scene.LoadScene(Define.Scene.MainTitle);
    }
    public override void Init()
    {
        base.Init();
    }

    private IEnumerator GoingBlack()
    {
        yield return new WaitForSecondsRealtime(duration);
        goodSleepMan.DOFade(1f, 2f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(2f);
        WhyAmI.DOFade(1f, 4f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(2f);
        Managers.Scene.LoadScene(Define.Scene.MainTitle);
    }
}

