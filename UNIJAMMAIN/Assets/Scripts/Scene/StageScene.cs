using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageScene : BaseScene
{
    public bool Test;
    private void Start()
    {
        Managers.Sound.Play("BGM/MainScene_V2", Define.Sound.BGM); 
        Managers.UI.ShowPopUpUI<StageSceneUI>();
        Init();
    }
    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();
        if(Test) { Managers.Game.GameStage = 8; }
        Managers.Game.Init();
    }
}
