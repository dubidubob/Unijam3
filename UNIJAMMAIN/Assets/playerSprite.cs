using UnityEngine;

public class playerSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite W, A, S, D, LU, LD, RU, RD;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Managers.Input.KeyBoardChecking -= forWASD;
        Managers.Input.KeyBoardChecking += forWASD;
        Managers.Input.KeyArrowcodeAction -= forLR;
        Managers.Input.KeyArrowcodeAction += forLR;
    }

    private void forWASD(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.A:
                spriteRenderer.sprite = A;
                break;
            case KeyCode.W:
                spriteRenderer.sprite = W;
                break;
            case KeyCode.S:
                spriteRenderer.sprite = S;
                break;
            case KeyCode.D:
                spriteRenderer.sprite = D;
                break;
        }
    }

    private void forLR(GamePlayDefine.RangedAttackType type)
    {
        switch (type)
        {
            case GamePlayDefine.RangedAttackType.LeftDown:
                spriteRenderer.sprite = LD;
                break;
            case GamePlayDefine.RangedAttackType.LeftUp:
                spriteRenderer.sprite = LU;
                break;
            case GamePlayDefine.RangedAttackType.RightDown:
                spriteRenderer.sprite = RD;
                break;
            case GamePlayDefine.RangedAttackType.RightUp:
                spriteRenderer.sprite = RU;
                break;
        }
    }
}
