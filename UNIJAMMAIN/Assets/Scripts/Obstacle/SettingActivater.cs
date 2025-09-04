using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingActivater : MonoBehaviour
{
    public Button panel; // 이 변수는 현재 사용되지 않으므로 제거할 수도 있습니다.

    // Start() 함수에서 이벤트 구독을 설정합니다.
    void Start()
    {
        // 중복 구독을 방지하기 위해 이전에 구독된 이벤트가 있다면 해제합니다.
        PauseManager.IsPaused -= ShowPanel;
        // PauseManager의 IsPaused 이벤트에 ShowPanel 함수를 연결합니다.
        PauseManager.IsPaused += ShowPanel;

        // panel 변수가 인스펙터에서 할당되지 않았을 경우를 대비하여 자식에서 버튼을 찾습니다.
        if (panel == null)
        {
            panel = GetComponentInChildren<Button>();
        }

        // 이 UI 오브젝트의 캔버스가 다른 모든 UI보다 가장 앞에 표시되도록 순서를 설정합니다.
        Managers.UI.SetCanvasMost(this.gameObject);
    }

    // 오브젝트가 파괴될 때 메모리 누수를 방지하기 위해 이벤트 구독을 해제합니다.
    void OnDestroy()
    {
        PauseManager.IsPaused -= ShowPanel;
    }

    // PauseManager.IsPaused 이벤트가 호출될 때 실행되는 함수입니다.
    private void ShowPanel(bool isStop)
    {
        // isStop 값이 true이면 게임이 일시정지 상태임을 나타냅니다.
        if (isStop)
        {
            // 게임이 멈췄을 때만 InGameOption_PopUp UI를 엽니다.
            Managers.UI.ShowPopUpUI<InGameOption_PopUp>();
        }
        else
        {
            // 만약 이 팝업 위에 더 올라갈 팝업이 없다면 아래 코드 쓰고, 아니라면 UI_Manager의 PeekUITop 함수 사용해도 좋음
            Managers.UI.ClosePopUpUI();
        }
    }
}