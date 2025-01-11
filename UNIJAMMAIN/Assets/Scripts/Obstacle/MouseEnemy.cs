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

        // ������ �ݺ������� ����
        blinkTweener = image
            .DOColor(Color.red, blinkDuration)
            .SetLoops(-1, LoopType.Yoyo) // ���� �ݺ�(Yoyo: �պ�)
            .SetEase(Ease.Linear);
    }

    private void Update()
    {
        // ���콺 ���� Ŭ�� ó��
        if (Input.GetMouseButtonDown(1) && dir == Dir.Left) // 0�� ���콺 ���� ��ư
        {
            StopBlinking();
            this.gameObject.SetActive(false);
        }

        // ���콺 ������ Ŭ�� ó��
        if (Input.GetMouseButtonDown(0) && dir == Dir.Right) // 1�� ���콺 ������ ��ư
        {
            StopBlinking();
            this.gameObject.SetActive(false);
        }
    }


    private void StopBlinking()
    {
        if (blinkTweener != null && blinkTweener.IsActive())
        {
            blinkTweener.Kill(); // Tweener �ߴ�
        }

        // ������ �⺻ ������ �ʱ�ȭ (���� ����)
        image.color = Color.white;
    }

    private void OnDisable()
    {
        StopBlinking(); // ������Ʈ�� ��Ȱ��ȭ�Ǹ� ������ �ߴ�
    }
}
