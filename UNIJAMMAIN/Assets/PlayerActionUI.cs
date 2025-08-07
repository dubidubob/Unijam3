using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ActionImg 
{
    public GamePlayDefine.WASDType wasdType;
    public Sprite sprite;
}
public class PlayerActionUI : MonoBehaviour
{
    [SerializeField] List<ActionImg> actionImgs = new List<ActionImg>();
    private Dictionary<GamePlayDefine.WASDType, Sprite> _actionImgsDic;
    private SpriteRenderer sp;
    private Sprite origin;
    void Start()
    {
        sp = GetComponent<SpriteRenderer>();
        origin = sp.sprite;

        _actionImgsDic = new Dictionary<GamePlayDefine.WASDType, Sprite>();
        foreach (var a in actionImgs)
        {
            _actionImgsDic[a.wasdType] = a.sprite;
        }

        Managers.Input.KeyBoardChecking -= ChangePlayerSprite;
        Managers.Input.KeyBoardChecking += ChangePlayerSprite;
    }

    private void ChangePlayerSprite(GamePlayDefine.WASDType type)
    { 
        sp.sprite = _actionImgsDic[type];
        Invoke("Recover", 0.2f);
    }

    private void Recover()
    {
        sp.sprite = origin;
    }
}
