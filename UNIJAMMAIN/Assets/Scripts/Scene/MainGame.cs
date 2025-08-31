public class MainGame : BaseScene   // MainGame Ŭ������ BaseScene Ŭ������ ����� ������� ���� ���� �� �ʿ��� �ʱ�ȭ �۾��� �����ϴ� Ŭ����
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
        if (IngameData.ChapterIdx == 0)
        {
            Managers.UI.ShowPopUpUI<Tutorial_PopUp>();
        }
    }
}
