using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class MovingEnemy : MonoBehaviour
{
    [SerializeField] private GamePlayDefine.MovingAttackType enemyType = GamePlayDefine.MovingAttackType.D;
    [SerializeField] private float startPosScale = 4.0f;
    [SerializeField] private float endPosScale = 2.8f;
    private float speed = 1f;
    private Vector3 playerPos = Vector3.zero;
    private float duration;

    // 현재 내 시작 위치에서 targetpos까지 1초 걸려야한다면, speed가 몇이어야 하나?
    private void Calculate()
    {
        float distance = startPosScale - endPosScale;
        // 원하는 시간(duration)에 도착하려면 speed = distance / duration
        duration = 1f;
        speed = distance / duration;
    }

    private void OnEnable()
    {
        Calculate();
    }

    public void SetDead()
    {
        Poolable poolable = GetComponent<Poolable>();
        Managers.Pool.Push(poolable);
    }

    public void SetSpeed(float movingDuration)
    {
        duration = movingDuration;
    }
   
    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerPos, speed * Time.deltaTime);
        transform.position = newPosition;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "detectArea")
        {
            Managers.Game.AddAttackableEnemy(enemyType.ToString(), this.gameObject); 
        }
        else if (collision.tag == "Player")
        {
            Managers.Game.DecHealth();
            SetDead();
        }
    }
}
