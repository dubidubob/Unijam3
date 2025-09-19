using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageUIData
{
    public string stageMainText;
    public string stageMainSubText;
    [TextArea(2,5)] // 여러줄 인식받기
    public string stageExplain;
    [TextArea(1,3)]
    public string levelText; 
}
