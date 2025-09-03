using UnityEngine;
using UnityEngine.EventSystems;
public class MainTitle : BaseScene

{
    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();
    }

    private void Start()
    {
        Init();
        Managers.UI.ShowPopUpUI<MainScene>();
        Managers.Sound.Play("BGM/MainScreen_V1", Define.Sound.BGM);
        //Managers.UI.ShowPopUpUI<GameOver>();

    }

    public void IsStart()
    {
        Managers.Scene.LoadScene(Define.Scene.TitleScene);
    }
}
