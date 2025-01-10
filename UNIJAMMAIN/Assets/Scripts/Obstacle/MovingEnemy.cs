using UnityEngine;

public class MovingEnemy : MonoBehaviour
{
    /*phase ����*/
    public float moveSpeed = 2f;  // ���� �̵� �ӵ�

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
        playerTransform = GameObject.FindWithTag("Player").transform; // �÷��̾��� ������ġ�ޱ�
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
        if (ArrowCheck==false)
        {
            // �÷��̾ ���� ���� ���
            Vector3 direction = (playerTransform.position - transform.position).normalized;

            // ���� ���ο� ��ġ ���
            Vector3 newPosition = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

            // ���� ���ο� ��ġ�� �̵�
            transform.position = newPosition;
        }
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
