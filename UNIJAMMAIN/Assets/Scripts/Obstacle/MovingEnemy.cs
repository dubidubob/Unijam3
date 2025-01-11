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

    public void SetSpeed(float defaultSpeed)
    { 
        currentSpeed = defaultSpeed;
    }

    private void Update()
    {
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerPos, currentSpeed * Time.deltaTime);
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
