using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class PopUpMainScene : UI_Popup
{
    enum Buttons
    {
        GameGuide,
        GameOut
    }
    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        SetResolution();
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.GameGuide).gameObject.AddUIEvent(GameGuideClicked);
        GetButton((int)Buttons.GameOut).gameObject.AddUIEvent(GameOut);
    }
    /*
    void GameStartClicked(PointerEventData eventData)
    {
        Managers.Scene.LoadScene(Define.Scene.MainGame);
    }
    */

    void GameGuideClicked(PointerEventData eventData)
    {
        Managers.UI.ShowPopUpUI<GameGuide>();

    }
    void GameOut(PointerEventData eventData)
    {

    }
}
