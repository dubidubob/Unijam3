using UnityEngine;
using UnityEngine.SceneManagement;

public class AspectRatioEnforcer : MonoBehaviour
{
    public static AspectRatioEnforcer Instance;

    [Header("Aspect Ratio")]
    public float targetAspectWidth = 16.0f;
    public float targetAspectHeight = 9.0f;
    [Header("Size Limits")]
    public int minWindowWidth = 960;

    private float targetRatio;
    private int minWindowHeight;

    // --- [핵심 수정] ---
    // 이 변수들은 "사용자가 마지막으로 사용한 *창모드* 크기"를 기억합니다.
    private int _lastWindowWidth;
    private int _lastWindowHeight;

    // 이 변수는 "사용자가 *현재* 원하는 상태(전체화면 or 창모드)"를 기억합니다.
    private bool _userWantsFullscreen = false;
    // --- [핵심 수정 끝] ---


    void Awake()
    {
        // 1. 싱글톤 (복제본 B가 실행) - 해결책 2 적용
        if (Instance != null)
        {
            this.enabled = false;
            Destroy(this); // 스크립트 컴포넌트만 파괴
            return;
        }

        // 2. 원본(A)만 실행
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        // 3. 초기 계산
        targetRatio = targetAspectWidth / targetAspectHeight;
        minWindowHeight = (int)Mathf.Round((float)minWindowWidth / targetRatio);

        // 4. [수정] 게임 시작 시 현재 상태를 저장합니다.
        _userWantsFullscreen = Screen.fullScreen;
        if (_userWantsFullscreen)
        {
            // 전체화면으로 시작했다면, 창모드 크기는 최소값으로 기억
            _lastWindowWidth = minWindowWidth;
            _lastWindowHeight = minWindowHeight;
        }
        else
        {
            // 창모드로 시작했다면, 현재 크기를 기억하고 비율을 1회 적용
            _lastWindowWidth = Screen.width;
            _lastWindowHeight = Screen.height;
            EnforceWindowedResolution();
        }
    }

    void Update()
    {
        if (Application.isEditor) return;

        // 1. [핵심] 사용자가 Alt+Enter 등으로 상태를 "변경"했는지 감지
        if (Screen.fullScreen != _userWantsFullscreen)
        {
            _userWantsFullscreen = Screen.fullScreen; // 새 상태를 즉시 저장

            if (!_userWantsFullscreen)
            {
                // 사용자가 "전체화면 -> 창모드"로 *방금* 전환했습니다.
                // 우리가 기억해둔 마지막 창 크기로 복원해줍니다.
                Screen.SetResolution(_lastWindowWidth, _lastWindowHeight, false);
            }
            // (사용자가 "창모드 -> 전체화면"으로 전환한 경우, 
            // 우린 아무것도 할 필요 없이 그냥 두면 됩니다.)

            return; // 상태 변경이 감지된 프레임에는 다른 작업을 하지 않음
        }

        // 2. 사용자가 "전체화면" 상태를 원한다면, 아무것도 하지 않고 놔둡니다.
        if (_userWantsFullscreen)
        {
            return;
        }

        // 3. (여기 도달했다면 _userWantsFullscreen == false)
        // 사용자가 "창모드" 상태일 때, 창 크기가 (수동 조절 등으로) 변경되었는지 감지
        if (Screen.width != _lastWindowWidth || Screen.height != _lastWindowHeight)
        {
            // 창 크기가 바뀌었으므로, 비율을 다시 적용하고 새 크기를 저장합니다.
            EnforceWindowedResolution();
        }
    }

    /// <summary>
    /// [수정] 이 함수는 "현재 창 너비"를 기준으로 비율을 강제하고,
    /// 그 결과를 _lastWindow 변수에 저장합니다.
    /// </summary>
    private void EnforceWindowedResolution()
    {
        if (Screen.fullScreen) return; // 창모드일 때만 실행

        int newWidth = Screen.width;

        // 최소 크기 보정
        if (newWidth < minWindowWidth)
        {
            newWidth = minWindowWidth;
        }

        // 새 너비에 맞는 높이 계산
        int newHeight = (int)Mathf.Round((float)newWidth / targetRatio);

        // 해상도 적용
        Screen.SetResolution(newWidth, newHeight, false);

        // [핵심] 방금 적용한 "새로운" 창 크기를 변수에 저장합니다.
        _lastWindowWidth = newWidth;
        _lastWindowHeight = newHeight;
    }


    /// <summary>
    /// [매우 중요] 씬 로딩 직후 호출될 "해상도 복원" 함수입니다.
    /// 씬 로딩 버그가 해상도를 엉망으로 만들어도, 
    /// 이 함수가 우리가 기억한 상태(_userWantsFullscreen, _lastWindowWidth)로 강제 복구시킵니다.
    /// </summary>
    public void RestoreDisplayState()
    {
        if (_userWantsFullscreen)
        {
            // 사용자는 전체화면 상태였으므로, 전체화면을 강제로 다시 적용
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        else
        {
            // 사용자는 창모드 상태였으므로, 마지막 창 크기를 강제로 다시 적용
            Screen.SetResolution(_lastWindowWidth, _lastWindowHeight, false);
        }
    }

    // OnDestroy는 비워둡니다 (싱글톤 원본은 파괴되지 않음)
    void OnDestroy() { }
}