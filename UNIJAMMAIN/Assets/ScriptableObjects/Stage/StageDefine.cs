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
    [TextArea] // ���� �� �Է��� �����ϵ��� TextArea ��Ʈ����Ʈ ���
    public string stageDescription;
}
