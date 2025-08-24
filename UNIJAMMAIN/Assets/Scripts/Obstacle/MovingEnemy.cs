using System.Collections;
using UnityEngine;

public class Knockback
{
    bool isEnabled;
    int hp;
    public void OnKnockback(bool isOn)
    {
        isEnabled = isOn;
        hp = 2;
    }

    public bool CheckKnockback()
    {
        if (!isEnabled) return false;
        if (--hp <= 0) return false;

        return true;
    }
}

[RequireComponent(typeof(Poolable))]
public class MovingEnemy : MonoBehaviour
{
    private GamePlayDefine.WASDType enemyType;

    private Vector3 playerPos = Vector3.zero;
    private float speed, movingDuration;
    private float _elapsedTime;
    private Knockback knockback;
    private SpriteRenderer monsterImg;
    private Vector3 origin;
    private bool isResizeable = false;
    private Vector2 sizeDiffRate;
    private float speedUpRate;
    public bool isKnockbacked=false;

    private float backwardDuration, knockbackDistance;
    private void OnEnable()
    {
        _elapsedTime = 0f;
        knockback = new Knockback();
        isKnockbacked = false;
        monsterImg = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        playerPos = Managers.Game.playerTransform.position;
        if (monsterImg != null)
        {
            origin = monsterImg.transform.localScale;
        }

        if(enemyType == GamePlayDefine.WASDType.W || enemyType == GamePlayDefine.WASDType.S)
            isResizeable = true;
    }

    public void SetDead()
    {
        Poolable poolable = GetComponent<Poolable>();
        Managers.Pool.Push(poolable);
    }

    public void SetVariance(float distance, MonsterData monster, Vector2 sizeDiffRate, GamePlayDefine.WASDType wasdType)
    {
        enemyType = wasdType;
        speedUpRate = monster.speedUpRate;
        movingDuration = (float)IngameData.BeatInterval*monster.moveBeat;
        this.sizeDiffRate = sizeDiffRate;
        backwardDuration = movingDuration * 0.125f;
        knockbackDistance = distance * 0.125f;
        speed = distance / this.movingDuration;
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

    public bool CheckCanDead()
    {
        if (knockback.CheckKnockback())
        {
            if(this.isActiveAndEnabled) StartCoroutine(ExecuteKnockback());
            isKnockbacked = true; 
            return false;
        }
        else
            return true;
    }

    bool isKnockbackActive = false;
    IEnumerator ExecuteKnockback()
    {
        isKnockbackActive = true;
        float elapsedTime = 0f;
        Vector3 knockbackDirection = (transform.position - playerPos).normalized;

        while (elapsedTime < backwardDuration)
        {
            // 진행되고 있는 distance에서 movingDuration의 1/4 시간 동안 뒤로 물러나고,
            float t = elapsedTime / backwardDuration;
            // 이때 가속도가 붙기
            float knockbackMovement = Mathf.Lerp(knockbackDistance, 0, (t * t));
            transform.position += knockbackDirection * knockbackMovement * Time.deltaTime * speedUpRate * 2.0f;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 앞으로 갈 때는 등속도 운동을 해야함.
        isKnockbackActive = false;
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

    private void Move()
    {
        if (isKnockbackActive) return;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerPos, speed * Time.deltaTime * speedUpRate);
        transform.position = newPosition;
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
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "detectArea")
        {
            Managers.Game.AddAttackableEnemy(enemyType, this.gameObject);
        }
        else if (collision.tag == "dangerLine")
        {
            if (Managers.Game.attacks[enemyType].Count == 0) return;

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
