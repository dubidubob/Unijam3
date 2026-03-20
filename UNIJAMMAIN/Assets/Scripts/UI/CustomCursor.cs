using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;

// CanvasGroup 컴포넌트가 없으면 자동으로 추가해 줍니다.
[RequireComponent(typeof(CanvasGroup))]
public class CustomCursor : MonoBehaviour
{
    public RectTransform cursorTransform;

    [Header("커서 설정")]
    public float hideDelay = 3.0f;     // 대기 시간
    public float fadeDuration = 0.3f;  // 스르륵 사라지는 시간 (DOTween)

    private Vector3 _lastMousePosition;
    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _hideCts;

    private bool _isHidden = false;

    private void Awake()
    {
        Cursor.visible = false;

        // UI 메쉬 리빌드 없이 알파값만 조절하기 위해 CanvasGroup 사용 (성능 최적화 핵심)
        _canvasGroup = GetComponent<CanvasGroup>();

        _lastMousePosition = Input.mousePosition;

        // 처음 시작할 때 3초 타이머 가동
        StartHideTimer();
    }

    private void Update()
    {
        // 1. 현재 마우스 위치 가져오기
        Vector3 currentMousePosition = Input.mousePosition;

        // 2. 마우스가 1픽셀이라도 움직였을 때만 내부 로직 실행
        if (currentMousePosition != _lastMousePosition)
        {
            _lastMousePosition = currentMousePosition;
            cursorTransform.position = currentMousePosition; // 위치 갱신

            // 커서가 숨겨져 있거나, 숨겨지는 연출 중이라면 즉시 표시
            if (_isHidden || _canvasGroup.alpha < 1f)
            {
                ShowCursor();
            }

            // 마우스가 움직였으므로 숨김 타이머를 리셋하고 다시 시작
            StartHideTimer();
        }
    }

    private void ShowCursor()
    {
        _isHidden = false;

        // 진행 중이던 DOTween 페이드 효과 즉시 중단
        _canvasGroup.DOKill();

        // 투명도 100%로 즉시 복구
        _canvasGroup.alpha = 1f;
        Cursor.visible = false; // 혹시나 윈도우 창 밖을 나갔다 왔을 때 대비
    }

    private void StartHideTimer()
    {
        // 기존에 돌아가던 타이머(UniTask)가 있다면 칼같이 취소 및 메모리 해제
        if (_hideCts != null)
        {
            _hideCts.Cancel();
            _hideCts.Dispose();
        }

        // 새로운 토큰 발급 후 비동기 대기 시작
        _hideCts = new CancellationTokenSource();
        HideAfterDelayAsync(_hideCts.Token).Forget();
    }

    /// <summary>
    /// Update문 없이 백그라운드에서 3초를 세고 DOTween으로 사라지게 하는 UniTask
    /// </summary>
    private async UniTaskVoid HideAfterDelayAsync(CancellationToken token)
    {
        // 1. 3초 대기 (마우스를 움직여서 Cancel 되면 에러 없이 즉시 반환)
        bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(hideDelay), cancellationToken: token).SuppressCancellationThrow();

        if (isCanceled) return; // 3초가 지나기 전에 마우스를 움직여서 취소됨

        // 2. 3초를 온전히 무사히 넘겼다면 DOTween으로 부드럽게 페이드 아웃
        _isHidden = true;

        // CanvasGroup의 alpha를 0으로 서서히 변경
        await _canvasGroup.DOFade(0f, fadeDuration)
            .SetLink(gameObject) // 오브젝트 파괴 시 트윈 안전 종료
            .ToUniTask(cancellationToken: token)
            .SuppressCancellationThrow();
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 메모리 누수 방지
        _canvasGroup.DOKill();
        if (_hideCts != null)
        {
            _hideCts.Cancel();
            _hideCts.Dispose();
        }
    }
}