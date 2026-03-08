using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TMP 사용을 위해 추가
using UnityEngine.Localization; // Localization 사용을 위해 추가

public class StageSceneLocalizationController : MonoBehaviour
{
    [Header("Localization References")]
    // 인스펙터에서 "레벨 {0}" 또는 "{0}단계" 등의 키를 연결하세요.
    public LocalizedString normalLevelFormat;

    // 인스펙터에서 "이벤트 {0}" 또는 "보너스 {0}" 등의 키를 연결하세요.
    public LocalizedString event_Winter_LevelFormat;

    public LocalizedString event_City_LevelFormat;

    public List<LocalizedString> levelGuide_localizedString;

    /// <summary>
    /// 레벨 표현 UI를 현재 언어와 맵 종류에 맞게 업데이트합니다.
    /// </summary>
    /// <param name="stageLevelInfo_TMP">텍스트를 출력할 TMP 컴포넌트</param>
    /// <param name="currentPageLevel">현재 표시할 레벨 숫자</param>
    /// <param name="isEventMap">이벤트 맵 여부</param>
    public void RefreshLevelInfoUI(TMP_Text stageLevelInfo_TMP, int currentPageLevel, bool isEventMap)
    {
        if (stageLevelInfo_TMP == null)
        {
            Debug.LogWarning("RefreshLevelInfoUI: stageLevelInfo_TMP가 할당되지 않았습니다.");
            return;
        }

        // 1. 맵 종류에 따라 사용할 로컬라이즈 변수(LocalizedString)를 선택합니다.
        LocalizedString eventLevelFormat = currentPageLevel == 2 ? event_City_LevelFormat : event_Winter_LevelFormat;
        LocalizedString selectedFormat = isEventMap ? eventLevelFormat : normalLevelFormat;

        // 2. Arguments(인자) 설정 없이, 현재 언어에 맞는 순수 텍스트를 통째로 가져옵니다. 
        // (시트 내용: "제0막", "Act 0" 등)
        string localizedText = selectedFormat.GetLocalizedString();

        // 3. '0' 자리에 들어갈 실제 문자열을 결정합니다. (2페이지면 "?", 아니면 레벨+1)
        string levelValue = (currentPageLevel == 2) ? "?" : (currentPageLevel + 1).ToString();

        // 4. 가져온 텍스트에서 "0"을 찾아서 우리가 만든 값(levelValue)으로 바꿔치기 합니다.
        stageLevelInfo_TMP.text = localizedText.Replace("0", levelValue);
    }
}