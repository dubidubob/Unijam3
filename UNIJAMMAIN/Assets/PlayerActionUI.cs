using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ActionImg 
{
    public GamePlayDefine.AllType type;
    public Sprite sprite;
}
public class PlayerActionUI : MonoBehaviour
{
    [SerializeField] List<ActionImg> actionImgs = new List<ActionImg>();
    private Dictionary<GamePlayDefine.AllType, Sprite> _actionImgsDic;
    private SpriteRenderer sp;
    private Sprite origin;

    void Start()
    {
        sp = GetComponent<SpriteRenderer>();
        origin = sp.sprite;

        _actionImgsDic = new Dictionary<GamePlayDefine.AllType, Sprite>();
        foreach (var a in actionImgs)
        {
            _actionImgsDic[a.type] = a.sprite;
        }

        Managers.Input.KeyBoardChecking -= ChangePlayerSprite;
        Managers.Input.KeyBoardChecking += ChangePlayerSprite;

        Managers.Input.KeyArrowcodeAction -= ChangePlayerSprite_Arrow;
        Managers.Input.KeyArrowcodeAction += ChangePlayerSprite_Arrow;


    }

    private void ChangePlayerSprite(GamePlayDefine.WASDType type)
    {
        sp.sprite = _actionImgsDic[Util.WASDTypeChange(type)];
        Invoke("Recover", 0.4f);
    }

    private void ChangePlayerSprite_Arrow(GamePlayDefine.DiagonalType type)
    {
        sp.sprite = _actionImgsDic[Util.DiagonalTypeChange(type)];
        Invoke("Recover", 0.4f);
    }

    private void Recover()
    {
        sp.sprite = origin;
    }
    
}
