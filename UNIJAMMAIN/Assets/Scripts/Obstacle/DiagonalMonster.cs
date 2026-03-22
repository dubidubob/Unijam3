using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask 네임스페이스 추가
using System.Threading; // CancellationToken을 위해 추가

[RequireComponent(typeof(SpriteRenderer))]
public class DiagonalMonster : MonoBehaviour
{
    [SerializeField] GamePlayDefine.DiagonalType diagonalT;
    [SerializeField] Transform targetPos;
    [SerializeField] Sprite outline;
    [SerializeField] Sprite attackedSprite;
    [SerializeField] SpriteRenderer MonsterSprite;
    [SerializeField] SpriteRenderer attackedEffectSpriteRenderer;
    public GamePlayDefine.DiagonalType DiagonalT => diagonalT;

    private bool _isDying = false;
    private int _jumpCnt = 4;

    private Vector3 _originPos;
    private Sprite _originSprite;
    private SpriteRenderer _objectRenderer; // 페이드 아웃을 적용할 객체의 렌더러

    private float _duration;
    private Vector3 _stride;
    private float _moveBeat;
    private DG.Tweening.Sequence jumpSequence;

    private int attackValue = 20;
    private int healingValue = 2;

    // UniTask 취소 토큰 소스
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _objectRenderer = GetComponent<SpriteRenderer>();
        _originSprite = _objectRenderer.sprite;
        _originPos = transform.position;

