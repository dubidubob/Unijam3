using UnityEngine;
using UnityEngine.UI;

public class CanvasForceUpdate : MonoBehaviour
{
    void Awake()
    {
        // 씬이 로드되는 순간 캔버스의 모든 레이아웃 계산을 강제로 다시 실행합니다.
        Canvas.ForceUpdateCanvases();

        // 캔버스 내의 모든 Layout Group이나 Fitter들을 갱신
        foreach (var layout in GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
        }
    }
}