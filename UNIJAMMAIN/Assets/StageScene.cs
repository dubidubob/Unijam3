using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageScene : BaseScene
{
    private void Start()
    {
        Managers.UI.ShowPopUpUI<StageSceneUI>();
        Init();
    }
    public override void Clear()
    {
        throw new System.NotImplementedException();
    }

    protected override void Init()
    {
        base.Init();
        
    }
}
