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

        if (monsterImg != null) // 몬스터의 색깔 조정
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

        // Steam 업적을위해 InGameData에 처치한 수 저장
        IngameData._defeatEnemyCount++;

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

        ApplyMonsterTypeBehavior(monsterType);

    }

    private void ApplyMonsterTypeBehavior(Define.MonsterType monsterType)
    {
        // 기본적으로 넉백 기능은 꺼둠 (필요한 경우만 켬)
        knockback.OnKnockback(false);

        switch (monsterType)
        {
            case Define.MonsterType.Knockback:
                EnableKnockback();
                break;

            case Define.MonsterType.WASDHiding:
                EnableHiding();
                break;

            case Define.MonsterType.WASDDash:
                EnableSpeeding();
                break;

            case Define.MonsterType.WASDFIFO:
                EnableFIFO();
                break;
            case Define.MonsterType.WASD_CristMas_Dash:
                EnableSpeeding();
                break;

            case Define.MonsterType.WASD_EDM_Normal:
                EnableZigZag(_cts.Token).Forget();
                break;

            case Define.MonsterType.WASD_EDM_Dash:
                EnableSpin();
                break;
            case Define.MonsterType.WASD_STOPANDGO:
                EnableStopAndGo(_cts.Token).Forget();
                break;
            case Define.MonsterType.WASD_Blink:
                EnableBlink(_cts.Token).Forget();
                break;
            case Define.MonsterType.WASD_Spiral:
                EnableSpiral(_cts.Token).Forget();
                break;
            case Define.MonsterType.WASD_SmoothDash:
                EnableSmoothDash(_cts.Token).Forget();
                break;
            // 일반 몬스터나 기타 타입은 추가 행동 없음
            default:
                break;
        }
    }

    #region Specific Behaviors 

    // 파라미터 제거: 이미 멤버변수 _monsterType과 세팅된 값들을 사용
    private void EnableKnockback()
    {
        // 넉백 활성화
        knockback.OnKnockback(true);
        // 스프라이트는 이미 SetVariance -> SettingSprite에서 설정됨
    }

    private void EnableSpeeding()
    {
        Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;
        transform.DOMove(targetPos, movingDuration)
                 .SetEase(Ease.InQuint)
                 .SetId("Speeding")
                 .SetLink(gameObject);
    }
    private async UniTaskVoid EnableSmoothDash(CancellationToken token)
    {
        Vector3 startPos = transform.position;

        // 1. [급접근 Phase]
        // 목표: 40% 지점까지 순식간에 도달
        // 거리: 40% (0.4f)
        // 시간: 15% (0.15f) -> 아주 짧은 시간에 이동하므로 속도가 빠름
        float fastPhaseDuration = movingDuration * 0.15f;
        Vector3 midPos = Vector3.Lerp(startPos, playerPos, 0.4f);

        // Ease.OutSine: 빠르지만 연결 부위에서 튀지 않게 살짝 감속 시작
        await transform.DOMove(midPos, fastPhaseDuration)
                       .SetEase(Ease.OutSine)
                       .SetLink(gameObject)
                       .ToUniTask(cancellationToken: token);

        // 2. [스무스 돌격 Phase]
        // 목표: 나머지 거리를 부드럽게 관성으로 밀고 들어감
        // 거리: 남은 60%
        // 시간: 남은 85% (0.85f) -> 긴 시간 동안 이동하므로 느려짐
        float smoothPhaseDuration = movingDuration * 0.85f;

        // Ease.OutCubic: Quad보다 조금 더 길게 늘어지는 느낌 (미끄러짐 강조)
        await transform.DOMove(playerPos, smoothPhaseDuration)
                       .SetEase(Ease.OutCubic)
                       .SetLink(gameObject)
                       .ToUniTask(cancellationToken: token);
    }
    private void EnableFIFO()
    {
        Vector3 targetPos = transform.position + CalculateNormalVector() * movingDistanceTmp;
        transform.DOMove(targetPos, movingDuration)
                 .SetEase(Ease.InOutCirc)
                 .SetId("FIFO")
                 .SetLink(gameObject);
    }

    private void EnableHiding()
    {
        HidingAnimation(monsterImg, _cts.Token).Forget();
    }

    private async UniTaskVoid EnableStopAndGo(CancellationToken token)
    {
        // 전체 거리를 2분할
        Vector3 startPos = transform.position;
        Vector3 midPos = Vector3.Lerp(startPos, playerPos, 0.4f); // 40% 지점까지 이동

        float firstMoveTime = movingDuration * 0.3f; // 전체 시간의 30% 동안 40% 이동 (빠름)
        float stopTime = movingDuration * 0.2f;      // 20% 시간 동안 정지 (엇박 유도)
        float lastMoveTime = movingDuration * 0.5f;  // 나머지 시간 동안 돌진

        // 1. 1차 급접근
        await transform.DOMove(midPos, firstMoveTime).SetEase(Ease.OutQuad)
                       .SetLink(gameObject).ToUniTask(cancellationToken: token);

        // 2. 정지 (두근거림 연출 추가 가능)
        await transform.DOPunchScale(Vector3.one * 0.2f, stopTime, 2, 1)
                       .SetLink(gameObject).ToUniTask(cancellationToken: token);

        // 3. 2차 돌진 (플레이어에게 꽂힘)
        await transform.DOMove(playerPos, lastMoveTime).SetEase(Ease.InExpo)
                       .SetLink(gameObject).ToUniTask(cancellationToken: token);
    }

    private async UniTaskVoid EnableZigZag(CancellationToken token)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = playerPos; // Start에서 계산된 타겟 위치

        // 진행 방향의 오른쪽(수직) 벡터 계산
        Vector3 forward = (endPos - startPos).normalized;
        Vector3 right = Vector3.Cross(forward, Vector3.forward).normalized;

        float frequency = 6f; // 흔들리는 빈도
        float magnitude = 0.5f; // 흔들리는 폭

        while (elapsedTime < movingDuration)
        {
            if (token.IsCancellationRequested) return;
            if (IngameData.Pause) { await UniTask.Yield(token); continue; }

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / movingDuration;

            // 1. 선형 이동 (기본 이동)
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);

            // 2. 수직 흔들림 (Sin 파형)
            // 끝으로 갈수록(t가 1에 가까울수록) 흔들림이 줄어들어 정확히 타겟에 꽂히게(Mathf.Lerp(magnitude, 0, t)) 처리
            float wave = Mathf.Sin(t * frequency * Mathf.PI) * Mathf.Lerp(magnitude, 0f, t);

            transform.position = currentPos + (right * wave);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }
    }

    private void EnableSpin()
    {
        // 이동은 기본 Move() 혹은 EnableSpeeding() 등을 사용한다고 가정
        // 별도로 회전만 추가

        // 1초에 360도 회전, 무한 반복
        monsterImg.transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear)
            .SetLink(gameObject);

        // 참고: 이동 로직은 별도로 실행되어야 함 (예: default의 Move() 사용)
    }

   
    private async UniTaskVoid EnableBlink(CancellationToken token)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        // 전체 거리의 30% 지점에서 사라졌다가 70% 지점에서 나타남
        bool hasBlinked = false;

        while (elapsedTime < movingDuration)
        {
            if (token.IsCancellationRequested) return;
            if (IngameData.Pause) { await UniTask.Yield(token); continue; }

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / movingDuration;

            // 30% ~ 70% 구간에서 점멸 처리
            if (t > 0.3f && t < 0.7f)
            {
                if (!hasBlinked)
                {
                    hasBlinked = true;
                    monsterImg.color = new Color(monsterImg.color.r, monsterImg.color.g, monsterImg.color.b, 0.2f); // 반투명 혹은 투명
                                                                                                                    // 필요하다면 Collider 꺼서 무적 판정 줄 수도 있음
                }

                // 이 구간에서는 이동을 멈추거나, 혹은 아주 빠르게 다음 지점으로 건너뛰는 연출 가능
                // 여기서는 단순히 모습만 감추고 위치는 선형 이동 (예측 가능하게 하려면)
                // 예측 불가능하게 하려면 여기서 transform.position 업데이트를 안 하다가 0.7f에 텔레포트
            }
            else if (t >= 0.7f && hasBlinked)
            {
                hasBlinked = false; // 플래그 리셋하여 한 번만 실행되게
                monsterImg.color = new Color(monsterImg.color.r, monsterImg.color.g, monsterImg.color.b, 1f); // 원복

                // '뿅' 하고 나타나는 효과음이나 파티클 추가 가능
            }

            // 기본 이동 (Blink 구간에 위치를 건너뛰고 싶다면 로직 추가 필요)
            transform.position = Vector3.Lerp(startPos, playerPos, t);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }
    }

    private async UniTaskVoid EnableSpiral(CancellationToken token)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        // 진행 방향 벡터 계산
        Vector3 forward = (playerPos - startPos).normalized;
        // 진행 방향의 수직 벡터 (오른쪽, 위쪽) - 2D 기준 임의의 축 생성
        Vector3 right = Vector3.Cross(forward, Vector3.forward).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized; // 사실상 forward와 직각인 또 다른 축

        // 나선 회전 반경과 속도
        float radius = 1f;
        float frequency = 5f; // 회전 속도

        while (elapsedTime < movingDuration)
        {
            if (token.IsCancellationRequested) return;
            if (IngameData.Pause) { await UniTask.Yield(token); continue; }

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / movingDuration;

            // 1. 중심축 이동 (선형)
            Vector3 centerPos = Vector3.Lerp(startPos, playerPos, t);

            // 2. 나선 회전 (플레이어에게 가까워질수록 반지름 0으로 수렴)
            float currentRadius = Mathf.Lerp(radius, 0f, t);
            float angle = t * frequency * Mathf.PI * 2f;

            // 2D 평면상에서의 회전 (X, Y 축 기준 변형)
            // 진행 방향이 어디냐에 따라 축 보정이 필요하지만, 간단히 right 벡터를 기준으로 회전
            Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * currentRadius;

            // *단순 2D 게임(Z축 고정)이라면 아래 방식이 더 직관적일 수 있음*
            // transform.position = centerPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * currentRadius;

            // 벡터 기반 적용
            transform.position = centerPos + offset;

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }
    }

    private async UniTaskVoid EnableBerserk(CancellationToken token)
    {
        // 색상을 빨갛게 예열하는 연출 추가
        monsterImg.DOColor(Color.red, movingDuration * 0.5f).SetLink(gameObject);

        Vector3 startPos = transform.position;

        // 커스텀 Easing: 처음 60% 시간 동안 20%만 이동, 나머지 40% 시간에 80% 이동
        await transform.DOMove(playerPos, movingDuration)
            .SetEase(Ease.InExpo) // 혹은 Custom Curve 사용
            .SetLink(gameObject)
            .ToUniTask(cancellationToken: token);

        // 참고: 만약 직접 제어하고 싶다면 아래처럼 Lerp의 t값을 조작
        /*
        float elapsedTime = 0f;
        while (elapsedTime < movingDuration)
        {
           // ... (Pause 체크 등) ...
           float t = elapsedTime / movingDuration;
           // t를 세제곱하면 초반엔 아주 작고 후반에 급격히 커짐
           float curvedT = t * t * t * t; 
           transform.position = Vector3.Lerp(startPos, playerPos, curvedT);
           // ...
        }
        */
    }

    #endregion
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

        // 커스텀 이동 로직을 가진 애들은 기본 이동(Move)을 막음
        if (IsCustomMovementType(_monsterType)) return;

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

        if (IngameData.ChapterIdx == 10) // Chapter10에서는 색깔이 다름
        {
            monsterImg.color = BackGroundEffect_10.CurrentClubColor;
        }
       
    }
    #endregion

    #region tool
    private bool IsCustomMovementType(Define.MonsterType type)
    {
        return type == Define.MonsterType.WASD_EDM_Normal ||
               type == Define.MonsterType.WASD_EDM_Normal 
               ;
    }

    #endregion
}