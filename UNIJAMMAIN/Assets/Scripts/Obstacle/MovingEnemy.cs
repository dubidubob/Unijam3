using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class MovingEnemy : MonoBehaviour
{
    [SerializeField] private GamePlayDefine.WASDType enemyType = GamePlayDefine.WASDType.D;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Vector3 playerPos = Vector3.zero;
    private float speed, intervalBetweenNext, movingDuration;
    private float _elapsedTime;
    private KnockbackPattern knockback;

    private void OnEnable()
    {
        _elapsedTime = 0f;
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

    public void SetSpeed(float distance, float movingDuration, int numInRow)
    {
        this.movingDuration = movingDuration;
        speed = distance / this.movingDuration;

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
        while (_elapsedTime <= movingDuration)
        {
            PerspectiveResize(_elapsedTime);
            _elapsedTime += Time.deltaTime;
        }
        Move();
    }

    private void PerspectiveResize(float _elapsedTime)
    {
        float t = _elapsedTime / movingDuration; // 0에서 1 사이의 값
        t = Mathf.Clamp01(t); // 혹시 몰라서 0~1로 고정

        if (enemyType == GamePlayDefine.WASDType.W) // 작아졌다 커지기
        {
            // 처음엔 작고 → 점점 커짐 (0.5 → 1.0)
            transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, t);
        }
        else if (enemyType == GamePlayDefine.WASDType.D) // 커졌다 작아지기
        {
            // 처음엔 크고 → 점점 작아짐 (1.5 → 1.0)
            transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t);
        }
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
            Managers.Game.AddAttackableEnemy(enemyType, this.gameObject);
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
