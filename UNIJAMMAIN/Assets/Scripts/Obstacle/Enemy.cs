using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum State // player의 콜라이더 안에 들어가면 바뀜
    { 
        Attackable,
        NonAttackable,
        Dead,
    }



    private AttackType enemyType { get; set; }
    private State attack { get; set; }
    private SpriteRenderer spriteRenderer { get; set; }

    private void Awake()
    {
        spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        attack = State.Attackable;
        SetType();
        SendEnemyTransform();
    }

    private void SetType()
    {
        enemyType = (AttackType)Random.Range(0, (int)AttackType.MaxCnt);
        Debug.Log($"this enemyType {enemyType}");

        switch (enemyType)
        {
            case AttackType.keyW:
                spriteRenderer.color = Color.red;
                break;
            case AttackType.keyA:
                spriteRenderer.color = Color.yellow;
                break;
            case AttackType.keyS:
                spriteRenderer.color = Color.blue;
                break;
            case AttackType.keyD:
                spriteRenderer.color = Color.black;
                break;
            default:
                Debug.Log("Key not matched");
                break;
        }
    }

    private void SendEnemyTransform()
    { 

    }
}
