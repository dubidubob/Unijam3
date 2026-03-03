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
       

    }

    public void IsStart()
    {
        Managers.Sound.Play("SFX/UI/PressToStart_V1", Define.Sound.SFX);
        SceneLoadingManager.Instance.LoadScene("TitleScene");
    }
}
