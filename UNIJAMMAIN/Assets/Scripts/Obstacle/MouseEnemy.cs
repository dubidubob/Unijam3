using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MouseEnemy : MonoBehaviour
{
    private enum Dir
    {
        Left,
        Right
    }

    [SerializeField] private Dir dir = Dir.Left;

    private Image image;
    private Tweener blinkTweener;
    public float initialBlinkDuration = 0.8f; // 시작 블링킹 지속 시간
    public float minBlinkDuration = 0.05f;     // 블링킹 최소 지속 시간
    public float blinkSpeedIncrease = 0.1f;  // 매 사이클마다 지속 시간 감소량

    private float currentBlinkDuration;

    private void Awake()
    {
        image = GetComponent<Image>();
        // 초기 색상 설정: 원하는 RGB 색상에 알파 1로 설정 (예: 흰색)
        image.color = new Color(1f, 0f, 0f, 1f);
    }

    private void OnEnable()
    {
        currentBlinkDuration = initialBlinkDuration;
        StartBlinking();
    }

    private void StartBlinking()
    {
        // 기존의 Tweener가 활성화되어 있다면 중단
        if (blinkTweener != null && blinkTweener.IsActive())
        {
            blinkTweener.Kill();
        }

        // 블링킹 Tween 생성: 알파 1 -> 0 -> 1 (투명도 변화)
        blinkTweener = image
            .DOFade(0f, currentBlinkDuration) // 알파를 0으로 변경
            .SetLoops(2, LoopType.Yoyo)       // 한 번의 블링킹 사이클 (0 -> 1)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // 다음 블링킹을 위해 지속 시간 감소 (최소값 보장)
                currentBlinkDuration = Mathf.Max(currentBlinkDuration - blinkSpeedIncrease, minBlinkDuration);
                StartBlinking(); // 재귀 호출로 다음 블링킹 시작
            });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && dir == Dir.Left)
        {
            StopBlinking();
            Debug.Log($"왼쪽 {dir}");
            gameObject.SetActive(false);
        }

        if (Input.GetMouseButtonDown(1) && dir == Dir.Right)
        {
            StopBlinking();
            Debug.Log($"오른쪽 {dir}");
            gameObject.SetActive(false);
        }
    }

    private void StopBlinking()
    {
        if (blinkTweener != null && blinkTweener.IsActive())
        {
            blinkTweener.Kill(); // 현재 Tween 중단
        }

        // 알파를 1로 초기화 (완전히 불투명)
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
    }

    private void OnDisable()
    {
        StopBlinking(); // 오브젝트 비활성화 시 블링킹 중단
    }
}
