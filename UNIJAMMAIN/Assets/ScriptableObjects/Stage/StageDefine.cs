using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class StageDefine
{
    public string stageName;
    public int stageNumber;
    public Sprite stageBackgroundImage;
    public Sprite stageBackgroundImageGray;
    [TextArea] // 여러 줄 입력이 가능하도록 TextArea 어트리뷰트 사용
    public string stageDescription;
}
