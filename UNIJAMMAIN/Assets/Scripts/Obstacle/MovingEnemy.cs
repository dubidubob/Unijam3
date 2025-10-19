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
    public SpriteRenderer monsterImg;
    public GameObject dyingEffectObject;
    private Vector3 origin;
    private bool isResizeable = false;
    private Vector2 sizeDiffRate;
    public bool isKnockbacked=false;
    private Coroutine _hidingCoroutine;

    // yejun
    private Define.MonsterType _monsterType;


    private float movingDistanceTmp;
    private int attackValue = 10;
    private Sprite orginSprite;

    private float backwardDuration, knockbackDistance;
    private float backwardRate = 0.125f; // 기존 0.125f
    private bool isDead = false;
    Define.MonsterType type;
    private void OnEnable()
    {
        _elapsedTime = 0f;
        knockback = new Knockback();
        isKnockbacked = false;
        isDead = false;


        // ================== 추가된 초기화 코드 ==================
        // 몬스터 스프라이트의 알파값(투명도)을 원래대로 되돌립니다.
        // Hiding 등으로 투명해진 상태로 풀에 반납되었을 수 있기 때문입니다.
        if (monsterImg != null)
        {
            Color originalColor = monsterImg.color;
            monsterImg.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        // 사망 이펙트 오브젝트의 크기를 0으로 확실하게 초기화합니다.
        // 이것이 "몹이 등장할 때 이펙트가 커져있는 문제"의 핵심 해결책입니다.
        if (dyingEffectObject != null)
        {
            dyingEffectObject.transform.localScale = Vector3.zero;
        }
        // =======================================================
    }

    private void Start()
    {
        if (monsterImg != null)
        {
            origin = monsterImg.transform.localScale;
        }
        GarbageClean();
        if(enemyType == GamePlayDefine.WASDType.W || enemyType == GamePlayDefine.WASDType.S)
            isResizeable = true;
    }

    public void SetDead()
    {
        // 진행 중이던 다른 모든 트윈과 코루틴을 확실히 중지시킵니다.
        KillingDO();
        if (_hidingCoroutine != null)
        {
            StopCoroutine(_hidingCoroutine);
            _hidingCoroutine = null;
        }

        // 사망 처리 코루틴을 시작합니다.
        StartCoroutine(ProcessDeath());
    }
    private void KillingDO()
    {
        DOTween.Kill(transform, "FIFO");
        DOTween.Kill(transform, "Speeding");
    }

    public void SetVariance(float distance, MonsterData monster, Vector2 sizeDiffRate, Vector3 playerPos, GamePlayDefine.WASDType wasdType,Define.MonsterType monsterType)
    {
        // yejun
        this._monsterType = monsterType; // 멤버 변수에 몬스터 타입 저장

        movingDistanceTmp = distance;
        this.playerPos = playerPos;
        enemyType = wasdType;
        movingDuration = (float)IngameData.BeatInterval*monster.moveBeat;
        this.sizeDiffRate = sizeDiffRate;
        backwardDuration = movingDuration * backwardRate;
        knockbackDistance = distance * 0.125f;
        speed = distance / this.movingDuration;
        type = monsterType;
        SettingSprite(type);

        // yejun 몹들의 꼬리부분이 좀 어색해져서, 아래부분은 일단 삭제

        if (wasdType == GamePlayDefine.WASDType.A || wasdType == GamePlayDefine.WASDType.W)
        {
            // A 또는 W 타입일 경우, 스프라이트를 좌우로 뒤집습니다.
            monsterImg.flipX = true;
        }
        else
        {
            // S 또는 D 타입일 경우, 원래 방향으로 설정합니다.
            monsterImg.flipX = false;
        }

        SetKnockback(monsterType == Define.MonsterType.Knockback,monsterType);
        SetHiding(monsterType == Define.MonsterType.WASDHiding, monsterType);
        SetSpeeding(monsterType == Define.MonsterType.WASDDash, monsterType);
        SetFIFO(monsterType == Define.MonsterType.WASDFIFO, monsterType);


    }
    public void SetKnockback(bool isTrue,Define.MonsterType monsterType)
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

      
        if (isTrue)
        {
            Debug.Log("SetknockBack 출력");
            SettingSprite(monsterType);
        }
        else 
        {

        }
        knockback.OnKnockback(isTrue);
    }

    public void SetSpeeding(bool isTrue,Define.MonsterType monsterType)
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (isTrue)
        {
            Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;

            transform.DOMove(targetPos, movingDuration).SetEase(Ease.InQuint).SetId("Speeding");
            SettingSprite(monsterType);
        }

    }

    public void SetFIFO(bool isTrue,Define.MonsterType monsterType)
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (isTrue)
        {
            Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;
            transform.DOMove(targetPos, movingDuration).SetEase(Ease.InOutCirc).SetId("FIFO");
            SettingSprite(monsterType);
        }

    }
    public void SetHiding(bool isTrue,Define.MonsterType monsterType)
        {

        if (isTrue)
        {
            SettingSprite(monsterType);
            // 숨기기 코루틴 시작
            if (_hidingCoroutine == null)
            {
                _hidingCoroutine = StartCoroutine(HidingAnimation(monsterImg));
            }
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
        AttackedAnimation();

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
            _elapsedTime += Time.fixedDeltaTime;
        }
        Move();
    }

    private void Move()
    {
        if (isKnockbackActive) return;
        if (isDead) return;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerPos, speed * Time.fixedDeltaTime);
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

            // yejun (첫 피격 몬스터 효과음 들리지 않던 문제 해결)

            // [수정] 몬스터가 dangerLine에 닿으면 무조건 플레이어는 공격받고, 해당 몬스터는 죽습니다.
            Managers.Game.PlayerAttacked(attackValue);
            SetDead();
            // 그 후에, 공격 가능 리스트에 다른 몬스터가 있었다면 그것도 놓친 것이므로 제거합니다.
            if (Managers.Game.attacks[enemyType].Count > 0)
            {
                Managers.Game.attacks[enemyType].Dequeue();
            }
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


    void AttackedAnimation()    
    {
        // yejun
        Sprite attackedSprite = Managers.Game.monster.GetAttackedSprite(this._monsterType);


        // TODO: 몬스터 타입에 맞는 피격 스프라이트를 가져오도록 수정해야 할 수 있습니다.
        //Sprite attackedSprite = Managers.Game.monster.GetAttackedSprite(Define.MonsterType.WASDDash);
        //orginSprite = monsterImg.sprite;

        if (attackedSprite != null)
        {
            monsterImg.sprite = attackedSprite;
        }
    }


    #region Animation



    private IEnumerator ProcessDeath()
    {
        // 1. 사망 이펙트 애니메이션 준비
        Sprite dyingSprite = Managers.Game.monster.DyingEffectSprite();
        Transform effectTransform = dyingEffectObject.GetComponent<Transform>();
        SpriteRenderer effectRenderer = dyingEffectObject.GetComponent<SpriteRenderer>();
        isDead = true;

        effectRenderer.sprite = dyingSprite;

        // 2. DOTween 애니메이션 실행
        //    체인을 연결하여 순차적으로 실행하고, 이 트윈 자체를 변수에 저장합니다.
        Tween dyingTween = effectTransform.DOScale(1.5f, 0.1f)
                                          .OnComplete(() => effectTransform.DOScale(0, 0.1f));

        // 3. 위에서 만든 DOTween 애니메이션이 끝날 때까지 기다립니다.
        //    WaitForSeconds(0.9f) 같은 고정된 시간이 아니라 실제 애니메이션 길이에 맞춰 기다립니다.
        yield return dyingTween.WaitForCompletion();

        // 4. 애니메이션이 끝나면 오브젝트를 풀에 반납합니다.
        //    스프라이트 원상복구 등은 OnEnable에서 처리하므로 여기선 필요 없습니다.
        monsterImg.sprite = orginSprite;
        Poolable poolable = GetComponent<Poolable>();
        Managers.Pool.Push(poolable);
    }
    #endregion

    #region Tool
    IEnumerator GarbageClean()
    {
        yield return new WaitForSeconds(30f);
        Debug.Log("GarbageClean");
        SetDead();
        
    }
    private void SettingSprite(Define.MonsterType monsterType)
    {
        monsterImg.sprite = Managers.Game.monster.GetSprite(monsterType);
        orginSprite = monsterImg.sprite;
        monsterImg.color = Managers.Game.monster.GetColor(monsterType);
        // Debug.Log(monsterImg.sprite);
    }
    #endregion

}
