using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameGuide : UI_Popup
{
     enum Buttons
    {
        Out
    }

    private void Start()
    {
        Init();
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.Out).gameObject.AddUIEvent(OutClicked);
    }

    void OutClicked(PointerEventData eventData)
    {
        Managers.UI.ClosePopUpUI();
    }


}
