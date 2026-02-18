using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using System.Runtime.InteropServices;

public class CameraController : MonoBehaviour
{
    public static float TargetBaseSize { get; private set; } = 5f;
    public static bool IsLocked { get; private set; } = false;

    [Header("Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _zoomIntensity = 0.3f; // 줌인 되는 강도 (낮을수록 강함)
    [SerializeField] private float _rotateAngle = 2.0f;   // 좌우 회전 각도

    private CancellationTokenSource _shakeCTS;

    [Header("Window Stretch Settings")]
    [SerializeField] private bool _useWindowStretch = true;
    [SerializeField] private int _stretchIntensityX = 100; // 가로로 늘어날 픽셀 양
    [SerializeField] private int _stretchIntensityY = 100;   // 세로로 늘어날 픽셀 양 (필요 시)

    // 설정 변수들 
    [SerializeField] float expandTime = 1f;  // 커지는 데 걸리는 시간
    [SerializeField] float holdTime = 0.5f;    // 커진 상태로 유지되는 시간
    [SerializeField] float shrinkTime = 1f;  // 다시 원래대로 돌아오는 시간


    [Header("CanvasGroup")]
    [SerializeField] private CanvasGroup backGroundCanvas;


    [Header("Enemy Transform")]
    [SerializeField] Transform A_Enemytransform;
    [SerializeField] Transform D_Enemytransform;
    [SerializeField] Transform W_Enemytransform;
    [SerializeField] Transform S_Enemytransform;
    public static void SetMonsterMode(bool isActive, float size = 4f)
    {
        IsLocked = isActive;
        TargetBaseSize = isActive ? size : 5f;
    }

    /// <summary>
    /// durationBeat 박자만큼 쿵짝거리는 카메라 액션을 실행합니다.
    /// </summary>
    /// <param name="durationBeat">몇 박자 동안 지속할지 (예: 4.0f)</param>
    public async UniTask RythmCameraAction(float durationBeat)
    {
        // 1. 기존에 실행 중인 카메라 액션이 있다면 취소
        if (_shakeCTS != null)
        {
            _shakeCTS.Cancel();
            _shakeCTS.Dispose();
        }
        _shakeCTS = new CancellationTokenSource();

        // 2. 현재 BPM 가져오기 (Managers.Beat.BPM이 있다고 가정, 없으면 기본값 120)
        // 실제 프로젝트의 BPM 변수로 교체하세요.
        float currentBPM = 120f;
        try { currentBPM = IngameData.GameBpm; } catch { }

        float secPerBeat = 60f / currentBPM; // 1박자당 시간(초)
        int totalBeats = Mathf.FloorToInt(durationBeat); // 총 반복 횟수

        // 안전 장치: 부모 객체 파괴 시 취소되도록 토큰 연결
        var token = CancellationTokenSource.CreateLinkedTokenSource(_shakeCTS.Token, this.GetCancellationTokenOnDestroy()).Token;

        try
        {
            // 3. 박자 수만큼 반복 (쿵-짝-쿵-짝)
            for (int i = 0; i < totalBeats; i++)
            {
                // 홀수/짝수에 따라 회전 방향 결정 (오른쪽 -> 왼쪽 -> 오른쪽...)
                // i가 0부터 시작하므로 첫 박자는 왼쪽(-1) 혹은 오른쪽(1) 취향껏 설정
                float dir = (i % 2 == 0) ? -1f : 1f;

                float timer = 0f;

                // --- [단일 비트 루프] ---
                // 한 박자 시간 동안 (강하게 줌인/회전 했다가 -> 서서히 원상복구)
                while (timer < secPerBeat)
                {
                    timer += Time.deltaTime;
                    float t = timer / secPerBeat; // 0.0 ~ 1.0 진행도

                    // EaseOutCubic: 처음에 확 변했다가 천천히 돌아오는 느낌
                    // 1에서 0으로 줄어드는 값 (Bounce Factor)
                    float punch = 1f - Mathf.Pow(t, 3);

                    // A. 회전 처리 (원래 각도(0) + 펀치 * 방향 * 각도)
                    float currentZ = punch * dir * _rotateAngle;
                    _camera.transform.rotation = Quaternion.Euler(0, 0, currentZ);

                    // B. 줌 처리 (원래 사이즈 - 펀치 * 강도)
                    // * TargetBaseSize는 외부에서 모드가 바뀌어도 반영되도록 실시간 참조
                    float currentSize = TargetBaseSize - (punch * _zoomIntensity);
                    _camera.orthographicSize = currentSize;

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
        }
        catch (System.OperationCanceledException)
        {
            // 취소되었을 때 처리 (필요시)
        }
        finally
        {
            // 4. 액션이 끝나거나 취소되면 카메라를 깔끔하게 원상복구
            _camera.transform.rotation = Quaternion.identity;
            _camera.orthographicSize = TargetBaseSize;

            _shakeCTS.Dispose();
            _shakeCTS = null;
        }
    }

    /// <summary>
    /// 창모드로 게임시작하여 적용될 수 있는 좌우로 OS화면을 넓히는 코드
    /// </summary>
    /// <param name="durationBeats"></param>
    /// <returns></returns>
    async public UniTask WindowStretchAction(float durationBeat, float stretchX_rate = 0,float stretchY_rate =0)
    {
        if (Screen.fullScreen) return; // 전체화면일 때는 작동하지 않음
        AspectRatioEnforcer.Instance.isCameraAction = true; // true로 설정하여 화면 비율을 마음대로 조절 할 수 없도록 설정
        await backGroundCanvas.DOFade(0, 1f);

        // 3배 멀어지게하기
        A_Enemytransform.DOLocalMoveX(A_Enemytransform.position.x * 3, 0f);
        S_Enemytransform.DOLocalMoveY(S_Enemytransform.position.y * 3, 0f);
        D_Enemytransform.DOLocalMoveX(D_Enemytransform.position.x * 3, 1f);
        W_Enemytransform.DOLocalMoveY(W_Enemytransform.position.y * 3, 1f);


        // 1. 초기 해상도 저장 (원복을 위해)
        int baseWidth = Screen.width;
        int baseHeight = Screen.height;
        // 2. 늘어날 양 계산 (rate가 2이면 baseWidth * 2만큼 더해서 최종 3배가 됨)
        // rate가 0이면 늘어날 양이 0이므로 크기 변화 없음
        int stretchX = (int)(baseWidth * stretchX_rate);
        int stretchY = (int)(baseHeight * stretchY_rate);



        float currentBPM = 120f;
        try { currentBPM = IngameData.GameBpm; } catch { }
        float secPerBeat = 60f / currentBPM;
        int totalBeats = Mathf.FloorToInt(durationBeat);

        // 늘어나는 시간: 1초 / 유지: durationBeat / 줄어드는 시간: 1초
        float expandDuration = expandTime;
        float holdDuration = (secPerBeat * durationBeat);
        float shrinkDuration = secPerBeat * shrinkTime;


        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            // A. 확장 (0 -> 1)
            await DOVirtual.Float(0, 1, expandDuration, value =>
            {
                // _stretchIntensityX 대신 계산된 stretchX를 전달
                int targetWidth = baseWidth + (int)(value * stretchX);
                int targetHeight = baseHeight + (int)(value * stretchY);
                Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed);
            }).SetEase(Ease.OutExpo).ToUniTask(cancellationToken: token);

            // B. 유지 (Hold)
            if (holdDuration > 0)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(holdDuration), cancellationToken: token);
            }

            // C. 축소 (1 -> 0)
            await DOVirtual.Float(1, 0, shrinkDuration, value =>
            {
                int targetWidth = baseWidth + (int)(value * stretchX);
                int targetHeight = baseHeight + (int)(value * stretchY);
                Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed);
            }).ToUniTask(cancellationToken: token);
        }
        catch (System.OperationCanceledException) { }
        finally
        {
            // 종료 및 복구
            Screen.SetResolution(baseWidth, baseHeight, FullScreenMode.Windowed);
            await backGroundCanvas.DOFade(1, 1f);
            AspectRatioEnforcer.Instance.isCameraAction = false;
        }
    }

    [Header("Render Texture Setup")]
    [SerializeField] private Canvas _overlayCanvas; // 최상위 Canvas (Constant Pixel Size 권장)
    [SerializeField] private UnityEngine.UI.RawImage _renderDisplay; // 게임 화면을 보여줄 RawImage

    /// <summary>
    /// 비트에 따라 화면이 쿵짝쿵짝 변하는 함수 (리듬닥터 스타일)
    /// 창만 움직이고 내부 렌더링은 고정
    /// </summary>
    async public UniTask WindowRythmContinueStretchAction(float durationBeat)
    {
        if (Screen.fullScreen) return;
        if (_overlayCanvas == null || _renderDisplay == null)
        {
            Debug.LogError("Overlay Canvas와 RawImage를 할당해야 리듬닥터 효과가 가능합니다.");
            return;
        }

        AspectRatioEnforcer.Instance.isCameraAction = true;

        // 1. 초기 상태 저장
        WindowManager.RECT startRect = WindowManager.GetCurrentWindowPos();
        int baseWidth = Screen.width;
        int baseHeight = Screen.height;

        // 2. [핵심] 게임 화면을 고정 크기 RenderTexture로 "박제"
        RenderTexture rt = new RenderTexture(baseWidth, baseHeight, 24);
        RenderTexture originalTarget = _camera.targetTexture;
        _camera.targetTexture = rt;

        // 3. Canvas 설정 (Screen Space - Overlay, Constant Pixel Size)
        _overlayCanvas.gameObject.SetActive(true);

        // 4. RawImage 설정 - 화면 중앙에 고정 크기로 표시
        _renderDisplay.texture = rt;
        _renderDisplay.gameObject.SetActive(true);
        _renderDisplay.rectTransform.sizeDelta = new Vector2(baseWidth, baseHeight);
        _renderDisplay.rectTransform.anchoredPosition = Vector2.zero; // 중앙 고정

        float currentBPM = IngameData.GameBpm;
        float secPerBeat = 60f / currentBPM;
        int totalBeats = Mathf.FloorToInt(durationBeat);
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            for (int i = 0; i < totalBeats; i++)
            {
                float timer = 0f;
                while (timer < secPerBeat)
                {
                    timer += Time.deltaTime;
                    float t = timer / secPerBeat;

                    // Sin 곡선으로 자연스러운 리듬감
                    float punch = Mathf.Sin(t * Mathf.PI);

                    // 창 크기 계산 (중앙 정렬을 위해 짝수 스냅핑)
                    int addW = Mathf.RoundToInt(punch * _stretchIntensityX);
                    int addH = Mathf.RoundToInt(punch * _stretchIntensityY);
                    if (addW % 2 != 0) addW++;
                    if (addH % 2 != 0) addH++;

                    int targetWidth = baseWidth + addW;
                    int targetHeight = baseHeight + addH;

                    // [중요] Unity 해상도는 변경하지 않음! Win32 API로만 창 조작
                    WindowManager.ResizeCentered(targetWidth, targetHeight, startRect);

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
        }
        catch (System.OperationCanceledException) { }
        finally
        {
            // 원상 복구
            _camera.targetTexture = originalTarget;
            _renderDisplay.gameObject.SetActive(false);
            _overlayCanvas.gameObject.SetActive(false);
            _renderDisplay.texture = null;

            if (rt != null)
            {
                rt.Release();
                Destroy(rt);
            }

            WindowManager.ResizeCentered(baseWidth, baseHeight, startRect);
            AspectRatioEnforcer.Instance.isCameraAction = false;
        }
    }

    /// <summary>
    /// t값(0~1)에 따라 해상도를 실시간으로 적용하는 헬퍼 함수
    /// </summary>
    private void ApplyResolution(int baseW, int baseH, float t)
    {
        int targetWidth = baseW + (int)(t * _stretchIntensityX);
        int targetHeight = baseH + (int)(t * _stretchIntensityY);
        Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed);
    }
}