using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // PointerEventData를 사용하기 위해 추가
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InGameOption_PopUp : UI_Popup
{
    // 버튼들을 enum으로 관리하여 가독성과 유지보수성을 높입니다.
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
        // 부모 클래스의 초기화 메서드를 호출합니다.
        base.Init();

        // enum을 기반으로 UI 버튼 컴포넌트를 바인딩합니다.
        Bind<Button>(typeof(Buttons));

        // 각 버튼에 클릭 이벤트를 등록합니다.
        GetButton((int)Buttons.BGM).gameObject.AddUIEvent(BGMButtonClicked);
        GetButton((int)Buttons.SFX).gameObject.AddUIEvent(SFXButtonClicked);
        GetButton((int)Buttons.ReStart).gameObject.AddUIEvent(ReStartButtonClicked);
        GetButton((int)Buttons.Out).gameObject.AddUIEvent(OutButtonClicked);
        GetButton((int)Buttons.Continues).gameObject.AddUIEvent(ContinuesButtonClicked);

        // 팝업 UI가 다른 UI 위에 항상 보이도록 캔버스 순서를 설정합니다.
        Managers.UI.SetCanvasMost(this.gameObject);
    }

    private void Start()
    {
        // Init() 메서드를 호출하여 초기화 로직을 실행합니다.
        // Start() 대신 Init()을 오버라이드하여 명확성을 높이는 구조입니다.
        Init();
    }

    // BGM 버튼 클릭 시 호출
    public void BGMButtonClicked(PointerEventData eventData)
    {
        // BGM On/Off 또는 볼륨 조절 로직을 여기에 구현합니다.
        Debug.Log("BGM 버튼 클릭됨");
    }

    // SFX 버튼 클릭 시 호출
    public void SFXButtonClicked(PointerEventData eventData)
    {
        // SFX On/Off 또는 볼륨 조절 로직을 여기에 구현합니다.
        Debug.Log("SFX 버튼 클릭됨");
    }

    // ReStart 버튼 클릭 시 호출
    public void ReStartButtonClicked(PointerEventData eventData)
    {
        // 팝업을 닫고, 게임 시간을 재개한 후, 현재 씬을 다시 로드합니다.
        Managers.UI.ClosePopUpUI();
        PauseManager.ControlTime(false);
        Managers.Sound.StopBGM();
        Managers.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Out (게임 종료/메인 화면) 버튼 클릭 시 호출
    public void OutButtonClicked(PointerEventData eventData)
    {
        // 팝업을 닫고, 게임 시간을 재개한 후, 'StageScene'으로 이동합니다.
        Managers.UI.ClosePopUpUI();
        PauseManager.ControlTime(false);
        Managers.Sound.StopBGM();
        Managers.Clear();
        SceneManager.LoadScene("StageScene");
    }

    // Continues (계속하기) 버튼 클릭 시 호출
    public void ContinuesButtonClicked(PointerEventData eventData)
    {
        // PauseManager를 호출하여 게임 시간을 재개합니다.
        PauseManager.ControlTime(false);
        // 팝업 UI를 닫습니다.
        Managers.UI.ClosePopUpUI();
    }
}