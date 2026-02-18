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
    public LocalizedString eventLevelFormat;

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

        // 1. 맵 종류에 따라 사용할 로컬라이즈 형식을 선택합니다.
        LocalizedString selectedFormat = isEventMap ? eventLevelFormat : normalLevelFormat;

        // 2. 변수(Arguments)를 설정합니다. {0} 자리에 currentPageLevel이 들어갑니다.
        selectedFormat.Arguments = new object[] { currentPageLevel };

        // 3. 번역된 텍스트를 TMP에 적용합니다.
        // GetLocalizedString()은 현재 설정된 언어에 맞는 텍스트를 즉시 반환합니다.
        stageLevelInfo_TMP.text = selectedFormat.GetLocalizedString();
    }
}