using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;

public class MouseEnemy : MonoBehaviour
{
    public enum Dir
    {
        Left,
        Right
    }

    [SerializeField] public Dir dir = Dir.Left;

    private Image image;
    private Tweener blinkTweener;
    public float initialBlinkDuration = 0.8f; // 시작 블링킹 지속 시간
    public float minBlinkDuration = 0.05f;     // 블링킹 최소 지속 시간
    public float blinkSpeedIncrease = 0.1f;  // 매 사이클마다 지속 시간 감소량

    private float currentBlinkDuration;

    private int healingValue = 4;

    [Header("Visual Settings")]
    [SerializeField] private RectTransform _rectTransform; // UI 이동을 위해 필요
    [SerializeField] private RectTransform _waveTransform;
    [SerializeField] private Sprite _screamSprite;
    [SerializeField] private Sprite _originalSprite;
    private Vector3 _originalPos;


    private void Awake()
    {
        image = GetComponent<Image>();
        // 초기 색상 설정: 원하는 RGB 색상에 알파 1로 설정 (예: 흰색)
        image.color = new Color(1f, 0f, 0f, 1f);
        _rectTransform = GetComponent<RectTransform>();
        _originalPos = _rectTransform.localPosition;
        if (_waveTransform != null)
            _waveTransform.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;

        // 초기화: 위치 원복
        _rectTransform.localPosition = _originalPos;
        _rectTransform.localScale = Vector3.one;
        image.sprite = _originalSprite;
        image.color = Color.white;
    }

    // 부유상태시작
    public void PlayFloatAction()
    {
        // 둥둥 떠있는 느낌 (위아래 반복)
        _rectTransform.DOAnchorPosY(_originalPos.y + 50f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId("MouseFloat");

        // 블링킹우선 비활성화
        // BlinkingLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }
    // 강타 애니메이션
    public void PlaySlamAction(float duration, System.Action onImpact)
    {
        // 부유 트윈만 정확히 제거
        DOTween.Kill(_rectTransform);

        Sequence seq = DOTween.Sequence();
        float prepTime = duration * 0.3f;
        float slamTime = duration * 0.7f;

        // [준비 단계]
        image.sprite = _screamSprite;
        if (_waveTransform != null)
        {
            _waveTransform.gameObject.SetActive(true);
            _waveTransform.localScale = Vector3.zero;
            seq.Join(_waveTransform.DOScale(3f, prepTime).SetEase(Ease.OutQuad));
        }

        seq.Append(_rectTransform.DOScale(1.5f, prepTime).SetEase(Ease.OutQuad));
        seq.Join(_rectTransform.DOAnchorPosY(_originalPos.y + 70f, prepTime).SetEase(Ease.OutQuad));

        // [중간 콜백]
        seq.AppendCallback(() =>
        {
            image.sprite = _originalSprite;
            if (_waveTransform != null)
            {
                _waveTransform.gameObject.SetActive(false);
            }
        });

        // [타격 단계]
        seq.Append(_rectTransform.DOScale(1.0f, slamTime).SetEase(Ease.InBack));
        seq.Join(_rectTransform.DOAnchorPosY(-350f, slamTime).SetEase(Ease.InBack));
        seq.Join(image.DOColor(Color.red, slamTime));

        seq.OnComplete(() =>
        {
            Managers.Sound.Play("SFX/SlamSound");
            onImpact?.Invoke();

            // 자연스럽게 사라지기
            image.DOFade(0, 0.5f).OnComplete(() => {
                this.gameObject.SetActive(false);
            });
        });
    }

    private CancellationTokenSource _blinkCts; // 블링킹 취소용 토큰

    private async UniTask WaitForDisable()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        OnDisable();

    }

    /*
    // [변경점] 재귀 호출 -> async Loop 패턴으로 변경
    // 훨씬 직관적이며, 실행 중인 Tween을 관리하기 편합니다.
    private async UniTaskVoid BlinkingLoop(CancellationToken token)
    {
        _blinkCts = new CancellationTokenSource();
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _blinkCts.Token).Token;

        currentBlinkDuration = initialBlinkDuration;

        try
        {
            while (!linkedToken.IsCancellationRequested)
            {
                // 트윈을 생성하고 변수에 할당해야 일시정지(Pause)가 가능합니다.
                blinkTweener = image.DOFade(0f, currentBlinkDuration)
                                   .SetLoops(2, LoopType.Yoyo)
                                   .SetEase(Ease.InOutSine)
                                   .SetLink(gameObject);

                await blinkTweener.ToUniTask(cancellationToken: linkedToken);

                currentBlinkDuration = Mathf.Max(currentBlinkDuration - blinkSpeedIncrease, minBlinkDuration);
            }
        }
        catch (System.OperationCanceledException)
        {
            if (image != null) image.color = Color.white;
        }
    }
    */
    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.LeftShift) && dir == Dir.Left) 
            //(Input.GetMouseButtonDown(0) && dir == Dir.Left)
        {
            Managers.Sound.Play("SFX/Mouse_Monster_Death_SFX");
            StopBlinking();
            Managers.Game.ComboInc(healingValue);
            gameObject.SetActive(false);
        }

        if  (Input.GetKeyDown(KeyCode.RightShift) && dir == Dir.Right)
            //(Input.GetMouseButtonDown(1) && dir == Dir.Right)
        {
            Managers.Sound.Play("SFX/Mouse_Monster_Death_SFX");
            StopBlinking();
            Managers.Game.ComboInc(healingValue);
            gameObject.SetActive(false);
        }
        */
    }
    /*
    private void StopBlinking()
    {
        if (blinkTweener != null && blinkTweener.IsActive())
        {
            blinkTweener.Kill(); // 현재 Tween 중단
           
        }

        // 알파를 1로 초기화 (완전히 불투명)
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        
    }
    */
    private void OnDisable()
    {
        PauseManager.IsPaused -= PauseForWhile;
        // 비활성화 시 해당 오브젝트의 모든 트윈 정지
        _rectTransform.DOKill();
        image.DOKill();
        // 주의: 여기서 gameObject.SetActive(false)를 다시 호출하면 무한 루프가 발생할 수 있어 제거했습니다.
    }

    private void PauseForWhile(bool isStop)
    {
        if (isStop)
            blinkTweener.Pause();
        else
            blinkTweener.Play();
    }
}
