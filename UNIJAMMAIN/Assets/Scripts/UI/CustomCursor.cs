using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    // 커서 이미지의 RectTransform을 연결할 변수
    public RectTransform cursorTransform;

    void Awake()
    {
        // 1. 기본 마우스 커서를 보이지 않게 설정
        Cursor.visible = false;
    }

    void Update()
    {
        // 2. 매 프레임마다 마우스 위치를 따라다니도록 설정
        // Input.mousePosition은 스크린 좌표이므로 UI 위치에 바로 대입 가능
        cursorTransform.position = Input.mousePosition;
    }
}