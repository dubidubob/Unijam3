using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;  // ���� �̵� �ӵ�
    [SerializeField] private GamePlayDefine.AttackType enemyType = GamePlayDefine.AttackType.D;
    [SerializeField] bool ArrowCheck;
    [SerializeField] private GamePlayDefine.RangedAttackType RangedenemyType = GamePlayDefine.RangedAttackType.LeftDown;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform; // �÷��̾��� ������ġ�ޱ�
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
