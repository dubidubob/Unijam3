using UnityEngine;

public class DebugingUI : MonoBehaviour
{
    private string logText = "";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // 씬이 넘어가도 로그창 유지
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 최신 로그가 맨 위로 오도록 누적 (너무 길어지면 자름)
        logText = $"[{type}] {logString}\n" + logText;
        if (logText.Length > 2000) logText = logText.Substring(0, 2000);
    }

    private void OnGUI()
    {
        // 글씨 크기 키우기 (모바일에서도 잘 보이게)
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(2f, 2f, 1f));

        // 화면 좌측 상단에 반투명한 검은색 배경의 텍스트 박스 생성
        logText = GUI.TextArea(new Rect(10, 10, Screen.width / 2.5f, Screen.height / 2.5f), logText);
    }
}