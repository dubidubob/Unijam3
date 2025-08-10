public class MainGame : BaseScene   // MainGame Ŭ������ BaseScene Ŭ������ ����� ������� ���� ���� �� �ʿ��� �ʱ�ȭ �۾��� �����ϴ� Ŭ����
{


    public override void Clear()
    {
        throw new System.NotImplementedException();
    }

    protected override void Init()
    {
       
        base.Init();
   
    }

    private void Awake()
    {
        Init();

        // Managers.UI.ShowPopUpUI<S1_PopUp>();
        Managers.Sound.Play("BGM/84bpm_3min_64", Define.Sound.BGM);
        // Managers.Sound.Play("Sounds/BGM/Test",Define.Sound.BGM);
        Managers.Game.GameStart();
    }
    public void Option()
    {

    }
}
