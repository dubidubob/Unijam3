using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // 에디터 관련 기능 사용
#endif
public class EditorUtils
{
    // 유니티 상단 메뉴 [Tools] -> [Clear Save Data]를 누르면 실행됨
#if UNITY_EDITOR
    [MenuItem("Tools/Clear Save Data")]
    public static void ClearSaveData()
    {
        // 1. IngameData의 리셋 기능 호출
        IngameData.ResetData();

        // 2. 확실하게 하기 위해 PlayerPrefs 전체 삭제 (선택 사항)
        // PlayerPrefs.DeleteAll(); 

        Debug.Log("★ 저장 데이터가 초기화되었습니다! (Play 버튼 누르기 전)");
    }
    // 유니티 상단 메뉴 [Tools] -> [Unlock All Stages]를 누르면 실행됨
    [MenuItem("Tools/Unlock All Stages")]
    public static void UnlockAllStage()
    {
        // 1. StageScene의 Test 변수 로직과 동일하게 데이터 덮어쓰기
        IngameData._isStoryCompleteClear = true;
        IngameData._nowStageIndex = 0;
        IngameData._unLockStageIndex = 7; // 마지막 스테이지 해금 상태

        // 편의를 위해 프롤로그를 본 것으로 처리하려면 아래 주석을 해제하세요.
        // IngameData._isPrologueWatched = true; 

        // 2. 변경된 상태를 스팀 클라우드 연동 JSON 파일에 바로 저장
        IngameData.SaveGameData();

        Debug.Log("★ 모든 스테이지가 해금되었습니다! (데이터 저장 완료)");
    }
#endif
}