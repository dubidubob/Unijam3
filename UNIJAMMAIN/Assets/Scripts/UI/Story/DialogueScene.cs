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
    
}