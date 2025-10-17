using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : UI_Base
{
    // 싱글톤 인스턴스
    public static SceneLoadingManager Instance { get; private set; }

    // 로딩 중에 ESC 누르면 씬이 엉키는 문제 해결하기 위한 선언
    public static bool IsLoading { get; private set; } = false;

    // 1. 씬 준비 완료 신호를 기다리기 위한 변수 추가 ▼▼▼
    private bool isSceneReadyToDisplay = false;

    [SerializeField] private Image leftPanel;
    [SerializeField] private Image rightPanel;

    [Header("Animation Settings")]
    [Tooltip("문이 열려있을 때의 X 좌표 (예: 1490)")]
    [SerializeField] private float openXPosition = 1490f;

    [Tooltip("문이 닫혔을 때의 X 좌표 (예: 480)")]
    [SerializeField] private float closedXPosition = 480f;

    [Tooltip("문이 움직이는 데 걸리는 시간")]
    [SerializeField] private float animationDuration = 0.8f;

    [Tooltip("문이 닫힌 상태에서 대기하는 시간")]
    [SerializeField] private float waitDuration = 0.5f;

    private AnimationCurve closingCurve;
    private AnimationCurve openingCurve;

    private RectTransform leftRect;
    private RectTransform rightRect;



    private float panelWidth;

    public override void Init()
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        // 싱글톤 패턴 구현
      
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 전환되어도 이 오브젝트를 파괴하지 않음
            InitializePanels();
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있다면 이 오브젝트는 파괴
        }
   
    }

    public void LoadScene(string sceneName)
    {
        // 만약 이미 로딩 중이라면, 또 LoadScene을 실행하지 않고 즉시 종료합니다.
        if (IsLoading) return;

        // 코루틴을 시작하여 비동기 씬 로딩을 실행
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    // 씬 로딩 시퀀스를 관리하는 메인 코루틴
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 로딩 시작 시 즉시 true로 설정
        IsLoading = true;


        // ▼▼▼ 2. 코루틴 시작 시 준비 상태를 false로 초기화 ▼▼▼
        isSceneReadyToDisplay = false;


        // 1. 문 닫기 애니메이션
        Managers.Sound.Play("SFX/UI/StorySelect_V1", Define.Sound.SFX);
        leftPanel.gameObject.SetActive(true);
        rightPanel.gameObject.SetActive(true);
        yield return AnimatePanels(true); // true = 닫기


        Managers.Clear();
        // 2. 닫힌 상태에서 잠시 대기
        yield return new WaitForSecondsRealtime(waitDuration);

        // 3. 씬 비동기 로드 시작 (문이 닫힌 상태에서 진행)
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        // 씬 로딩이 90% 완료될 때까지 대기
        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }

        // 4. 씬 활성화 (아직 문은 닫혀있음)
        // 이 시점에서 다음 씬의 Awake(), OnEnable() 등이 호출됨
        asyncOperation.allowSceneActivation = true;

        // 씬이 완전히 활성화되고 첫 프레임을 그릴 시간을 주기 위해 한 프레임 대기
        yield return null;

        // ▼▼▼ 3. 여기서 바로 문을 열지 않고, 신호를 받을 때까지 무한 대기 ▼▼▼
        while (!isSceneReadyToDisplay)
        {
            yield return null;
        }

        Managers.Sound.Play("SFX/UI/Dialogue/Dialogue_V1", Define.Sound.SFX);

        // 5. 새로운 씬이 준비되면 문 열기 애니메이션 시작
        yield return AnimatePanels(false); // false = 열기

        // 잠시 후 패널 비활성화 (선택 사항)
        yield return new WaitForSecondsRealtime(0.1f);
        leftPanel.gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);
        Managers.Sound.SettingNewSceneVolume();

        // 모든 로딩 과정이 완전히 끝나면 false로 설정
        IsLoading = false;
        StopAllCoroutines();

    }

    public void NotifySceneReady()
    {
        isSceneReadyToDisplay = true;
    }

    // 실제 패널을 움직이는 애니메이션 코루틴
    private IEnumerator AnimatePanels(bool isClosing)
    {
        float timer = 0f;

        // 시작 위치와 목표 위치 설정 (isClosing 여부에 따라)
        float startX = isClosing ? openXPosition : closedXPosition;
        float endX = isClosing ? closedXPosition : openXPosition;

        AnimationCurve curve = isClosing ? closingCurve : openingCurve;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / animationDuration);

            // AnimationCurve를 적용하여 시간 진행률을 비선형적으로 변경 (튕기는 효과 구현)
            float curveProgress = curve.Evaluate(progress);

            // LerpUnclamped를 사용해야 커브 값이 1을 넘어도 정상적으로 동작
            float currentX = Mathf.LerpUnclamped(startX, endX, curveProgress);

            // 왼쪽 패널은 음수, 오른쪽 패널은 양수 좌표로 대칭 이동
            leftRect.anchoredPosition = new Vector2(-currentX, leftRect.anchoredPosition.y);
            rightRect.anchoredPosition = new Vector2(currentX, rightRect.anchoredPosition.y);

            yield return null;
        }

        // 애니메이션이 끝난 후 정확한 최종 위치로 설정
        leftRect.anchoredPosition = new Vector2(-endX, leftRect.anchoredPosition.y);
        rightRect.anchoredPosition = new Vector2(endX, rightRect.anchoredPosition.y);
    }


    // 패널 초기화 및 시작 위치 설정
    private void InitializePanels()
    {
        leftRect = leftPanel.GetComponent<RectTransform>();
        rightRect = rightPanel.GetComponent<RectTransform>();

        // 시작 시 문이 열린 상태로 강제 설정
        closingCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.0f),      // 0% 지점에서 시작 (값: 0)
            new Keyframe(0.7f, 0.9f),     // 
            new Keyframe(0.8f, 0.95f),     //
            new Keyframe(0.9f, 0.975f),     //
            new Keyframe(1f, 1f)    //
           
        );
        openingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        leftRect.anchoredPosition = new Vector2(-openXPosition, leftRect.anchoredPosition.y);
        rightRect.anchoredPosition = new Vector2(openXPosition, rightRect.anchoredPosition.y);
    }

}