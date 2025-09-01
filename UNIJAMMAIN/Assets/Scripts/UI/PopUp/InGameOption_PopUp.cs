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
        BGM,
        SFX,
        ReStart,
        Out,
        Continues
    }

    public override void Init()
    {
        // �θ� Ŭ������ �ʱ�ȭ �޼��带 ȣ���մϴ�.
        base.Init();

        // enum�� ������� UI ��ư ������Ʈ�� ���ε��մϴ�.
        Bind<Button>(typeof(Buttons));

        // �� ��ư�� Ŭ�� �̺�Ʈ�� ����մϴ�.
        GetButton((int)Buttons.BGM).gameObject.AddUIEvent(BGMButtonClicked);
        GetButton((int)Buttons.SFX).gameObject.AddUIEvent(SFXButtonClicked);
        GetButton((int)Buttons.ReStart).gameObject.AddUIEvent(ReStartButtonClicked);
        GetButton((int)Buttons.Out).gameObject.AddUIEvent(OutButtonClicked);
        GetButton((int)Buttons.Continues).gameObject.AddUIEvent(ContinuesButtonClicked);

        // �˾� UI�� �ٸ� UI ���� �׻� ���̵��� ĵ���� ������ �����մϴ�.
        Managers.UI.SetCanvasMost(this.gameObject);
    }

    private void Start()
    {
        // Init() �޼��带 ȣ���Ͽ� �ʱ�ȭ ������ �����մϴ�.
        // Start() ��� Init()�� �������̵��Ͽ� ��Ȯ���� ���̴� �����Դϴ�.
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
        Managers.UI.ClosePopUpUI();
        PauseManager.ControlTime(false);
        Managers.Sound.StopBGM();
        Managers.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Out (���� ����/���� ȭ��) ��ư Ŭ�� �� ȣ��
    public void OutButtonClicked(PointerEventData eventData)
    {
        // �˾��� �ݰ�, ���� �ð��� �簳�� ��, 'StageScene'���� �̵��մϴ�.
        Managers.UI.ClosePopUpUI();
        PauseManager.ControlTime(false);
        Managers.Sound.StopBGM();
        Managers.Clear();
        SceneManager.LoadScene("StageScene");
    }

    // Continues (����ϱ�) ��ư Ŭ�� �� ȣ��
    public void ContinuesButtonClicked(PointerEventData eventData)
    {
        // PauseManager�� ȣ���Ͽ� ���� �ð��� �簳�մϴ�.
        PauseManager.ControlTime(false);
        // �˾� UI�� �ݽ��ϴ�.
        Managers.UI.ClosePopUpUI();
    }
}