using UnityEngine;

public class MovingEnemy : MonoBehaviour
{
    /*phase 변수*/
    public float moveSpeed = 2f;  // 적의 이동 속도

    [SerializeField] private GamePlayDefine.AttackType enemyType = GamePlayDefine.AttackType.D;
    [SerializeField] private float upDownDebuf = 0.7f;
    [SerializeField] private float colliderDebuf = 0.8f;
    private Transform playerTransform;

    public void SetDead()
    {
        Destroy(gameObject);
    }

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform; // 플레이어의 현재위치받기
        DebufUpdown();
    }

    private void DebufUpdown()
    {
        if (enemyType == GamePlayDefine.AttackType.W || enemyType == GamePlayDefine.AttackType.S)
        {
            moveSpeed *= upDownDebuf;
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "detectArea")
        {
            Managers.Game.AddAttackableEnemy(enemyType.ToString(), this.gameObject); 
            moveSpeed *= colliderDebuf;
        }
        else if (collision.tag == "Player")
        {
            SetDead();
        }
    }
}
