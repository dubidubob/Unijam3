using System.Collections;
using UnityEngine;
using DG.Tweening;

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
    public bool isKnockbacked=false;
    private Coroutine _hidingCoroutine;

    private float movingDistanceTmp;
    private int attackValue = 1;

    private float backwardDuration, knockbackDistance;
    private SpriteRenderer spriteRenderer;
    private void OnEnable()
    {
        _elapsedTime = 0f;
        knockback = new Knockback();
        isKnockbacked = false;
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

    public void SetDead()
    {
        KillingDO();
        StopCoroutine(HidingAnimation(spriteRenderer));
        Poolable poolable = GetComponent<Poolable>();
        Managers.Pool.Push(poolable);
    }
    private void KillingDO()
    {
        DOTween.Kill(transform, "FIFO");
        DOTween.Kill(transform, "Speeding");
    }

    public void SetVariance(float distance, MonsterData monster, Vector2 sizeDiffRate, Vector3 playerPos, GamePlayDefine.WASDType wasdType)
    {
        movingDistanceTmp = distance;
        this.playerPos = playerPos;
        enemyType = wasdType;
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

        }
        knockback.OnKnockback(isTrue);
    }

    public void SetSpeeding(bool isTrue)
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (isTrue)
        {
            Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;

            transform.DOMove(targetPos, movingDuration).SetEase(Ease.InQuint).SetId("Speeding");
            spriteRenderer.color = Color.yellow;
        }

    }

    public void SetFIFO(bool isTrue)
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (isTrue)
        {
            Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;
            transform.DOMove(targetPos, movingDuration).SetEase(Ease.InOutCirc).SetId("FIFO");
            spriteRenderer.color = Color.white;
        }

    }
        public void SetHiding(bool isTrue)
        {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (isTrue)
        {
            spriteRenderer.color = Color.white;
            // 숨기기 코루틴 시작
            if (_hidingCoroutine == null)
            {
                _hidingCoroutine = StartCoroutine(HidingAnimation(spriteRenderer));
            }

        }
        else
        {
           
        }
       
    }
    private IEnumerator HidingAnimation(SpriteRenderer _spriteRenderer)
    {
        // 무한 루프를 통해 반복적으로 깜빡임
        while (true)
        {
            // 서서히 투명하게 변함 (0.7초 동안)
            _spriteRenderer.DOFade(0f, movingDuration/3f); // 몇초에 걸쳐서 투명화 되는지?
            yield return new WaitForSeconds(movingDuration/2.4f); // 몇초에 걸쳐서 

            // 서서히 원래대로 돌아옴 (0.7초 동안)
            _spriteRenderer.DOFade(1f, movingDuration/3f);
            yield return new WaitForSeconds(movingDuration/2f);
        }
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
            while (IngameData.Pause)
            {
                yield return null; // 일시 정지 상태에서 기다림
            }
            // 진행되고 있는 distance에서 movingDuration의 1/4 시간 동안 뒤로 물러나고,
            float t = elapsedTime / backwardDuration;
            // 이때 가속도가 붙기
            float knockbackMovement = Mathf.Lerp(knockbackDistance, 0, (t * t));
            transform.position += knockbackDirection * knockbackMovement * Time.deltaTime * 2.0f;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 앞으로 갈 때는 등속도 운동을 해야함.
        isKnockbackActive = false;
    }

    private void FixedUpdate()
    {
        if (IngameData.Pause) return;

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
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerPos, speed * Time.deltaTime);
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
            Managers.Game.PlayerAttacked(attackValue);
        }
    }

    private Vector3 CalculateNormalVector()
    {
        if(enemyType==GamePlayDefine.WASDType.A)
        {
            return Vector3.right;
        }
        else if(enemyType==GamePlayDefine.WASDType.D)
        {
            return Vector3.left;
        }
        else if(enemyType==GamePlayDefine.WASDType.W)
        {
            return Vector3.down;
        }
        else if(enemyType==GamePlayDefine.WASDType.S)
        {
            return Vector3.up;
        }
        Debug.Log("CalculateVector실패");
        return Vector3.forward;
    }
}
