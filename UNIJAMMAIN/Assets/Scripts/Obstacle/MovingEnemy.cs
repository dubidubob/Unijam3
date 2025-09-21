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
    public GameObject dyingEffectObject;
    private Vector3 origin;
    private bool isResizeable = false;
    private Vector2 sizeDiffRate;
    public bool isKnockbacked=false;
    private Coroutine _hidingCoroutine;
    private Define.MonsterType MonsterType;
   
    
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
        DyingAnimation();
        KillingDO();
        StopCoroutine(HidingAnimation(spriteRenderer));
        StartCoroutine(WaitForDyingAnimation());
       
    }
    private void KillingDO()
    {
        DOTween.Kill(transform, "FIFO");
        DOTween.Kill(transform, "Speeding");
    }

    public void SetVariance(float distance, MonsterData monster, Vector2 sizeDiffRate, Vector3 playerPos, GamePlayDefine.WASDType wasdType,Define.MonsterType monsterType)
    {
        MonsterType = monsterType;
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
            Debug.Log("SetknockBack 출력");
            spriteRenderer.color = Managers.Game.monster.GetColor(Define.MonsterType.Knockback);
            spriteRenderer.sprite = Managers.Game.monster.GetSprite(Define.MonsterType.Knockback);
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
            spriteRenderer.sprite = Managers.Game.monster.GetSprite(Define.MonsterType.WASDDash);
            spriteRenderer.color = Managers.Game.monster.GetColor(Define.MonsterType.WASDDash);
        }

    }

    public void SetFIFO(bool isTrue)
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (isTrue)
        {
            Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;
            transform.DOMove(targetPos, movingDuration).SetEase(Ease.InOutCirc).SetId("FIFO");
            spriteRenderer.sprite = Managers.Game.monster.GetSprite(Define.MonsterType.WASDFIFO);
            spriteRenderer.color = Managers.Game.monster.GetColor(Define.MonsterType.WASDFIFO);
        }

    }
    public void SetHiding(bool isTrue)
        {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (isTrue)
        {
            spriteRenderer.sprite = Managers.Game.monster.GetSprite(Define.MonsterType.WASDHiding);
            spriteRenderer.color = Managers.Game.monster.GetColor(Define.MonsterType.WASDHiding);
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
        yield return new WaitForSeconds(movingDuration * 0.1f);

        // 2. 서서히 투명하게 변함 (movingDuration의 30% 시간 동안)
        float fadeOutDuration = movingDuration * 0.3f;
        _spriteRenderer.DOFade(0f, fadeOutDuration);
        yield return new WaitForSeconds(fadeOutDuration);

        // 3. 완전히 투명한 상태로 중간 지점 통과 (movingDuration의 20% 시간 동안)
        yield return new WaitForSeconds(movingDuration * 0.2f);

        // 4. 도착 직전에 서서히 원래대로 돌아옴 (movingDuration의 30% 시간 동안)
        float fadeInDuration = movingDuration * 0.3f;
        _spriteRenderer.DOFade(1f, fadeInDuration);
        yield return new WaitForSeconds(fadeInDuration);
    }
    public bool CheckCanDead()
    {
        // 피격받는 모션 재생
        StartCoroutine(AttackedAnimation());
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

    #region Animation

    IEnumerator AttackedAnimation()
    {
        Sprite tmp = monsterImg.sprite;
        Sprite attackedSprite = Managers.Game.monster.GetAttackedSprite(MonsterType);
        monsterImg.sprite = attackedSprite;

        yield return new WaitForSeconds(0.4f);
        monsterImg.sprite = tmp;

    }

    void DyingAnimation()
    {
        Sprite dyingSprite = Managers.Game.monster.DyingEffectSprite();
        Transform transformTmp = dyingEffectObject.GetComponent<Transform>();
        dyingEffectObject.GetComponent<SpriteRenderer>().sprite = dyingSprite;
        //transform scale 0->1로 변경후 0으로 감소
        transformTmp.DOScale(1, 0.2f).OnComplete(() => transformTmp.DOScale(0, 0.2f));
    }

    IEnumerator WaitForDyingAnimation()
    {
        yield return new WaitForSeconds(0.4f);
        Poolable poolable = GetComponent<Poolable>();
        Managers.Pool.Push(poolable);
    }
    #endregion
}
