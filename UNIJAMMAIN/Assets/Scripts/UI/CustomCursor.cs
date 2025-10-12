using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    // Ŀ�� �̹����� RectTransform�� ������ ����
    public RectTransform cursorTransform;

    void Awake()
    {
        // 1. �⺻ ���콺 Ŀ���� ������ �ʰ� ����
        Cursor.visible = false;
    }

    void Update()
    {
        // 2. �� �����Ӹ��� ���콺 ��ġ�� ����ٴϵ��� ����
        // Input.mousePosition�� ��ũ�� ��ǥ�̹Ƿ� UI ��ġ�� �ٷ� ���� ����
        cursorTransform.position = Input.mousePosition;
    }
}