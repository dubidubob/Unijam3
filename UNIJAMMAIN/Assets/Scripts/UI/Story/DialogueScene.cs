using DG.Tweening;
using UnityEngine;

public enum DialogueCharacter
{
    HideDialog,
    None,      
    Player,
    Master,
    CorruptMaster,
    Boss,
    Evil,
    MovingEnemy1,
    MovingEnemy2,
    MovingEnemy3,
    MovingEnemy4
}

[System.Serializable]
public class DialogueScene
{
    [Header("대사 주인공 선택 (None은 배경대사)")]
    public CharacterData speakingCharacterData;
    public bool isFirstAppearance;
    [TextArea]
    public string text;
    public Sprite overrideSprite;
    public Vector2 panelPositionOffset;
    public float preDelay;
    public float postDelay;
    public bool showLeftCharacter;
    public bool showRightCharacter;
    public bool isAnger;
    public bool isSurprized;
    public bool leftSDAnim;
    public bool rightSDAnim;
    public bool isEnding;
    public bool XFlip;

    public float goingTimeAmount;
    public KeyCode requiredKey;

    [HideInInspector]            // inspector에 안 보이게 (원하면 public으로 노출)
    public string localizationKey; // 자동으로 채워질 키 (예: Story1_scenes_0_text)

    [Header("캐릭터 Y축 오프셋 (띄우기)")]
    public float spriteYOffset = 0f;  // 0 = 바닥

    [Header("배경 전환 연출")]
    public bool triggerBackgroundFade = false; // 이 대사가 끝나고 배경 전환 발동
    public Sprite newBackgroundSprite;       // 새로 교체할 배경 이미지
}