using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CameraController : MonoBehaviour
{
    public static float TargetBaseSize { get; private set; } = 5f;
    public static bool IsLocked { get; private set; } = false;

    [Header("Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _zoomIntensity = 0.3f; // 줌인 되는 강도 (낮을수록 강함)
    [SerializeField] private float _rotateAngle = 2.0f;   // 좌우 회전 각도

    private CancellationTokenSource _shakeCTS;
 
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
}