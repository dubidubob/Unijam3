using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryScene : BaseScene
{
    public override void Clear()
    {
    }

    protected override void Init()
    {

        base.Init();

    }

    private void Awake()
    {
        Init();

        Managers.Init();
    }
}
