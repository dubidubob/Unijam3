using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform playerTransform;
    public float moveSpeed = 2f;  // 적의 이동 속도
  
    public enum State // player의 콜라이더 안에 들어가면 바뀜
    { 
        Attackable,
        NonAttackable,
        Dead,
    }
    private State attack { get; set; }
    private SpriteRenderer spriteRenderer { get; set; }
    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform; // 플레이어의 현재위치받기
    }

    private void Update()
    {
        // 플레이어를 향한 방향 계산
        Vector3 direction = (playerTransform.position - transform.position).normalized;

        // 적의 새로운 위치 계산
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

        // 적을 새로운 위치로 이동
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
            Debug.Log("Player와 닿음");
        }
    }
}
