using UnityEngine;
using UnityEngine.UI;

public class KeyboardStageSceneUIController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private StageSceneUI stageSceneUI;

    [Header("Start Button Settings")]
    [SerializeField] private Button startButton;
    [SerializeField] private Sprite startButtonNormalSprite;
    [SerializeField] private Sprite startButtonFocusedSprite;

    [Header("Stage Button Settings")]
    [SerializeField] private Sprite stageButtonNormalSprite;

    // 상태 추적용 변수
    private bool isStartButtonFocused = false;
    private int lastSelectedStageIndex = -1;

    // 맵 이동 실시간 감지용 변수
    private int lastKnownPageLevel = -1;
    private bool lastKnownIsEventMap = false;

    private void Start()
    {
        if (startButton != null && startButton.image != null && startButtonNormalSprite != null)
        {
            startButton.image.sprite = startButtonNormalSprite;
        }

        if (stageSceneUI != null)
        {
            lastKnownPageLevel = stageSceneUI.currentPageLevel;
            lastKnownIsEventMap = stageSceneUI.isEventMap;
        }
    }

    private void Update()
    {
        if (SceneLoadingManager.IsLoading || stageSceneUI == null || stageSceneUI.isAnimating)
            return;

        // 외부 요인(BeadController 등)에 의한 맵 이동 실시간 감지
        if (stageSceneUI.currentPageLevel != lastKnownPageLevel || stageSceneUI.isEventMap != lastKnownIsEventMap)
        {
            lastKnownPageLevel = stageSceneUI.currentPageLevel;
            lastKnownIsEventMap = stageSceneUI.isEventMap;
            ForceResetSelection();
        }

        Button currentBtn = stageSceneUI.GetSelectedButton();

        // ====================================================
        // [상태 A] 시작 버튼에 포커스가 가 있을 때
        // ====================================================
        if (isStartButtonFocused)
        {
            // 왼쪽(LeftArrow 또는 A)
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                SetStartButtonFocus(false);
            }
            // 시작 실행 (기존 조건 + RightArrow, D 추가!)
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                stageSceneUI.StartButtonClicked(null);
            }
            // 위쪽 이동 (UpArrow 또는 W) 추가!
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                SetStartButtonFocus(false); // 1. 시작 버튼 포커스 즉시 해제 (<- 누른 효과)
                NavigateVertical(1);        // 2. 바로 위쪽 스테이지로 이동
            }
            // 아래쪽 이동 (DownArrow 또는 S) 추가!
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                SetStartButtonFocus(false); // 1. 시작 버튼 포커스 즉시 해제 (<- 누른 효과)
                NavigateVertical(-1);       // 2. 바로 아래쪽 스테이지로 이동
            }
            return;
        }

        // ====================================================
        // [상태 B] 스테이지 버튼들을 고르고 있을 때
        // ====================================================
        if (currentBtn == null)
        {
            // 위쪽(UpArrow 또는 W)
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                SelectBoundaryStage(true);
            }
            // 아래쪽(DownArrow 또는 S)
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                SelectBoundaryStage(false);
            }
        }
        else
        {
            // 아래쪽(DownArrow 또는 S)
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                NavigateVertical(-1);
            }
            // 위쪽(UpArrow 또는 W)
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                NavigateVertical(1);
            }
            // 오른쪽(RightArrow 또는 D): 시작 버튼 활성화
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)||Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return))
            {
                lastSelectedStageIndex = stageSceneUI.stageButtons.IndexOf(currentBtn);
                SetStartButtonFocus(true);
            }
        }
    }

    private void ForceResetSelection()
    {
        if (isStartButtonFocused)
        {
            isStartButtonFocused = false;
            if (startButton != null && startButton.image != null && startButtonNormalSprite != null)
            {
                startButton.image.sprite = startButtonNormalSprite;
            }
        }

        lastSelectedStageIndex = -1;
        stageSceneUI.ClearStageSelection();
    }

    private void SetStartButtonFocus(bool focus)
    {
        if (startButton == null || startButton.image == null) return;

        if (focus && !isStartButtonFocused)
        {
            Managers.Sound.Play("SFX/UI/StageHover3", Define.Sound.SFX, 1f, 1f);
        }

        isStartButtonFocused = focus;
        Button currentBtn = stageSceneUI.GetSelectedButton();

        if (focus)
        {
            if (startButtonFocusedSprite != null)
                startButton.image.sprite = startButtonFocusedSprite;

            if (currentBtn != null && stageButtonNormalSprite != null)
            {
                currentBtn.image.sprite = stageButtonNormalSprite;
            }
        }
        else
        {
            if (startButtonNormalSprite != null)
                startButton.image.sprite = startButtonNormalSprite;

            if (lastSelectedStageIndex != -1)
            {
                TriggerStageClick(lastSelectedStageIndex);
            }
        }
    }

    private void SelectBoundaryStage(bool isUpArrow)
    {
        GetCurrentPageBounds(out int minIdx, out int maxIdx);

        if (isUpArrow)
        {
            for (int i = minIdx; i <= maxIdx; i++)
            {
                if (stageSceneUI.IsStageUnlocked(i))
                {
                    TriggerStageClick(i);
                    return;
                }
            }
        }
        else
        {
            for (int i = maxIdx; i >= minIdx; i--)
            {
                if (stageSceneUI.IsStageUnlocked(i))
                {
                    TriggerStageClick(i);
                    return;
                }
            }
        }
    }

    private void NavigateVertical(int direction)
    {
        Button currentBtn = stageSceneUI.GetSelectedButton();
        if (currentBtn == null) return;

        int currentIndex = stageSceneUI.stageButtons.IndexOf(currentBtn);
        if (currentIndex == -1) return;

        int nextIndex = currentIndex + direction;

        GetCurrentPageBounds(out int minIdx, out int maxIdx);

        if (nextIndex < minIdx)
        {
            stageSceneUI.DownButtonClicked(null);
            stageSceneUI.ClearStageSelection();
            return;
        }
        else if (nextIndex > maxIdx)
        {
            stageSceneUI.UpButtonClicked(null);
            stageSceneUI.ClearStageSelection();
            return;
        }

        TriggerStageClick(nextIndex);
    }

    private void GetCurrentPageBounds(out int minIdx, out int maxIdx)
    {
        minIdx = 0; maxIdx = 0;

        if (stageSceneUI.isEventMap)
        {
            if (stageSceneUI.currentPageLevel == 0) { minIdx = 8; maxIdx = 8; }
            else if (stageSceneUI.currentPageLevel == 2) { minIdx = 9; maxIdx = 10; }
        }
        else
        {
            if (stageSceneUI.currentPageLevel == 0) { minIdx = 0; maxIdx = 3; }
            else if (stageSceneUI.currentPageLevel == 1) { minIdx = 4; maxIdx = 6; }
            else if (stageSceneUI.currentPageLevel == 2) { minIdx = 7; maxIdx = 7; }
        }
    }

    private void TriggerStageClick(int targetIndex)
    {
        if (targetIndex >= 0 && targetIndex < stageSceneUI.stageButtons.Count)
        {
            if (stageSceneUI.IsStageUnlocked(targetIndex))
            {
                Button targetButton = stageSceneUI.stageButtons[targetIndex];
                stageSceneUI.StageButtonClicked(targetButton, targetIndex + 1);
            }
        }
    }
}