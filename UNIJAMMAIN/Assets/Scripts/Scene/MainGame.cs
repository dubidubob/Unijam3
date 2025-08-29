public class MainGame : BaseScene   // MainGame Ŭ������ BaseScene Ŭ������ ����� ������� ���� ���� �� �ʿ��� �ʱ�ȭ �۾��� �����ϴ� Ŭ����
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

        // Managers.UI.ShowPopUpUI<S1_PopUp>();
        Managers.Sound.Play("BGM/84bpm_64_V1", Define.Sound.BGM);
        
        Managers.Game.GameStart();
    }
}
