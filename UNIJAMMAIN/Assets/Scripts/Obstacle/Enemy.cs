using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform playerTransform;
    public float moveSpeed = 2f;  // ���� �̵� �ӵ�
  
    public enum State // player�� �ݶ��̴� �ȿ� ���� �ٲ�
    { 
        Attackable,
        NonAttackable,
        Dead,
    }
    private State attack { get; set; }
    private SpriteRenderer spriteRenderer { get; set; }
    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform; // �÷��̾��� ������ġ�ޱ�
    }

    private void Update()
    {
        // �÷��̾ ���� ���� ���
        Vector3 direction = (playerTransform.position - transform.position).normalized;

        // ���� ���ο� ��ġ ���
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

        // ���� ���ο� ��ġ�� �̵�
        transform.position = newPosition;
    }
    private void Awake()
    {
        spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        attack = State.Attackable;
        SetType();
        SendEnemyTransform();



    }

    private void SetType()
    {
        //enemyType = (AttackType)Random.Range(0, (int)AttackType.MaxCnt);
        //Debug.Log($"this enemyType {enemyType}");

        //switch (enemyType)
        //{
        //    case AttackType.keyW:
        //        spriteRenderer.color = Color.red;
        //        break;
        //    case AttackType.keyA:
        //        spriteRenderer.color = Color.yellow;
        //        break;
        //    case AttackType.keyS:
        //        spriteRenderer.color = Color.blue;
        //        break;
        //    case AttackType.keyD:
        //        spriteRenderer.color = Color.black;
        //        break;
        //    default:
        //        Debug.Log("Key not matched");
        //        break;
        //}
    }

    private void SendEnemyTransform()
    { 

    }

    public void SetDead()
    { 
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "detectArea")
        {
            SetDead();
        }
        else if (collision.tag == "Player")
        {
            Debug.Log("Player�� ����");
        }
    }
}
