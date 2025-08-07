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
    private SpriteRenderer monsterImg;
    private Vector3 origin;
    private bool isResizeable = false;
    private Vector2 sizeDiffRate;

    private float instantDist;
    private float instantMovingDuration;
    private void OnEnable()
    {
        _elapsedTime = 0f;
        playerPos = Managers.Game.playerTransform.position;
        knockback = new KnockbackPattern();
        monsterImg = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (monsterImg != null)
        {
            origin = monsterImg.transform.localScale;
        }

        if(enemyType == GamePlayDefine.WASDType.W || enemyType == GamePlayDefine.WASDType.S)
            isResizeable = true;
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

    public void SetVariance(float distance, float movingDuration, int numInRow, Vector2 sizeDiffRate, Vector3 spawnPos)
    {
        this.sizeDiffRate = sizeDiffRate;
        this.movingDuration = movingDuration;
        speed = distance / this.movingDuration;

        instantDist = Vector3.Distance(spawnPos, playerPos);
        instantMovingDuration = instantDist / speed;
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
        if (_elapsedTime <= instantMovingDuration && isResizeable)
        {
            PerspectiveResize(_elapsedTime);
            _elapsedTime += Time.deltaTime;
        }
        Move();
    }

    private void PerspectiveResize(float _elapsedTime)
    {
        float t = _elapsedTime / instantMovingDuration;
        Debug.Log(t);
        t = Mathf.Clamp01(t); // 0~1로 고정 확인

        if (enemyType == GamePlayDefine.WASDType.W) // 작아졌다 커지기
        {
            monsterImg.transform.localScale = Vector3.Lerp(origin * sizeDiffRate.x, origin, t);
        }
        else if (enemyType == GamePlayDefine.WASDType.S) // 커졌다 작아지기
        {
            monsterImg.transform.localScale = Vector3.Lerp(origin * sizeDiffRate.y, origin, t);
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
