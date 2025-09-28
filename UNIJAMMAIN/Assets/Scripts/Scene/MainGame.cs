using UnityEngine;
using Unity;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class MainGame : BaseScene   // MainGame Ŭ������ BaseScene Ŭ������ ����� ������� ���� ���� �� �ʿ��� �ʱ�ȭ �۾��� �����ϴ� Ŭ����
{
    public bool isPopUp = false;
    private InGameOption_PopUp optionPopUp;
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPopUp)
            {
                optionPopUp.ContinuesButtonClicked();
                isPopUp = false;
            }
            else
            {
                optionPopUp = Managers.UI.ShowPopUpUI<InGameOption_PopUp>();
                optionPopUp.GetMainUI(this);
                isPopUp = true;
            }
        }
    }
}
