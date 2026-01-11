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
#endif
}