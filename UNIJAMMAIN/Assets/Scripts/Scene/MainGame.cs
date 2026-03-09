using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections; // 코루틴 사용을 위해 추가!

public class MainGame : BaseScene
{
    public bool isPopUp = false;
    private InGameOption_PopUp optionPopUp;
    [SerializeField] private GameObject escPanel;

    // ▼▼▼ 1. Start() 메서드 추가 ▼▼▼
    private void Start()
    {
        // 씬의 모든 준비가 끝났다고 LoadingManager에게 알립니다.
        StartCoroutine(NotifyManagerWhenReady());

    }

    // ▼▼▼ 2. "준비 완료" 신호를 보내는 코루틴 추가 ▼▼▼
    private IEnumerator NotifyManagerWhenReady()
    {
        // 씬의 모든 Start 함수가 실행되고 첫 프레임을 그릴 시간을 안전하게 확보합니다.
        yield return null;

        // SceneLoadingManager에게 "이제 문 열어도 돼!" 라고 신호를 보냅니다.
        if (SceneLoadingManager.Instance != null)
        {
            SceneLoadingManager.Instance.NotifySceneReady();
        }
    }




    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();
    }

    private void Awake()
    {
        PauseManager.ControlTime(false);
        Init();
        Managers.Init();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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