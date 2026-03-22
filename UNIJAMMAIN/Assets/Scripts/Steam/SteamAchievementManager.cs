using UnityEngine;
using Steamworks;

public class SteamAchievementManager
{
    // Callback 대신 CallResult를 사용해야 합니다.
    private CallResult<UserStatsReceived_t> m_UserStatsReceived;


    public void Init()
    {
#if UNITY_EDITOR
        return;
#endif
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("[Steam] Steam Manager가 초기화되지 않았습니다.");
            return;
        }

        // 1. 비동기 호출 결과를 받을 CallResult 생성
        m_UserStatsReceived = CallResult<UserStatsReceived_t>.Create(OnUserStatsReceived);

        // 2. 현재 유저의 SteamID를 가져와 스탯 요청 (SteamAPICall_t 반환)
        CSteamID mySteamID = SteamUser.GetSteamID();
        SteamAPICall_t handle = SteamUserStats.RequestUserStats(mySteamID);

        // 3. CallResult에 핸들 연결
        m_UserStatsReceived.Set(handle);
    }

    private void OnUserStatsReceived(UserStatsReceived_t pCallback, bool bIOFailure)
    {
        // bIOFailure가 false이고, 결과가 OK일 때 성공
        if (!bIOFailure && pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("[Steam] 유저 스탯/업적 정보를 성공적으로 불러왔습니다.");
        }
        else
        {
            Debug.LogError($"[Steam] 유저 스탯 정보 로드 실패. IO 오류: {bIOFailure}, 결과: {pCallback.m_eResult}");
        }
    }
        
    public void UnlockAchievement(string achievementID)
    {
#if UNITY_EDITOR
        return;
#endif
        // ★ 이 줄을 추가해서 데이터가 로드 안 됐으면 튕겨내게 하세요
        if (!SteamManager.Initialized || !SteamManager.IsStatsLoaded)
        {
            Debug.LogWarning($"[Steam] 아직 업적 데이터가 준비되지 않았습니다. (요청 업적: {achievementID})");
            return;
        }

        bool isAchieved = false;
        bool result = SteamUserStats.GetAchievement(achievementID, out isAchieved);

        // API 호출 실패 (스탯 로딩 전이거나 ID가 잘못됨)
        if (!result)
        {
            Debug.LogWarning($"[Steam] 업적 상태를 확인할 수 없습니다. (ID: {achievementID}) - 스탯 로딩 전일 수 있습니다.");
            return;
        }

        // 이미 달성한 경우 중단
        if (isAchieved)
        {
            Debug.Log($"[Steam] 업적을 이미 달성했습니다 : {achievementID}");
            return;
        }

        // 달성 안 했다면 서버에 전송
        SteamUserStats.SetAchievement(achievementID);
        SteamUserStats.StoreStats();

        Debug.Log($"[Steam] 업적 신규 달성! : {achievementID}");
    }

    // 스팀 업적을 다시 잠금 상태로 되돌리는 함수
    public void ResetAchievement(string achievementID)
    {
        if (!SteamManager.Initialized) return;

        // 업적 클리어 상태 취소
        SteamUserStats.ClearAchievement(achievementID);
        // 상태 서버에 저장
        SteamUserStats.StoreStats();

        Debug.Log($"[Steam] 업적 상태가 초기화되었습니다. 다시 테스트 가능합니다: {achievementID}");
    }
    public void ResetAllSteamAchievements()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("[Steam] 스팀이 초기화되지 않아 업적을 초기화할 수 없습니다.");
            return;
        }

        // true를 넣으면 업적(Achievements)을 포함한 모든 스탯(Stats)을 초기화합니다.
        bool success = SteamUserStats.ResetAllStats(true);

        if (success)
        {
            // 서버에 즉시 반영
            SteamUserStats.StoreStats();
            Debug.Log("[Steam] 모든 스팀 업적 및 통계가 초기화되었습니다.");
        }
        else
        {
            Debug.LogError("[Steam] 업적 초기화 실패");
        }
    }

    public void SetStat(string statName, int value)
    {
#if UNITY_EDITOR
        return;
#endif
        // 스팀 통계 값 설정
        Steamworks.SteamUserStats.SetStat(statName, value);
        // 설정한 값을 서버로 전송 (이걸 호출해야 스팀 오버레이에 즉각 반영됨)
        Steamworks.SteamUserStats.StoreStats();
    }


}