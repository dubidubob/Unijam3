using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public static float TargetBaseSize { get; private set; } = 5f;
    public static bool IsLocked { get; private set; } = false;

    [Header("Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _zoomIntensity = 0.3f; // 줌인 되는 강도 (낮을수록 강함)
    [SerializeField] private float _rotateAngle = 2.0f;   // 좌우 회전 각도

    private CancellationTokenSource _shakeCTS;

    // 무비 액션 취소 관리를 위한 CTS
    private CancellationTokenSource _movieCTS;

    [Header("MovieAction")]
    [SerializeField] Image image_MovieUp;
    [SerializeField] Image image_MovieDown;
    [SerializeField] Image image_MovieLeft;
    [SerializeField] Image image_MovieRight;

    private void Awake()
    {
        // [추가] 씬 시작 시 static 변수 반드시 초기화
        IsLocked = false;
        TargetBaseSize = 5f;

        if (_camera == null) _camera = GetComponent<Camera>();

        // 시작할 때 카메라 사이즈를 즉시 5로 초기화
        _camera.orthographicSize = 5f;

        // 초기 상태 모두 fillAmount 0으로 세팅
        ResetAllMovieImages();
    }

    public static void SetMonsterMode(bool isActive, float size = 4f)
    {
        IsLocked = isActive;
        TargetBaseSize = isActive ? size : 5f;
    }

    /// <summary>
    /// 타입에 따라 다른 무비액션
    /// </summary>
    public void MovieAction(float duration,float waitDuration,Define.MovieStyle style)
    {
        // 기존 진행 중인 무비 액션이 있다면 캔슬하여 중복/버그 방지
        CancelMovieAction();
        _movieCTS = new CancellationTokenSource();

        // 파괴될 때(OnDestroy) 안전하게 멈추도록 연결된 토큰 생성
        var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(
            _movieCTS.Token,
            this.GetCancellationTokenOnDestroy()
        );

        // 성능을 위해 UniTaskVoid로 실행
        PlayMovieActionTask(duration,waitDuration, style, linkedCTS.Token).Forget();
    }

    private async UniTaskVoid PlayMovieActionTask(float duration,float waitDruation, Define.MovieStyle style, CancellationToken token)
    {
        // 애니메이션이 확! 들어오는/나가는 시간 (duration이 너무 짧을 경우를 대비한 방어코드)
        float fadeTime = 0.25f;
        float waitTime = duration;

        await UniTask.WaitForSeconds(waitDruation); 
        try
        {
            Sequence inSeq = DOTween.Sequence();

            // 1. FillAmount 1로 확 가는 함수 (타임스케일 자동 영향 받음)
            switch (style)
            {
                case Define.MovieStyle.Hide_UpDown:
                    image_MovieUp.fillAmount = 0f; image_MovieDown.fillAmount = 0f;
                    inSeq.Join(image_MovieUp.DOFillAmount(1f, fadeTime).SetEase(Ease.OutCubic));
                    inSeq.Join(image_MovieDown.DOFillAmount(1f, fadeTime).SetEase(Ease.OutCubic));
                    break;

                case Define.MovieStyle.Hide_Left:
                    image_MovieLeft.fillAmount = 0f;
                    inSeq.Join(image_MovieLeft.DOFillAmount(1f, fadeTime).SetEase(Ease.OutCubic));
                    break;

                case Define.MovieStyle.Hide_Right:
                    image_MovieRight.fillAmount = 0f;
                    inSeq.Join(image_MovieRight.DOFillAmount(1f, fadeTime).SetEase(Ease.OutCubic));
                    break;
            }

            Managers.Sound.Play("SFX/InkSplash",Define.Sound.SFX,1,0.7f);
            // DOTween Task에 토큰을 묶어 게임 오버시 즉시 캔슬되게 설정
            await inSeq.WithCancellation(token);

            // 2. duration까지 지속 (timeScale의 영향을 받기 위해 delayTiming을 기본값으로 사용)
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime), ignoreTimeScale: false, cancellationToken: token);

            // 3. 끝날 때 쯤 FadeOut
            Sequence outSeq = DOTween.Sequence();
            switch (style)
            {
                case Define.MovieStyle.Hide_UpDown:
                    outSeq.Join(image_MovieUp.DOFillAmount(0f, fadeTime).SetEase(Ease.InCubic));
                    outSeq.Join(image_MovieDown.DOFillAmount(0f, fadeTime).SetEase(Ease.InCubic));
                    break;

                case Define.MovieStyle.Hide_Left:
                    outSeq.Join(image_MovieLeft.DOFillAmount(0f, fadeTime).SetEase(Ease.InCubic));
                    break;

                case Define.MovieStyle.Hide_Right:
                    outSeq.Join(image_MovieRight.DOFillAmount(0f, fadeTime).SetEase(Ease.InCubic));
                    break;
            }

            await outSeq.WithCancellation(token);
        }
        catch (OperationCanceledException)
        {
            // [중요] 게임 오버 등으로 인해 캔슬이 발생했을 때 안전하게 원상복구
            ResetAllMovieImages();
        }
    }

    /// <summary>
    /// 외부(예: 게임 매니저)에서 게임 오버 시 직접 호출하여 멈출 수 있는 메서드
    /// </summary>
    public void CancelMovieAction()
    {
        if (_movieCTS != null)
        {
            _movieCTS.Cancel();
            _movieCTS.Dispose();
            _movieCTS = null;
        }
    }

    private void ResetAllMovieImages()
    {
        // DOTween 킬링 후 초기화 (null 체크로 에러 방지)
        if (image_MovieUp != null) { image_MovieUp.DOKill(); image_MovieUp.fillAmount = 0f; }
        if (image_MovieDown != null) { image_MovieDown.DOKill(); image_MovieDown.fillAmount = 0f; }
        if (image_MovieLeft != null) { image_MovieLeft.DOKill(); image_MovieLeft.fillAmount = 0f; }
        if (image_MovieRight != null) { image_MovieRight.DOKill(); image_MovieRight.fillAmount = 0f; }
    }

    private void OnDestroy()
    {
        CancelMovieAction();
        _shakeCTS?.Cancel();
        _shakeCTS?.Dispose();
    }
}