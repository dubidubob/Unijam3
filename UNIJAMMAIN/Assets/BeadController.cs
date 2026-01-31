using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BeadController : MonoBehaviour
{
    [Header("타 cs 연결")]
    [SerializeField] StageSceneUI stageSceneUI;

    [Header("Bead Lists")]
    [SerializeField] List<GameObject> storyBeads;
    [SerializeField] List<GameObject> eventBeads;

    [Header("Effect Settings")]
    [SerializeField] private Camera uiCamera;       // 줌인 효과를 줄 카메라
    [SerializeField] private CanvasGroup blackPanel; // 암전 효과를 줄 검은 패널
    [SerializeField] private CanvasGroup backGroundBlackPanel; // 화면 완전 전환용 검은 패널
    [SerializeField] private float effectDuration = 1.0f;
    [SerializeField] private float targetZoomSize = 4.0f; // 목표 카메라 사이즈

    [Header("Boundary Settings")]
    [SerializeField] private RectTransform backgroundRect; // [추가] 배경 이미지(지도의 전체 크기)

    [Header("Bead 이펙트 이미지 관련 세팅")]
    [SerializeField] private RectTransform storyRect;
    [SerializeField] private RectTransform eventWinterRect;
    [SerializeField] private Image doorooImage;
    [SerializeField] private Image patternImage;
    [Header("맵 오브젝트")]
    private readonly float default_MapPositionY = 893.08f;
    [SerializeField] private GameObject map_MainStory; // 기본값 y 893.08
    [SerializeField] private GameObject map_EventWinter; // 기본값 y -3000


    [Header("스프라이트")]
    [SerializeField] private Sprite dooroo_Original;
    [SerializeField] private Sprite pattern_Original;

    [SerializeField] private Sprite dooroo_Winter;
    [SerializeField] private Sprite pattern_Winter;
       
    private void Start()
    {
        if (blackPanel != null)
        {
            blackPanel.alpha = 0f;
            blackPanel.blocksRaycasts = false;
        }

        if (uiCamera == null) uiCamera = Camera.main;


        map_EventWinter.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400, -4000);
    }


    private Sprite dooroo_targetSprite;
    private Sprite pattern_targetSprite;
    private RectTransform targetRect;
    private GameObject targetMapObject;
       
    public void StoryBeadAction(int index = 0)
    {
        if (storyBeads != null && index < storyBeads.Count)
        {
            // GetComponentInChildren 대신 바로 GetComponent 사용 (상황에 맞춰 조정)
            dooroo_targetSprite = dooroo_Original;
            pattern_targetSprite = pattern_Original;
            targetRect = storyRect;
            targetMapObject = map_MainStory;
            CameraZoominAndBlackOut(storyBeads[index].GetComponent<RectTransform>());
        }
    }


    // 이벤트 버튼 클릭 
    public void EventBeadAction(int index = 0)
    {
        if (eventBeads != null && index < eventBeads.Count)
        {
            dooroo_targetSprite = dooroo_Winter;
            pattern_targetSprite = pattern_Winter;
            targetRect = eventWinterRect;
            targetMapObject = map_EventWinter;
            CameraZoominAndBlackOut(eventBeads[index].GetComponent<RectTransform>());
        }
    }

    private void CameraZoominAndBlackOut(RectTransform targetRect)
    {
        StartCoroutine(CoZoomAndFade(targetRect));
    }

    private IEnumerator CoZoomAndFade(RectTransform target)
    {
        backGroundBlackPanel.blocksRaycasts = true;
        // 1. 초기값 저장 (이 위치로 반드시 돌아오게 됨)
        Vector3 startCamPos = uiCamera.transform.position;
        float startCamSize = uiCamera.orthographicSize;
        float time = 0f;

        // 2. 목표 위치 계산
        Vector3 targetCamPos = target.position;
        targetCamPos.z = startCamPos.z; // Z축 유지

        // 배경 밖으로 나가지 않게 보정
        if (backgroundRect != null)
        {
            targetCamPos = GetClampedTargetPos(targetCamPos, targetZoomSize);
        }

        // 입력 차단
        if (blackPanel != null) blackPanel.blocksRaycasts = true;

        // =========================================================
        // [Phase 1] 줌인 & 암전 (들어갈 때)
        // =========================================================
        // (참고: 루프 밖 DOFade는 제거했습니다. 루프 안에서 수동 제어하는 것이 더 정확합니다.)
        
        while (time < effectDuration)
        {
            yield return null;
            time += Time.unscaledDeltaTime;

            float t = time / effectDuration;
            float smoothT = 1f - Mathf.Pow(1f - t, 4f); // Ease Out Quart

            uiCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, smoothT);
            uiCamera.orthographicSize = Mathf.Lerp(startCamSize, targetZoomSize, smoothT);

            if (blackPanel != null) blackPanel.alpha = Mathf.Lerp(0f, 0.7f, smoothT);
        }

        // 줌인 상태 강제 고정
        uiCamera.transform.position = targetCamPos;
        uiCamera.orthographicSize = targetZoomSize;
        if (blackPanel != null) blackPanel.alpha = 1f;

        // =========================================================
        // [Phase 2] 맵 교체 (암전 상태)
        // =========================================================
        // 임시 모든 것들 안보이게 이동
        if (map_MainStory != null)
            map_MainStory.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400, -3000);
        if (map_EventWinter != null)
            map_EventWinter.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400, -3000);

        //---

        // 타겟은 보이는곳으로 이동
        if (targetMapObject != null)
            targetMapObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400, default_MapPositionY);

        yield return new WaitForSecondsRealtime(0.5f);

        float allblackDuration = 0.7f;
        backGroundBlackPanel.DOFade(1f, allblackDuration); // 완전히 암전

        yield return new WaitForSeconds(allblackDuration);
        // =========================================================
        // [Phase 3] 암전 복구 (화면 밝아짐)
        // =========================================================
        if (blackPanel != null)
        {
            uiCamera.transform.position = startCamPos;
            uiCamera.orthographicSize = startCamSize;
            NewSpriteSetting(); // 두루마리 등 세팅 이미지 설정
            stageSceneUI.MapTargetRectChange(targetRect);
            backGroundBlackPanel.DOFade(0f, 1f).SetUpdate(true);
            yield return blackPanel.DOFade(0f, 1f).SetUpdate(true).WaitForCompletion();  
            blackPanel.blocksRaycasts = false;
           
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // =========================================================
        // [Phase 4] 복귀 (수정됨: DOTween 제거 -> 수동 계산)
        // =========================================================
        // 여기서 DOTween을 쓰면 이상한 곳으로 튈 수 있으므로, 
        // 현재 위치에서 startCamPos까지 직접 계산해서 이동시킵니다.


        // 마지막 맵 이동 연출
        /*
        if (map_EventWinter != null)
        {
            map_EventWinter.GetComponent<RectTransform>().DOAnchorPosY(892f, 1f).SetUpdate(true);
        }
        */

        // 클릭가능
        backGroundBlackPanel.blocksRaycasts = false;
    }


    private void NewSpriteSetting()
    {
        doorooImage.sprite = dooroo_targetSprite;
        patternImage.sprite = pattern_targetSprite;
    }

    // [수정된 함수] 계산 오류 시 중앙 고정 방지 로직 추가
    private Vector3 GetClampedTargetPos(Vector3 targetPos, float targetSize)
    {
        float camHeight = targetSize * 2f;
        float camWidth = camHeight * uiCamera.aspect;

        Vector3[] corners = new Vector3[4];
        backgroundRect.GetWorldCorners(corners);

        float bgMinX = corners[0].x;
        float bgMaxX = corners[2].x;
        float bgMinY = corners[0].y;
        float bgMaxY = corners[2].y;

        float minX = bgMinX + (camWidth / 2f);
        float maxX = bgMaxX - (camWidth / 2f);
        float minY = bgMinY + (camHeight / 2f);
        float maxY = bgMaxY - (camHeight / 2f);

        // [중요 수정] 배경이 카메라보다 작거나 계산이 꼬여서 min > max가 되면
        // Clamp가 강제로 값을 이상한 곳(중앙 등)으로 튀게 만듭니다.
        // 이 경우 Clamp를 하지 않고 그냥 원래 구슬 위치(targetPos)를 반환하도록 안전장치를 겁니다.

        float clampedX = targetPos.x;
        float clampedY = targetPos.y;

        if (minX < maxX) clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
        if (minY < maxY) clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }
}