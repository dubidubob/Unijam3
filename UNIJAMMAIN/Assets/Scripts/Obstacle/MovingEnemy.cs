using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class MovingEnemy : MonoBehaviour
{
    public float moveSpeed = 2f;  // ���� �̵� �ӵ�

    [SerializeField] private GamePlayDefine.MovingAttackType enemyType = GamePlayDefine.MovingAttackType.D;
    [SerializeField] private float upDownDebuf = 0.7f;
    [SerializeField] private float colliderDebuf = 0.8f;
    private Transform playerTransform;
    private Poolable poolable;
    public void SetDead()
    {
        Managers.Pool.Push(poolable);
        //Destroy(gameObject);
    }

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform; // �÷��̾��� ������ġ�ޱ�
        poolable = GetComponent<Poolable>();
        DebufUpdown();
    }

    private void DebufUpdown()
    {
        if (enemyType == GamePlayDefine.MovingAttackType.W || enemyType == GamePlayDefine.MovingAttackType.S)
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
}
