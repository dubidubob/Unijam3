using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // PointerEventData�� ����ϱ� ���� �߰�
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InGameOption_PopUp : UI_Popup
{
    // ��ư���� enum���� �����Ͽ� �������� ������������ ���Դϴ�.
    enum Buttons
    {
        BGMUp,
        SFXUp,
        ReStart,
        Out,
        Continues
    }
    private MainGame main;

    public override void Init()
    {
        // �θ� Ŭ������ �ʱ�ȭ �޼��带 ȣ���մϴ�.
        base.Init();

        // enum�� ������� UI ��ư ������Ʈ�� ���ε��մϴ�.
        Bind<Button>(typeof(Buttons));

        // �� ��ư�� Ŭ�� �̺�Ʈ�� ����մϴ�.
        GetButton((int)Buttons.BGMUp).gameObject.AddUIEvent(BGMButtonClicked);
        GetButton((int)Buttons.SFXUp).gameObject.AddUIEvent(SFXButtonClicked);
        GetButton((int)Buttons.ReStart).gameObject.AddUIEvent(ReStartButtonClicked);
        GetButton((int)Buttons.Out).gameObject.AddUIEvent(OutButtonClicked);
        GetButton((int)Buttons.Continues).gameObject.AddUIEvent(ContinuesButtonClicked);

        // �˾� UI�� �ٸ� UI ���� �׻� ���̵��� ĵ���� ������ �����մϴ�.
        Managers.UI.SetCanvasMost(this.gameObject,32767);
        PauseManager.ControlTime(true);
    }

    private void Start()
    {
        Init();
    }

    // BGM ��ư Ŭ�� �� ȣ��
    public void BGMButtonClicked(PointerEventData eventData)
    {
        // BGM On/Off �Ǵ� ���� ���� ������ ���⿡ �����մϴ�.
        Debug.Log("BGM ��ư Ŭ����");
    }

    // SFX ��ư Ŭ�� �� ȣ��
    public void SFXButtonClicked(PointerEventData eventData)
    {
        // SFX On/Off �Ǵ� ���� ���� ������ ���⿡ �����մϴ�.
        Debug.Log("SFX ��ư Ŭ����");
    }

    // ReStart ��ư Ŭ�� �� ȣ��
    public void ReStartButtonClicked(PointerEventData eventData)
    {
        // �˾��� �ݰ�, ���� �ð��� �簳�� ��, ���� ���� �ٽ� �ε��մϴ�.
        Time.timeScale = 1;
        main.isPopUp = false;
        
        PauseManager.ControlTime(false);
        Managers.Sound.StopBGM();
        Managers.Clear();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ClosePopUPUI();
    }

    // Out (���� ����/���� ȭ��) ��ư Ŭ�� �� ȣ��
    public void OutButtonClicked(PointerEventData eventData)
    {
        // �˾��� �ݰ�, ���� �ð��� �簳�� ��, 'StageScene'���� �̵��մϴ�.
        main.isPopUp = false;
       
        PauseManager.ControlTime(false);
        Managers.Sound.StopBGM();
        Managers.Clear();
        SceneLoadingManager.Instance.LoadScene("StageScene");
        ClosePopUPUI();
    }

    // Continues (����ϱ�) ��ư Ŭ�� �� ȣ��
    public void ContinuesButtonClicked(PointerEventData eventData=null)
    {
        // PauseManager�� ȣ���Ͽ� ���� �ð��� �簳�մϴ�.
       
        main.isPopUp = false;
        // �˾� UI�� �ݽ��ϴ�.
        ClosePopUPUI();
        PauseManager.ControlTime(false);

    }
    public void GetMainUI(MainGame mainGame)
    {
        main = mainGame;
    }
}