        // 이벤트 등록/해제는 OnEnable/OnDisable에서 관리하는 것이 일반적이지만,
        // 기존 로직을 유지하여 Awake/OnDestroy 패턴(혹은 여기서처럼 Awake에서 한번)을 따릅니다.
        // 다만 PauseManager가 정적(static) 이벤트라면 OnDestroy에서 해제해주는 것이 안전합니다.
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;
    }

    private void OnDestroy()
    {
        PauseManager.IsPaused -= PauseForWhile;

        _cts?.Dispose();
    }

    private void OnEnable()
    {
        if (_isDying)
        {
            return;
        }

        // 새로운 토큰 생성
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // 이전에 남아있을 수 있는 타격 이펙트의 모든 애니메이션을 중지하고,
        attackedEffectSpriteRenderer.DOKill();
        // 즉시 투명하게(alpha=0) 만들어 보이지 않게 초기화합니다.
        attackedEffectSpriteRenderer.color = new Color(1, 1, 1, 0);

        _duration = (float)IngameData.BeatInterval;
        _stride = (targetPos.position - _originPos) / _jumpCnt;
        Managers.Sound.Play("SFX/Enemy/Diagonal_V4", Define.Sound.SFX, 1f, 6f);
        ChangeToOriginal();
        Move();
    }

    private void OnDisable()
    {
        // 진행 중인 모든 UniTask 취소
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        jumpSequence?.Kill();
        transform.DOKill();

        // PauseManager 해제는 OnDestroy나 OnDisable 한 곳에서 관리하는 것이 좋으나 원본 유지
        // PauseManager.IsPaused -= PauseForWhile; (Awake에서 등록했으므로 짝이 안 맞을 수 있음, 주의 필요)

        var child = GetComponentInChildren<TutorialDiagonalKeyUI>();
        if (child != null)
        {
            child.OnDisableCalledByParent();
        }
    }

    //TODO : tmp!
    public void SetMovebeat(float moveBeat)
    {
        _moveBeat = (moveBeat / 2);
    }

    private void PauseForWhile(bool isStop)
    {
        if (jumpSequence == null) return;

        if (isStop)
            jumpSequence.Pause();
        else
            jumpSequence.Play();
    }

    private void Move()
    {
        jumpSequence = DOTween.Sequence();

        // GarbageClean을 UniTask로 실행 (Fire and Forget)
        GarbageCleanAsync().Forget();

        // 점프와 대기를 번갈아 가며 설정
        for (int i = 0; i < _jumpCnt; i++)
        {
            Vector3 target = _originPos + _stride * (i + 1);
            jumpSequence.Append(transform.DOJump(
                target,   // 목표 지점
                0.5f,        // 점프 높이
                1,           // 점프 횟수 (한 번만 점프)
                _duration * _moveBeat
            ));

            // 쉬는 시간 (1박자)
            if (i < _jumpCnt - 1) // 마지막 점프는 대기 필요 없음
            {
                jumpSequence.AppendInterval(_duration * _moveBeat);
            }
            else
            {
                jumpSequence.AppendCallback(() =>
                {
                    DoFadeAsync().Forget(); // Async 메서드 호출
                });
            }
        }

        jumpSequence.OnComplete(() => {
            SetDead(false);
        });
    }

    // Invoke 대신 UniTask 사용
    private async UniTaskVoid DoFadeAsync()
    {
        // _cts가 null이면(이미 오브젝트가 꺼졌으면) 실행하지 않고 리턴
        if (_cts == null) return;

        _isDying = true;
        _objectRenderer.sprite = outline;

        float delay = 0.2f;

        // [수정] SuppressCancellationThrow()를 사용하여 취소 시 에러 로그 방지
        bool isCanceled = await UniTask.Delay((int)(delay * 1000), cancellationToken: _cts.Token).SuppressCancellationThrow();

        // [수정] 취소되었거나, 오브젝트가 파괴되었다면 이후 로직 실행 중단
        if (isCanceled || this == null) return;

        // 딜레이 (취소 토큰 적용)
        await UniTask.Delay((int)(delay * 1000), cancellationToken: _cts.Token);

        ChangeToOriginal();

        // DOTween 페이드 (Sequence가 아니라 별도 실행이므로 여기서 처리)
        _objectRenderer.material.DOFade(0, _duration - delay);
    }

    private void ChangeToOriginal()
    {
        // 여기 수정했어요! -> 렌더러가 파괴되지 않고 살아있을 때만 접근하도록 안전장치를 걸었어요.
        if (_objectRenderer != null)
            _objectRenderer.sprite = _originSprite;
    }

    public void SetDead(bool isAttackedByPlayer = true, bool isUnPool = false)
    {
        if (jumpSequence != null && jumpSequence.IsActive())
        {
            jumpSequence.Kill(false); // OnComplete 실행 안 함
            jumpSequence = null;
        }
        if (this != null && gameObject.activeInHierarchy)
            Managers.Sound.Play("SFX/Enemy/DiagonalSuccess_V4", Define.Sound.SFX, 1f, 2.5f);

        float waitForSeconds;

        if (!isAttackedByPlayer)
        {
            if (!isUnPool)// 풀로 언로드하는것은 데미지 안입히게 수정
            {
                Managers.Game.PlayerAttacked(attackValue);
            }
        
            waitForSeconds = 0f;
            DoFadeAsync().Forget();
        }
        else
        {
            if (_isDying) // player attacked, but late, this not count
                return;

            waitForSeconds = 0.22f;
            MonsterSprite.sprite = attackedSprite;
            attackedEffectSpriteRenderer.DOFade(1, 0);

            Managers.Game.ComboInc(healingValue);
        }

        // Coroutine 대신 UniTask 호출
        PoolOutGoAsync(waitForSeconds).Forget();
    }

    // IEnumerator -> async UniTaskVoid
    private async UniTaskVoid PoolOutGoAsync(float waitSeconds)
    {
        // 안전 장치: 토큰이 없으면 실행 중지
        if (_cts == null) return;

        // WaitForSeconds 대체
        if (waitSeconds > 0)
        {
            // [수정] 취소 에러 방지
            bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(waitSeconds), cancellationToken: _cts.Token).SuppressCancellationThrow();
            // [수정] 취소되거나 파괴되었으면 리턴
            if (isCanceled || this == null) return;
        }

        _isDying = false;

        // 여기 수정했어요! -> 각각의 컴포넌트가 파괴되지 않고 남아있는지 확인하고 초기화합니다.
        if (attackedEffectSpriteRenderer != null)
            attackedEffectSpriteRenderer.DOFade(1, 0);

        attackedEffectSpriteRenderer.DOFade(1, 0);
        transform.position = _originPos;
        _objectRenderer.color = new Color(_objectRenderer.color.r, _objectRenderer.color.g, _objectRenderer.color.b, 1);
        gameObject.SetActive(false);
        // [추가] 오브젝트가 완전히 꺼진 후 스포너의 가용 큐(대기열)에 반환
        var spawner = GetComponentInParent<DiagonalMonsterSpawner>();
        if (spawner != null)
        {
            spawner.RecycleMonsterIndex(diagonalT);
        }
    }

    // IEnumerator -> async UniTaskVoid
    private async UniTaskVoid GarbageCleanAsync()
    {
        if (_cts == null) return;

        // 여기 수정했어요! -> 15초를 기다리는 동안 몬스터가 파괴되었을 때 나는 에러를 막았습니다.
        bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(15f), cancellationToken: _cts.Token).SuppressCancellationThrow();

        // 여기 수정했어요! -> 파괴되었거나 취소되었으면 멈춥니다.
        if (isCanceled || this == null) return;

        // 여기 수정했어요! -> 혹시 jumpSequence가 비어있을 경우를 대비해 ?(널 체크)를 추가했어요.
        jumpSequence?.Kill();

        PoolOutGoAsync(0).Forget();
    }
}