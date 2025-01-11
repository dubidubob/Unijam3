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

    Image image;
    private Tweener blinkTweener;
    public float blinkDuration = 1.0f;

    private void Awake()
    {
        image = gameObject.GetComponent<Image>();
    }

    private void OnEnable()
    {
        image.color = Color.white;

        // 색상을 반복적으로 변경
        blinkTweener = image
            .DOColor(Color.red, blinkDuration)
            .SetLoops(-1, LoopType.Yoyo) // 무한 반복(Yoyo: 왕복)
            .SetEase(Ease.Linear);
    }

    private void Update()
    {
        // 마우스 왼쪽 클릭 처리
        if (Input.GetMouseButtonDown(1) && dir == Dir.Left) // 0은 마우스 왼쪽 버튼
        {
            StopBlinking();
            this.gameObject.SetActive(false);
        }

        // 마우스 오른쪽 클릭 처리
        if (Input.GetMouseButtonDown(0) && dir == Dir.Right) // 1은 마우스 오른쪽 버튼
        {
            StopBlinking();
            this.gameObject.SetActive(false);
        }
    }


    private void StopBlinking()
    {
        if (blinkTweener != null && blinkTweener.IsActive())
        {
            blinkTweener.Kill(); // Tweener 중단
        }

        // 색상을 기본 색으로 초기화 (선택 사항)
        image.color = Color.white;
    }

    private void OnDisable()
    {
        StopBlinking(); // 오브젝트가 비활성화되면 깜빡임 중단
    }
}
