using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : BaseScene
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
        LocalizationManager.CurrentLanguage = Language.Korean; // 로컬라이제이션 Language 설정 TODO: 스팀설정에따라 언어설정바꾸기
        LocalizationManager.Load();

    }

}
