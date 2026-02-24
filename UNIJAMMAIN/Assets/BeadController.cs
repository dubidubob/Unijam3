using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class BeadController : MonoBehaviour
{
    public enum Bead
    {
        event_Winter,
        event_City,
        story_1,
        story_2,
        story_3
    }

    [Header("타 cs 연결")]
    [SerializeField] StageSceneUI stageSceneUI;

    [Header("Bead Lists")]
    [SerializeField] List<GameObject> storyBeads;
    [SerializeField] List<GameObject> eventBeads;

    [Header("Effect Settings")]
    [SerializeField] private Camera uiCamera;       // 줌인 효과를 줄 카메라
    [SerializeField] private CanvasGroup blackPanel; // Bead 제외 암전 효과암전 효과를 줄 검은 패널
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

    [SerializeField] private RectTransform move_AllContainer;
    [SerializeField] private RectTransform move_NameContainer;


    [Header("스프라이트")]
    [SerializeField] private Sprite dooroo_Original;
    [SerializeField] private Sprite pattern_Original;

    [SerializeField] private Sprite dooroo_Winter;
    [SerializeField] private Sprite pattern_Winter;

    [Header("구슬 오브젝트")]
    [SerializeField] private GameObject eventBead_Winter;
    [SerializeField] private GameObject eventBead_City;
    [SerializeField] private GameObject storyBead_1;
    [SerializeField] private GameObject storyBead_2;
    [SerializeField] private GameObject storyBead_3;


    private Bounds bgWorldBounds;
    private void Start()
    {
        if (blackPanel != null)
        {
            blackPanel.alpha = 0f;
            blackPanel.blocksRaycasts = false;
        }

        if (uiCamera == null) uiCamera = Camera.main;


        map_EventWinter.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400, -4000);

        CacheBackgroundBounds();
        LoadBeadStateActive(); // 구슬 활성화
        WindMoveAction().Forget();
        // container들 움직이기 시작
    }

    private async UniTask WindMoveAction()
    {
        // 취소 토큰 (오브젝트가 파괴될 때 루프를 멈추기 위함)
        var cts = this.GetCancellationTokenOnDestroy();

        // 1. AllContainer 회전 (느리고 크게 흔들림)
        move_AllContainer.DOLocalRotate(new Vector3(0, 0, 2.5f), 2.0f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);

        // 2. NameContainer 회전 (약간 더 빠르고 불규칙하게 흔들림)
        // AllContainer와 시작 타이밍이나 속도를 다르게 주어 엇박자 연출
        move_NameContainer.DOLocalRotate(new Vector3(0, 0, -2.5f), 1.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // UniTask를 사용해 별도의 로직이 필요하다면 아래에 작성 (현재는 Tween이 무한 반복)
        await UniTask.CompletedTask;
    }


    private Sprite dooroo_targetSprite;
    private Sprite pattern_targetSprite;
    private RectTransform targetRect;
    private GameObject targetMapObject;

    private bool isEventMap = false;
    public void StoryBeadAction(int index = 0)
    {
        if (storyBeads != null && index < storyBeads.Count)
        {
            // GetComponentInChildren 대신 바로 GetComponent 사용 (상황에 맞춰 조정)
            if (stageSceneUI.storedStoryLevel == 2) // 지옥스테이지라면 특별 타깃 스프라이트로 지정
            {
                dooroo_targetSprite = stageSceneUI.doroDarkSprite;
                pattern_targetSprite = stageSceneUI.backGroundDarkSprite;
            }
            else
            {
                dooroo_targetSprite = dooroo_Original;
                pattern_targetSprite = pattern_Original;
            }

            targetRect = storyRect;
            targetMapObject = map_MainStory;
            isEventMap = false;
            CameraZoominAndBlackOut(storyBeads[index].GetComponent<RectTransform>(),index);
        }
    }


    // 이벤트 버튼 클릭 index =0 -> 도시 , index =1 -> 겨울
    public void EventBeadAction(int index = 0)
    {
        if (eventBeads != null && index < eventBeads.Count)
        {
            if(index==0)
            {
                Managers.Sound.Play("SFX/UI/GoToWinter_V2", Define.Sound.SFX, 1f, 5f);
            }
            else if(index==1)
            { 
                Managers.Sound.Play("SFX/UI/GoToCity_V2", Define.Sound.SFX, 1f, 5f);
            }

            dooroo_targetSprite = dooroo_Winter;
            pattern_targetSprite = pattern_Winter;
            targetRect = eventWinterRect;
            targetMapObject = map_EventWinter;
            isEventMap = true;
            CameraZoominAndBlackOut(eventBeads[index].GetComponent<RectTransform>(),index);
        }
    }

    private void CameraZoominAndBlackOut(RectTransform targetRect,int idx =0)
    {
        backGroundBlackPanel.blocksRaycasts = true;
        backGroundBlackPanel.interactable = true;
        StartCoroutine(CoZoomAndFade(targetRect,idx));
    }

    private IEnumerator CoZoomAndFade(RectTransform target,int idx)
    {
        // 0. 해상도 변경사항 물리적 강제 갱신
        Canvas.ForceUpdateCanvases();

        // 1. 초기값 저장
        Vector3 startCamPos = uiCamera.transform.position;
        float startCamSize = uiCamera.orthographicSize;
        float time = 0f;

        // 2. [수정] 월드 좌표를 직접 가져오되, Z축은 카메라의 시작 위치를 엄격히 따름
        // TransformPoint를 쓰지 않고 타겟의 world position을 직접 활용합니다.
        Vector3 targetWorldPos = target.position;

        // 만약 피벗이 중앙이 아니라서 틀어지는 경우를 대비해 센터 보정 추가
        Vector3 targetCenterOffset = (Vector3)target.rect.center;
        // 로컬 오프셋을 월드 스케일에 맞춰 변환하여 더함
        targetWorldPos += target.TransformDirection(targetCenterOffset);

        Vector3 targetCamPos = new Vector3(targetWorldPos.x, targetWorldPos.y, startCamPos.z);

        // 3. 타겟 위치를 구한 직후, 배경 범위를 벗어나지 않도록 안전하게 제한 (실시간 갱신)
        if (backgroundRect != null)
        {
            targetCamPos = GetSafeClampedPosition(targetCamPos, targetZoomSize);
        }

        // [Phase 1] 이동 로직 (동일)
        blackPanel.DOFade(0.95f, effectDuration / 2f).SetUpdate(true).SetEase(Ease.OutSine);
        while (time < effectDuration)
        {
            yield return null;
            time += Time.unscaledDeltaTime;
            float t = time / effectDuration;
            float smoothT = 1f - Mathf.Pow(1f - t, 4f);

            uiCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, smoothT);
            uiCamera.orthographicSize = Mathf.Lerp(startCamSize, targetZoomSize, smoothT);

        }

        uiCamera.transform.position = targetCamPos;
        uiCamera.orthographicSize = targetZoomSize;

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

            stageSceneUI.MapTargetRectChange(targetRect);
            stageSceneUI.MapSetting(isEventMap,idx);
            NewSpriteSetting(); // 두루마리 등 세팅 이미지 설정
            backGroundBlackPanel.DOFade(0f, 1f).SetUpdate(true);
            blackPanel.DOFade(0, 1f);
            yield return backGroundBlackPanel.DOFade(0f, 1f).SetUpdate(true).WaitForCompletion();
            blackPanel.blocksRaycasts = false;

        }

        yield return new WaitForSecondsRealtime(0.5f);

        // 클릭가능
        backGroundBlackPanel.blocksRaycasts = false;
        backGroundBlackPanel.interactable = false;
    }


    private void NewSpriteSetting()
    {
        doorooImage.sprite = dooroo_targetSprite;
        patternImage.sprite = pattern_targetSprite;
        patternImage.SetNativeSize();
    }

    private Vector3 GetClampedTargetPos(Vector3 targetPos, float targetSize)
    {
        // 카메라가 실제로 차지하는 월드 크기
        float camHalfHeight = targetSize;
        float camHalfWidth = camHalfHeight * uiCamera.aspect;

        // backgroundRect의 월드 경계
        Vector3[] corners = new Vector3[4];
        backgroundRect.GetWorldCorners(corners);

        float bgLeft = corners[0].x;
        float bgRight = corners[2].x;
        float bgBottom = corners[0].y;
        float bgTop = corners[2].y;

        // 카메라 중심이 갈 수 있는 한계
        float minX = bgLeft + camHalfWidth;
        float maxX = bgRight - camHalfWidth;
        float minY = bgBottom + camHalfHeight;
        float maxY = bgTop - camHalfHeight;

        // 배경이 카메라보다 작은 경우 → 중앙 고정
        float clampedX = (minX <= maxX)
            ? Mathf.Clamp(targetPos.x, minX, maxX)
            : (bgLeft + bgRight) * 0.5f;

        float clampedY = (minY <= maxY)
            ? Mathf.Clamp(targetPos.y, minY, maxY)
            : (bgBottom + bgTop) * 0.5f;

        return new Vector3(clampedX, clampedY, targetPos.z);
    }

    private void CacheBackgroundBounds()
    {
        if (backgroundRect == null) return;

        Vector3[] corners = new Vector3[4];
        backgroundRect.GetWorldCorners(corners);

        bgWorldBounds = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 4; i++)
            bgWorldBounds.Encapsulate(corners[i]);
    }
    private Vector3 ClampCameraPosition(Vector3 desiredPos, float orthoSize)
    {
        float halfHeight = orthoSize;
        float halfWidth = orthoSize * uiCamera.aspect;

        float minX = bgWorldBounds.min.x + halfWidth;
        float maxX = bgWorldBounds.max.x - halfWidth;
        float minY = bgWorldBounds.min.y + halfHeight;
        float maxY = bgWorldBounds.max.y - halfHeight;

        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);

        return desiredPos;
    }

    /// <summary>
    /// 새로운 비드 추가해주기
    /// </summary>
    public void UpdateNewBead(Bead bead)
    {
        GameObject target = null;

        switch (bead)
        {
            case Bead.event_Winter:
                target = eventBead_Winter;
                break;

            case Bead.event_City:
                target = eventBead_City;
                break;

            case Bead.story_1:
                target = storyBead_1;
                break;

            case Bead.story_2:
                target = storyBead_2;
                break;

            case Bead.story_3:
                target = storyBead_3;
                break;
        }

        if (target == null)
        {
            Debug.LogWarning($"[BeadController] {bead} 에 해당하는 오브젝트가 없습니다.");
            return;
        }

        // 이미 켜져있으면 무시
        if (target.activeSelf)
            return;

        // 활성화
        target.SetActive(true);

        // 리스트에도 자동 추가 (나중에 클릭용 인덱스 대응 유지)
        if (bead.ToString().StartsWith("story"))
        {
            if (!storyBeads.Contains(target))
                storyBeads.Add(target);
        }
        else
        {
            if (!eventBeads.Contains(target))
                eventBeads.Add(target);
        }
    }

    public void LoadBeadStateActive()
    {
        int clear = IngameData._unLockStageIndex;

        UpdateNewBead(Bead.story_1);
        // 1 이상 → 겨울 이벤트 + 겨울맞이(스토리1)
        if (IngameData._unLockStageIndex >= 2 || IngameData._isStoryCompleteClear)
        {
            UpdateNewBead(Bead.event_Winter);
        }

        // 3 이상 → 도시 이벤트 + 스토리2
        if(IngameData._unLockStageIndex >= 4 || IngameData._isStoryCompleteClear)
        {
            UpdateNewBead(Bead.event_City);
            UpdateNewBead(Bead.story_2);
        }

        // 6 이상 → 스토리3
        if (clear >= 6)
        {
            UpdateNewBead(Bead.story_3);
        }

        Debug.Log(IngameData._unLockStageIndex);

        // (필요 시 추가 확장 가능)
        // 7 이상 → 밤하늘 맵 같은 추가 해금 여기에 이어서 작성
    }

    /// <summary>
    /// 실시간으로 배경 RectTransform의 월드 좌표를 가져와 카메라가 배경 밖으로 나가지 않도록 제한합니다.
    /// </summary>
    private Vector3 GetSafeClampedPosition(Vector3 targetPos, float targetZoomSize)
    {
        if (backgroundRect == null) return targetPos;

        // 1. 현재 프레임 기준의 배경 월드 좌표 실시간 계산 (해상도 변경 대응)
        Vector3[] corners = new Vector3[4];
        backgroundRect.GetWorldCorners(corners);

        float bgLeft = corners[0].x;
        float bgRight = corners[2].x;
        float bgBottom = corners[0].y;
        float bgTop = corners[2].y;

        // 2. 카메라가 차지하는 월드 크기 계산
        float camHalfHeight = targetZoomSize;
        float camHalfWidth = camHalfHeight * uiCamera.aspect;

        // 3. 카메라 중심이 위치할 수 있는 한계선 (최소/최대 좌표)
        float minX = bgLeft + camHalfWidth;
        float maxX = bgRight - camHalfWidth;
        float minY = bgBottom + camHalfHeight;
        float maxY = bgTop - camHalfHeight;

        // 4. 안전한 클램핑 (배경이 너무 작아서 카메라보다 작을 경우 중앙에 고정)
        float clampedX = (minX <= maxX)
            ? Mathf.Clamp(targetPos.x, minX, maxX)
            : (bgLeft + bgRight) * 0.5f;

        float clampedY = (minY <= maxY)
            ? Mathf.Clamp(targetPos.y, minY, maxY)
            : (bgBottom + bgTop) * 0.5f;

        return new Vector3(clampedX, clampedY, targetPos.z);
    }
}