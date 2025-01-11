using System.Collections;
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
    [SerializeField] private Sprite monsterMouse;
    private Sprite monsterHi;
    private Image image;
    private Tweener blinkTweener;
    public float initialBlinkDuration = 0.8f; // ���� ��ŷ ���� �ð�
    public float minBlinkDuration = 0.05f;     // ��ŷ �ּ� ���� �ð�
    public float blinkSpeedIncrease = 0.1f;    // �� ����Ŭ���� ���� �ð� ���ҷ�

    private float currentBlinkDuration;

    private void Awake()
    {
        image = GetComponent<Image>();
        // �ʱ� ���� ����: ���ϴ� RGB ���� ���� 1�� ���� (��: ���)
        image.color = new Color(1f, 0f, 0f, 1f);
        monsterHi = image.sprite;
    }

    private void OnEnable()
    {
        currentBlinkDuration = initialBlinkDuration;
        StartBlinking();
    }

    private void StartBlinking()
    {
        float lifetime = 3f;
        // ������ Tweener�� Ȱ��ȭ�Ǿ� �ִٸ� �ߴ�
        if (blinkTweener != null && blinkTweener.IsActive())
        {
            blinkTweener.Kill();
        }

        // ��ŷ Tween ����: ���� 1 -> 0 -> 1 (���� ��ȭ)
        blinkTweener = image
            .DOFade(0f, currentBlinkDuration) // ���ĸ� 0���� ����
            .SetLoops(2, LoopType.Yoyo)       // �� ���� ��ŷ ����Ŭ (0 -> 1)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // ���� ��ŷ�� ���� ���� �ð� ���� (�ּҰ� ����)
                currentBlinkDuration = Mathf.Max(currentBlinkDuration - blinkSpeedIncrease, minBlinkDuration);
                lifetime -= currentBlinkDuration * 2;
                if (lifetime > 0)
                {
                    StartBlinking(); // ��� ȣ��� ���� ��ŷ ����
                }
                else
                {
                    Managers.Game.DecHealth();
                    image.sprite = monsterMouse;
                    gameObject.SetActive(false); // �������� ���� �� ������Ʈ ��Ȱ��ȭ
                }
            });

        // ��ŷ �߿� sprite�� �����ϰ� �ʹٸ� ���⼭ ����
        image.sprite = monsterHi;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && dir == Dir.Left)
        {
            StopBlinking();
            Debug.Log($"���� {dir}");
            Managers.Game.ComboInc();
            gameObject.SetActive(false);
        }

        if (Input.GetMouseButtonDown(1) && dir == Dir.Right)
        {
            StopBlinking();
            Debug.Log($"������ {dir}");
            Managers.Game.ComboInc();
            gameObject.SetActive(false);
        }
    }

    private void StopBlinking()
    {
        if (blinkTweener != null && blinkTweener.IsActive())
        {
            blinkTweener.Kill(); // ���� Tween �ߴ�
        }

        // ���ĸ� 1�� �ʱ�ȭ (������ ������)
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
    }

    private void OnDisable()
    {
        StopBlinking(); // ������Ʈ ��Ȱ��ȭ �� ��ŷ �ߴ�
    }
}
