using UnityEngine;

public class KeyUIEffect : MonoBehaviour
{
    [SerializeField] GamePlayDefine.WASDType type = GamePlayDefine.WASDType.A;
    [SerializeField] Sprite candidate;
    private Sprite basic;
    private SpriteRenderer sp;
    private void Awake()
    {
        sp = GetComponentInChildren<SpriteRenderer>();
        basic = sp.sprite;

        Managers.Input.KeyBoardChecking -= TurnUIEffect;
        Managers.Input.KeyBoardChecking += TurnUIEffect;
    }

    private void TurnUIEffect(GamePlayDefine.WASDType t)
    {
        if (type == t)
        { 
            sp.sprite = candidate;
            Invoke("TurnOff", 0.2f);
        }
    }

    private void TurnOff()
    { 
        sp.sprite = basic;
    }
}
