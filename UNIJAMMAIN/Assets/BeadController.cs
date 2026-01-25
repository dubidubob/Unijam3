using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BeadController : MonoBehaviour
{
    [Header("Bead Lists")]
    [SerializeField] List<GameObject> storyBeads;
    [SerializeField] List<GameObject> eventBeads;

    [Header("Effect Settings")]
    [SerializeField] private Camera uiCamera;       // 줌인 효과를 줄 카메라
    [SerializeField] private CanvasGroup blackPanel; // 암전 효과를 줄 검은 패널
    [SerializeField] private float effectDuration = 1.0f;
    [SerializeField] private float targetZoomSize = 4.0f; // 목표 카메라 사이즈

    [Header("Boundary Settings")]
    [SerializeField] private RectTransform backgroundRect; // [추가] 배경 이미지(지도의 전체 크기)

    private void Start()
    {
        if (blackPanel != null)
        {
            blackPanel.alpha = 0f;
            blackPanel.blocksRaycasts = false;
        }

        if (uiCamera == null) uiCamera = Camera.main;
    }

    public void StoryBeadAction(int index = 0)
    {
        if (storyBeads != null && index < storyBeads.Count)
        {
            // GetComponentInChildren 대신 바로 GetComponent 사용 (상황에 맞춰 조정)
            CameraZoominAndBlackOut(storyBeads[index].GetComponent<RectTransform>());
        }
    }

    public void EventBeadAction(int index = 0)
    {
        if (eventBeads != null && index < eventBeads.Count)
        {
            CameraZoominAndBlackOut(eventBeads[index].GetComponent<RectTransform>());
        }
    }

    private void CameraZoominAndBlackOut(RectTransform targetRect)
    {
        StartCoroutine(CoZoomAndFade(targetRect));
    }

    private IEnumerator CoZoomAndFade(RectTransform target)
    {
        // 1. 초기값 설정
        Vector3 startCamPos = uiCamera.transform.position;
        float startCamSize = uiCamera.orthographicSize;
        float time = 0f;

        // 2. 목표 위치 계산
        Vector3 targetCamPos = target.position;
        targetCamPos.z = startCamPos.z; // Z축 유지

        // [핵심 로직] 목표 위치가 배경 밖으로 나가지 않도록 보정(Clamp)
        if (backgroundRect != null)
        {
            targetCamPos = GetClampedTargetPos(targetCamPos, targetZoomSize);
        }

        // 암전 패널 클릭 차단
        if (blackPanel != null) blackPanel.blocksRaycasts = true;

        // 3. 루프 실행
        while (time < effectDuration)
        {
            time += Time.deltaTime;
            float t = time / effectDuration;

            // Ease Out Quart (천천히 도착하는 느낌)
            float smoothT = 1f - Mathf.Pow(1f - t, 4f);

            // 이동 및 줌
            uiCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, smoothT);
            uiCamera.orthographicSize = Mathf.Lerp(startCamSize, targetZoomSize, smoothT);

          
            // 암전
            if (blackPanel != null)
            {
                blackPanel.alpha = Mathf.Lerp(0f, 1f, smoothT);
            }

            yield return null;
        }

        // 4. 최종값 적용
        uiCamera.transform.position = targetCamPos;
        uiCamera.orthographicSize = targetZoomSize;
        if (blackPanel != null) blackPanel.alpha = 1f;

        Debug.Log("연출 종료!");
        // 씬 이동 로직...
    }

    // [추가 함수] 카메라가 배경 밖으로 나가지 않게 위치를 제한하는 함수
    private Vector3 GetClampedTargetPos(Vector3 targetPos, float targetSize)
    {
        // 1. 줌인 된 상태에서의 카메라 높이/너비 계산
        float camHeight = targetSize * 2f;
        float camWidth = camHeight * uiCamera.aspect;

        // 2. 배경 이미지의 월드 좌표 경계(Bound) 구하기
        Vector3[] corners = new Vector3[4];
        backgroundRect.GetWorldCorners(corners);
        // corners[0] = 좌하단, corners[2] = 우상단

        float bgMinX = corners[0].x;
        float bgMaxX = corners[2].x;
        float bgMinY = corners[0].y;
        float bgMaxY = corners[2].y;

        // 3. 카메라 중심이 갈 수 있는 최소/최대 좌표 계산
        // (배경 끝 - 카메라 절반 크기) 만큼만 갈 수 있음
        float minX = bgMinX + (camWidth / 2f);
        float maxX = bgMaxX - (camWidth / 2f);
        float minY = bgMinY + (camHeight / 2f);
        float maxY = bgMaxY - (camHeight / 2f);

        // 4. 목표 위치를 이 범위 안으로 가두기(Clamp)
        float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }
}