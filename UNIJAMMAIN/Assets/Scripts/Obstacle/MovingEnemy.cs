using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class MovingEnemy : MonoBehaviour
{
    private GamePlayDefine.WASDType enemyType;

    private Vector3 playerPos = Vector3.zero;
    private float speed, intervalBetweenNext, movingDuration;
    private float _elapsedTime;
    private KnockbackPattern knockback;
    private SpriteRenderer monsterImg;
    private Vector3 origin;
    private bool isResizeable = false;
    private Vector2 sizeDiffRate;
    private float _enabledTime;
    private void OnEnable()
    {
        _elapsedTime = 0f;
        // 현재 시각 기록
        _enabledTime = Time.time;
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

    public void SetVariance(float distance, float movingDuration, int numInRow, Vector2 sizeDiffRate, GamePlayDefine.WASDType wasdType)
    {
        enemyType = wasdType;
        this.sizeDiffRate = sizeDiffRate;
        this.movingDuration = movingDuration;
        speed = distance / this.movingDuration;

        intervalBetweenNext = distance / (float)numInRow;
    }
    public void SetKnockback(bool isTrue)
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (isTrue)
        {
            spriteRenderer.color = Color.red;
        }
        else 
        {
            spriteRenderer.color = Color.white;
        }
        knockback.OnKnockback(isTrue);
    }

    private void FixedUpdate()
    {
        if (_elapsedTime <= movingDuration && isResizeable)
        {
            PerspectiveResize(_elapsedTime);
            _elapsedTime += Time.deltaTime;
        }
        Move();
    }

    private void PerspectiveResize(float _elapsedTime)
    {
        float t = _elapsedTime / movingDuration;
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
            Managers.Game.attacks[enemyType].Dequeue();
            SetDead();
            Managers.Game.DecHealth();
        }
        //else if (collision.tag == "test")
        //{
        //    float elapsed = Time.time - _enabledTime;
        //    // Debug.Log($"{enemyType} 경과 시간: {elapsed:F2}초");
        //    Managers.Game.attacks[enemyType].Dequeue();
        //    SetDead();
        //}
    }
}
