using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageScene : BaseScene
{
    private void Start()
    {
        Managers.Sound.Play("BGM/StageSelect", Define.Sound.BGM);
        Managers.UI.ShowPopUpUI<StageSceneUI>();
        Init();
    }
    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();
        Managers.Game.Init();
    }
}
