using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class MovingEnemy : MonoBehaviour
{
    [SerializeField] private GamePlayDefine.MovingAttackType enemyType = GamePlayDefine.MovingAttackType.D;
    [SerializeField] private float colliderDebuf = 0.8f;

    private float currentSpeed = 0f;
    private Vector3 playerPos = Vector3.zero;

    public void SetDead()
    {
        Poolable poolable = GetComponent<Poolable>();
        Managers.Pool.Push(poolable);
    }

    public void SetSpeed(float defaultSpeed, float rangeDebuf)
    { 
        currentSpeed = defaultSpeed;
        colliderDebuf = rangeDebuf;
    }

    private void Update()
    {
        // ���� ���ο� ��ġ ���
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerPos, currentSpeed * Time.deltaTime);
        // ���� ���ο� ��ġ�� �̵�
        transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "detectArea")
        {
            Managers.Game.AddAttackableEnemy(enemyType.ToString(), this.gameObject); 
            currentSpeed *= colliderDebuf;
        }
        else if (collision.tag == "Player")
        {
            Managers.Game.DecHealth();
            SetDead();
        }
    }
}
