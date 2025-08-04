using UnityEngine;
using UnityEngine.EventSystems;
public class MainTitle : BaseScene

{
    public override void Clear()
    {
        throw new System.NotImplementedException();
    }

    protected override void Init()
    {
        base.Init();
    }

    private void Start()
    {
        Init();
        Managers.UI.ShowPopUpUI<PopUpMainScene>();
        
        //Managers.UI.ShowPopUpUI<GameOver>();

    }

    public void IsStart()
    {
        Managers.Scene.LoadScene(Define.Scene.TitleScene);
    }
}
