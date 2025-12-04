using System;
using System.Collections; // UniTask 사용 시 거의 안 쓰지만 호환성 위해 남김
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks; // UniTask 필수
using System.Threading;      // CancellationToken 필수

public class Knockback
{
    bool isEnabled;
    int hp;

    public void OnKnockback(bool isOn)
    {
        isEnabled = isOn;
        hp = 2; // 넉백 가능 횟수
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
    public bool isKnockbacked = false;

    // UniTask 제어용 토큰 소스
    private CancellationTokenSource _cts;

    // yejun
    private Define.MonsterType _monsterType;

    private float movingDistanceTmp;
    private int attackValue = 10;
    private Sprite orginSprite;

    private float backwardDuration, knockbackDistance;
    private float backwardRate = 0.125f;
    private bool isDead = false;
    Define.MonsterType type;

    private void Awake()
    {
        if (monsterImg != null)
        {
            origin = monsterImg.transform.localScale;
        }
    }

    private void OnEnable()
    {
        // 이전 작업 취소 및 새 토큰 생성
        RefreshCancellationToken();

        _elapsedTime = 0f;
        knockback = new Knockback();
        isKnockbacked = false;
        isDead = false;
        isKnockbackActive = false; // 넉백 상태 초기화 필수

        if (monsterImg != null)
        {
            Color originalColor = monsterImg.color;
            monsterImg.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            monsterImg.transform.localScale = origin; // 크기 초기화 안전장치
        }

        if (dyingEffectObject != null)
        {
            dyingEffectObject.transform.localScale = Vector3.zero;
        }

        // 안전장치: 일정 시간 후 자동 사망 (Garbage Collection)
        GarbageClean(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        // 비활성화 시 실행 중인 모든 비동기 작업 취소
        _cts?.Cancel();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    // 토큰 갱신 헬퍼 함수
    private void RefreshCancellationToken()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        _cts = new CancellationTokenSource();
    }

    private void Start()
    {
        // OnEnable에서 초기화하므로 Start는 주로 의존성 체크
        if (enemyType == GamePlayDefine.WASDType.W || enemyType == GamePlayDefine.WASDType.S)
            isResizeable = true;
    }

    public void SetDead()
    {
        if (isDead) return; // 중복 사망 방지

        KillingDO();
        // 사망 시 기존 움직임/숨기기 등은 취소하고 사망 연출만 실행
        RefreshCancellationToken();

        // 사망 처리 (UniTask)
        ProcessDeath(_cts.Token).Forget();
    }

    private void KillingDO()
    {
        DOTween.Kill(transform, "FIFO");
        DOTween.Kill(transform, "Speeding");
        // Hiding 트윈도 킬
        monsterImg.DOKill();
    }

    public void SetVariance(float distance, MonsterData monster, Vector2 sizeDiffRate, Vector3 playerPos, GamePlayDefine.WASDType wasdType, Define.MonsterType monsterType,float timeOffset)
    {
        this._monsterType = monsterType;
        movingDistanceTmp = distance;
        this.playerPos = playerPos;
        enemyType = wasdType;
        movingDuration = (float)IngameData.BeatInterval * monster.moveBeat;
        this.sizeDiffRate = sizeDiffRate;

        backwardDuration = movingDuration * backwardRate;
        knockbackDistance = distance * 0.125f;
        speed = distance / this.movingDuration;
        type = monsterType;

        SettingSprite(type);

        if (wasdType == GamePlayDefine.WASDType.A || wasdType == GamePlayDefine.WASDType.W)
            monsterImg.flipX = true;
        else
            monsterImg.flipX = false;

        if (timeOffset > 0)
        {
            // 1. 이동할 방향 계산
            Vector3 direction = (playerPos - transform.position).normalized;

            // 2. 건너뛰어야 할 거리 계산 (속도 * 시간)
            float skippedDistance = speed * timeOffset;

            // 3. 위치 강제 이동
            transform.position += direction * skippedDistance;

            // 4. 경과 시간도 미리 더해줌 (애니메이션, 크기 조절 등 싱크 맞추기)
            _elapsedTime += timeOffset;
        }

        SetKnockback(monsterType == Define.MonsterType.Knockback, monsterType);
        SetHiding(monsterType == Define.MonsterType.WASDHiding, monsterType);
        SetSpeeding(monsterType == Define.MonsterType.WASDDash, monsterType);
        SetFIFO(monsterType == Define.MonsterType.WASDFIFO, monsterType);
    }

    public void SetKnockback(bool isTrue, Define.MonsterType monsterType)
    {
        if (isTrue) SettingSprite(monsterType);
        knockback.OnKnockback(isTrue);
    }

    public void SetSpeeding(bool isTrue, Define.MonsterType monsterType)
    {
        if (isTrue)
        {
            Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;
            // DOTween도 Link를 걸어 게임오브젝트 파괴 시 안전하게
            transform.DOMove(targetPos, movingDuration).SetEase(Ease.InQuint).SetId("Speeding").SetLink(gameObject);
            SettingSprite(monsterType);
        }
    }

    public void SetFIFO(bool isTrue, Define.MonsterType monsterType)
    {
        if (isTrue)
        {
            Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;
            transform.DOMove(targetPos, movingDuration).SetEase(Ease.InOutCirc).SetId("FIFO").SetLink(gameObject);
            SettingSprite(monsterType);
        }
    }

    public void SetHiding(bool isTrue, Define.MonsterType monsterType)
    {
        if (isTrue)
        {
            SettingSprite(monsterType);
            HidingAnimation(monsterImg, _cts.Token).Forget();
        }
    }

    // async UniTask로 변경
    private async UniTaskVoid HidingAnimation(SpriteRenderer _spriteRenderer, CancellationToken token)
    {
        // 1. 대기
        await UniTask.Delay(TimeSpan.FromSeconds(movingDuration * 0.1f), cancellationToken: token);

        // 2. Fade Out
        float fadeOutDuration = movingDuration * 0.3f;
        await _spriteRenderer.DOFade(0f, fadeOutDuration).SetLink(gameObject).ToUniTask(cancellationToken: token);

        // 3. 투명 유지
        await UniTask.Delay(TimeSpan.FromSeconds(movingDuration * 0.2f), cancellationToken: token);

        // 4. Fade In
        float fadeInDuration = movingDuration * 0.3f;
        await _spriteRenderer.DOFade(1f, fadeInDuration).SetLink(gameObject).ToUniTask(cancellationToken: token);
    }

    public bool CheckCanDead()
    {
        AttackedAnimation();

        if (knockback.CheckKnockback())
        {
            if (this.isActiveAndEnabled)
            {
                // 기존 넉백 태스크가 있다면 취소할 필요 없이 플래그 제어 혹은 중복 실행 허용 여부 결정
                // 여기서는 단순히 실행
                ExecuteKnockback(_cts.Token).Forget();
            }
            isKnockbacked = true;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool isKnockbackActive = false;

    // async UniTask로 변경
    private async UniTaskVoid ExecuteKnockback(CancellationToken token)
    {
        isKnockbackActive = true;
        float elapsedTime = 0f;
        Vector3 knockbackDirection = (transform.position - playerPos).normalized;

        while (elapsedTime < backwardDuration)
        {
            // 일시 정지 처리 (WaitUntil 사용)
            if (IngameData.Pause)
            {
                await UniTask.WaitUntil(() => !IngameData.Pause, cancellationToken: token);
            }

            // 토큰 취소 확인
            if (token.IsCancellationRequested) return;

            float t = elapsedTime / backwardDuration;
            // Lerp 이용한 감속 이동
            float knockbackMovement = Mathf.Lerp(knockbackDistance, 0, (t * t));

            // [중요] FixedUpdate가 아닌 Update 타이밍에 맞게 이동
            transform.position += knockbackDirection * knockbackMovement * Time.deltaTime * 2.0f;

            elapsedTime += Time.deltaTime;

            // 다음 프레임까지 대기
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }

        isKnockbackActive = false;
    }

    // [수정] FixedUpdate -> Update 변경
    // 리듬게임이나 2D 이동에서 Rigidbody 물리 충돌(AddForce 등)을 쓰지 않는다면 Update가 훨씬 부드럽습니다.
    private void Update()
    {
        if (IngameData.Pause) return;
        if (isDead) return;

        // Resize 로직 (FixedDeltaTime -> DeltaTime)
        if (_elapsedTime <= movingDuration && isResizeable)
        {
            _elapsedTime += Time.deltaTime; // 수정
            PerspectiveResize(_elapsedTime);
        }

        Move();
    }

    private void Move()
    {
        // 넉백 중일 때는 일반 이동 로직을 태우지 않음 (위치가 튀는 현상 방지)
        if (isKnockbackActive) return;

        // 단순 Transform 이동이므로 Update에서 실행
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerPos, speed * Time.deltaTime);
        transform.position = newPosition;
    }

    private void PerspectiveResize(float currentElapsedTime)
    {
        float t = currentElapsedTime / movingDuration;
        t = Mathf.Clamp01(t);

        if (enemyType == GamePlayDefine.WASDType.W)
        {
            monsterImg.transform.localScale = Vector3.Lerp(origin * sizeDiffRate.x, origin, t);
        }
        else if (enemyType == GamePlayDefine.WASDType.S)
        {
            monsterImg.transform.localScale = Vector3.Lerp(origin * sizeDiffRate.y, origin, t);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("detectArea")) // CompareTag가 가비지가 덜 나옴
        {
            Managers.Game.AddAttackableEnemy(enemyType, this.gameObject);
        }
        else if (collision.CompareTag("dangerLine"))
        {
            Managers.Game.PlayerAttacked(attackValue);
            SetDead();

            if (Managers.Game.attacks[enemyType].Count > 0)
            {
                Managers.Game.attacks[enemyType].Dequeue();
            }
        }
    }

    private Vector3 CalculateNormalVector()
    {
        switch (enemyType)
        {
            case GamePlayDefine.WASDType.A: return Vector3.right;
            case GamePlayDefine.WASDType.D: return Vector3.left;
            case GamePlayDefine.WASDType.W: return Vector3.down;
            case GamePlayDefine.WASDType.S: return Vector3.up;
            default:
                Debug.Log("CalculateVector실패");
                return Vector3.forward;
        }
    }


    void AttackedAnimation()
    {
        Sprite attackedSprite = Managers.Game.monster.GetAttackedSprite(this._monsterType);
        if (attackedSprite != null)
        {
            monsterImg.sprite = attackedSprite;
        }
    }


    #region Animation

    private async UniTaskVoid ProcessDeath(CancellationToken token)
    {
        isDead = true;

        Sprite dyingSprite = Managers.Game.monster.DyingEffectSprite();
        Transform effectTransform = dyingEffectObject.transform;
        SpriteRenderer effectRenderer = dyingEffectObject.GetComponent<SpriteRenderer>();

        effectRenderer.sprite = dyingSprite;

        // DOTween Sequence 대신 비동기 await으로 처리 (가독성)
        // 1. 커지기
        await effectTransform.DOScale(1.5f, 0.1f)
                             .SetLink(dyingEffectObject) // 객체 파괴시 트윈 자동 중단
                             .ToUniTask(cancellationToken: token);

        // 2. 작아지기
        await effectTransform.DOScale(0f, 0.1f)
                             .SetLink(dyingEffectObject)
                             .ToUniTask(cancellationToken: token);

        // 복구 및 반납
        monsterImg.sprite = orginSprite;

        // 풀링 반납
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null)
            Managers.Pool.Push(poolable);
    }
    #endregion

    #region Tool

    async UniTaskVoid GarbageClean(CancellationToken token)
    {
        // 30초 대기 (Delay는 기본적으로 Time.timeScale의 영향을 받음)
        await UniTask.Delay(TimeSpan.FromSeconds(30f), cancellationToken: token);

        Debug.Log("GarbageClean");
        if (this.isActiveAndEnabled) SetDead();
    }

    private void SettingSprite(Define.MonsterType monsterType)
    {
        monsterImg.sprite = Managers.Game.monster.GetSprite(monsterType);
        orginSprite = monsterImg.sprite;
        monsterImg.color = Managers.Game.monster.GetColor(monsterType);
    }
    #endregion
}