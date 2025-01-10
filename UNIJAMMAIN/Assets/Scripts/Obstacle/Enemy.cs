using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;  // ���� �̵� �ӵ�
    [SerializeField] private GamePlayDefine.AttackType enemyType = GamePlayDefine.AttackType.D;
    [SerializeField] private float upDownDebuf = 0.5f;
    [SerializeField] private float colliderDebuf = 0.5f;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform; // �÷��̾��� ������ġ�ޱ�

        if (enemyType == GamePlayDefine.AttackType.W || enemyType == GamePlayDefine.AttackType.S)
        {
            moveSpeed *= upDownDebuf;
        }
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
    public void SetDead()
    {
        Destroy(gameObject);
    }
}
