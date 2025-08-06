using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class MovingEnemy : MonoBehaviour
{
    [SerializeField] private GamePlayDefine.MovingAttackType enemyType = GamePlayDefine.MovingAttackType.D;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float startPosScale = 4.0f;
    [SerializeField] private float endPosScale = 2.8f;    
    private Vector3 playerPos = Vector3.zero;
    private float speed;
    private float intervalBetweenNext;
    private KnockbackPattern knockback;

    private void OnEnable()
    {
        playerPos = Managers.Game.playerTransform.position;
        knockback = new KnockbackPattern();
    }
    
    public bool CheckCanDead()
    {
        if (knockback.CheckKnockback())
        {
            ExecuteKnockback();
            return false;
        }
        return true;
    }

    private void ExecuteKnockback()
    {
        Vector3 tmp = (playerPos - transform.position).normalized * intervalBetweenNext;
        transform.position -= tmp;
    }

    public void SetDead()
    {
        Poolable poolable = GetComponent<Poolable>();
        Managers.Pool.Push(poolable);
    }

    public void SetSpeed(float movingDuration, int numInRow)
    {
        float spawnInterval = movingDuration;

        float distance = startPosScale - endPosScale;
        speed = distance / spawnInterval;

        intervalBetweenNext = distance / (float)numInRow;
    }
    public void SetKnockback(bool isTrue)
    {
        if (isTrue)
        {
            spriteRenderer.color = Color.gray;
        }
        else 
        {
            spriteRenderer.color = Color.white;
        }
        knockback.OnKnockback(isTrue);
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
        else if (collision.tag == "dangerLine")
        {
            Managers.Game.DecHealth();
            if (CheckCanDead())
            {
                SetDead();
            }
        }
    }
}
