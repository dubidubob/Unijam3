using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class StageUIData
{
    // 이제 인스펙터에서 텍스트 대신 'Localization Key'를 선택하게 됩니다.
    public LocalizedString stageMainText;
    public LocalizedString stageMainSubText;

    // 설명글과 레벨 텍스트도 변환
    public LocalizedString stageExplain;
    public LocalizedString levelText;
}