using UnityEngine;

public class KeyUIEffect : MonoBehaviour
{
    [SerializeField] bool isWasd = true;

    [SerializeField] GamePlayDefine.WASDType type = GamePlayDefine.WASDType.A;
    [SerializeField] Sprite candidate;

    [SerializeField] GamePlayDefine.DiagonalType type2 = GamePlayDefine.DiagonalType.RightUp;
    
    private Sprite basic;
    private Color baseColor;
    private SpriteRenderer[] sp;
    private void Awake()
    {
        sp = GetComponentsInChildren<SpriteRenderer>(true);

        if (isWasd)
        {
            baseColor = sp[0].color;
            basic = sp[1].sprite;
            sp[2].gameObject.SetActive(false);

            Managers.Input.KeyBoardChecking -= TurnUIEffect;
            Managers.Input.KeyBoardChecking += TurnUIEffect;
        }
        else
        {
            sp[0].enabled = false;

            Managers.Input.KeyArrowcodeAction -= TurnUIEffect;
            Managers.Input.KeyArrowcodeAction += TurnUIEffect;
        }
    }

    private void ChangePos()
    {
        
    }
    private void TurnUIEffect(GamePlayDefine.DiagonalType t)
    {
        if (type2 == t)
        {
            sp[0].enabled = true;
            Invoke("TurnOff", 0.2f);
        }
    }

    private void TurnUIEffect(GamePlayDefine.WASDType t)
    {
        if (type == t)
        {
            // sp[0].color = new Color32(0xFF, 0xFB, 0x37, 0xFF);
            sp[1].sprite = candidate;
            sp[2].gameObject.SetActive(true);
            Managers.Game.accuracy.ShowAccuracy(Accuracy.AccuracyState.Perfect);
            Invoke("TurnOff", 0.2f);
            
           
        }        
    }

    private void TurnOff()
    {
        if (isWasd)
        {
            sp[0].color = baseColor;
            sp[1].sprite = basic;
            sp[2].gameObject.SetActive(false);
        }
        else
        {
            sp[0].enabled = false;
        }
    }
}
