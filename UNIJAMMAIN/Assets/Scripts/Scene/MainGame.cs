using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections; // 코루틴 사용을 위해 추가!

public class MainGame : BaseScene
{
    public bool isPopUp = false;
    private InGameOption_PopUp optionPopUp;

    // ▼▼▼ 1. Start() 메서드 추가 ▼▼▼
    private void Start()
    {
        // 씬의 모든 준비가 끝났다고 LoadingManager에게 알립니다.
        StartCoroutine(NotifyManagerWhenReady());

        // GameManager의 체력 업데이트 이벤트에 LogCurrentHealth 함수를 등록합니다.
        if (Managers.Game != null)
        {
            Managers.Game.HealthUpdate += LogCurrentHealth;
        }
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

    // ▼▼▼ 3. 현재 체력을 콘솔에 출력하는 함수 추가 ▼▼▼
    private void LogCurrentHealth(float currentHealth)
    {
        // 체력이 변경될 때마다 "현재 체력: [값]" 형식으로 로그를 출력합니다.
        //Debug.Log($"현재 체력: {currentHealth}");
    }

    // ▼▼▼ 4. OnDestroy() 메서드 추가 (메모리 누수 방지) ▼▼▼
    private void OnDestroy()
    {
        // 씬이 파괴될 때 등록했던 체력 업데이트 이벤트를 반드시 해제해야 합니다.
        if (Managers.Game != null)
        {
            Managers.Game.HealthUpdate -= LogCurrentHealth;
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