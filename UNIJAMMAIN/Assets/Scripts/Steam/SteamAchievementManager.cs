using UnityEngine;
using Steamworks;

// [중요] : MonoBehaviour 상속 제거!
public class SteamAchievementManager
{
    // 초기화 함수 (Managers.Init() 등에서 호출해줘도 됨)
    public void Init()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam Manager가 초기화되지 않았습니다.");
        }
    }

    public void UnlockAchievement(string achievementID)
    {
        // 1. 스팀이 초기화 안 됐으면 중단
        if (!SteamManager.Initialized) return;

        // 2. [핵심 최적화] 이미 달성한 업적(true)인지 확인
        bool isAchieved = false;
        bool result = SteamUserStats.GetAchievement(achievementID, out isAchieved);

        // API 호출 자체가 실패했거나(result == false), 
        // 이미 달성한 상태(isAchieved == true)라면 여기서 끝냄! (서버 전송 안 함)
        if (!result || isAchieved)
        {
            return;
        }

        // 3. 달성 안 했다면 그때 비로소 서버에 전송
        SteamUserStats.SetAchievement(achievementID);
        SteamUserStats.StoreStats(); // 여기가 비용이 드는 부분인데, 위에서 걸러져서 최초 1회만 실행됨

        Debug.Log($"[Steam] 업적 신규 달성! : {achievementID}");
    }
}