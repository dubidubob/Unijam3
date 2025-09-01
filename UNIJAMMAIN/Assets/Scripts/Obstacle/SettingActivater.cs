using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingActivater : MonoBehaviour
{
    public Button panel; // �� ������ ���� ������ �����Ƿ� ������ ���� �ֽ��ϴ�.

    // Start() �Լ����� �̺�Ʈ ������ �����մϴ�.
    void Start()
    {
        // �ߺ� ������ �����ϱ� ���� ������ ������ �̺�Ʈ�� �ִٸ� �����մϴ�.
        PauseManager.IsPaused -= ShowPanel;
        // PauseManager�� IsPaused �̺�Ʈ�� ShowPanel �Լ��� �����մϴ�.
        PauseManager.IsPaused += ShowPanel;

        // panel ������ �ν����Ϳ��� �Ҵ���� �ʾ��� ��츦 ����Ͽ� �ڽĿ��� ��ư�� ã���ϴ�.
        if (panel == null)
        {
            panel = GetComponentInChildren<Button>();
        }

        // �� UI ������Ʈ�� ĵ������ �ٸ� ��� UI���� ���� �տ� ǥ�õǵ��� ������ �����մϴ�.
        Managers.UI.SetCanvasMost(this.gameObject);
    }

    // ������Ʈ�� �ı��� �� �޸� ������ �����ϱ� ���� �̺�Ʈ ������ �����մϴ�.
    void OnDestroy()
    {
        PauseManager.IsPaused -= ShowPanel;
    }

    // PauseManager.IsPaused �̺�Ʈ�� ȣ��� �� ����Ǵ� �Լ��Դϴ�.
    private void ShowPanel(bool isStop)
    {
        // isStop ���� true�̸� ������ �Ͻ����� �������� ��Ÿ���ϴ�.
        if (isStop)
        {
            // ������ ������ ���� InGameOption_PopUp UI�� ���ϴ�.
            Managers.UI.ShowPopUpUI<InGameOption_PopUp>();
        }
        else
        {
            // ���� �� �˾� ���� �� �ö� �˾��� ���ٸ� �Ʒ� �ڵ� ����, �ƴ϶�� UI_Manager�� PeekUITop �Լ� ����ص� ����
            Managers.UI.ClosePopUpUI();
        }
    }
}