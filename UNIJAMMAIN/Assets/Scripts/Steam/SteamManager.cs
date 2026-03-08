using System;
using UnityEngine;
using Steamworks;

// 이 스크립트는 Steam API를 초기화하고 유지관리하는 필수 스크립트입니다.
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
    protected static bool s_EverInitialized = false;
    protected static SteamManager s_Instance;
    protected static bool s_Initialized = false;

    // ★ 추가: 스팀 업적 데이터가 성공적으로 로드되었는지 확인하는 플래그
    public static bool IsStatsLoaded { get; private set; } = false;

    // ★ 추가: 비동기 데이터 요청 결과를 받을 콜백 변수
    private CallResult<UserStatsReceived_t> m_UserStatsReceived;

    public static bool Initialized
    {
        get { return s_Initialized; }
    }

    protected virtual void Awake()
    {
        if (s_Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_Instance = this;

        if (s_EverInitialized)
        {
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }

        // 씬이 넘어가도 파괴되지 않게 유지
        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try
        {
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);
            Application.Quit();
            return;
        }

        // 스팀 API 초기화
        s_Initialized = SteamAPI.Init();
        if (!s_Initialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
            return;
        }

        Debug.Log("[Steam] 성공적인 Steam 연결 완료");

        // ★ 추가: 콜백 객체 생성 및 스탯 데이터 요청 시작
        m_UserStatsReceived = CallResult<UserStatsReceived_t>.Create(OnUserStatsReceived);
        RequestStats();

        s_EverInitialized = true;
    }

    protected virtual void OnEnable()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
        }

        if (!s_Initialized)
        {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null)
        {
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

    protected virtual void OnDestroy()
    {
        if (s_Instance != this)
        {
            return;
        }

        s_Instance = null;

        if (!s_Initialized)
        {
            return;
        }

        SteamAPI.Shutdown();
    }

    protected virtual void Update()
    {
        if (!s_Initialized)
        {
            return;
        }

        // 스팀 콜백 실행 (이게 돌아가야 데이터 도착 알림을 받을 수 있습니다)
        SteamAPI.RunCallbacks();
    }

    protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
    protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

    // =========================================================
    // ★ 새로 추가된 업적(스탯) 요청 및 콜백 처리 로직
    // =========================================================

    private void RequestStats()
    {
        CSteamID currentUserID = SteamUser.GetSteamID();

        // 1. 서버에 데이터 요청하고 비동기 핸들을 받음
        SteamAPICall_t hSteamAPICall = SteamUserStats.RequestUserStats(currentUserID);

        // 2. 요청이 정상적으로 들어갔는지 확인
        if (hSteamAPICall != SteamAPICall_t.Invalid)
        {
            // 요청 성공! 결과가 도착하면 OnUserStatsReceived 함수가 실행되도록 핸들 연결
            m_UserStatsReceived.Set(hSteamAPICall);
            Debug.Log("[Steam] 업적 데이터 요청 전송 완료. 서버 응답 대기 중...");
        }
        else
        {
            Debug.LogError("[Steam] 업적 데이터 요청 실패. Steam에 정상 로그인되어 있는지 확인하세요.");
        }
    }

    // 서버에서 데이터가 도착하면 자동으로 실행되는 콜백 함수
    private void OnUserStatsReceived(UserStatsReceived_t pCallback, bool bIOFailure)
    {
        if (!bIOFailure && pCallback.m_eResult == EResult.k_EResultOK)
        {
            IsStatsLoaded = true;
            Debug.Log("[Steam] 유저 업적 데이터 로딩 완료! (IsStatsLoaded = true)");

            // 1. 스팀 서버가 인식하고 있는 이 게임의 총 업적 개수를 가져옵니다.
            uint numAchievements = SteamUserStats.GetNumAchievements();
            Debug.Log($"<color=cyan>[Steam] 현재 이 게임에 등록된 업적 총 개수: {numAchievements}개</color>");

            // 2. 반복문을 돌면서 모든 업적의 'API 이름'을 로그로 찍습니다.
            for (uint i = 0; i < numAchievements; i++)
            {
                // 이 apiName이 UnlockAchievement("여기에") 들어갈 진짜 이름입니다.
                string apiName = SteamUserStats.GetAchievementName(i);

                // 달성 여부도 같이 확인해볼까요?
                bool isUnlocked;
                SteamUserStats.GetAchievement(apiName, out isUnlocked);
                string status = isUnlocked ? "<color=green>해금됨</color>" : "<color=red>잠김</color>";

                Debug.Log($"<b>[Steam 업적 리스트 {i}]</b> API ID: <color=yellow>{apiName}</color> | 상태: {status}");
            }
        }
        else
        {
            Debug.LogError($"[Steam] 데이터 로딩 실패. Result: {pCallback.m_eResult}");
        }
    }
}