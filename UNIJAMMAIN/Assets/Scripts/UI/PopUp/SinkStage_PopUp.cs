using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 쓴다면 추가, 일반 Text면 UnityEngine.UI만 있으면 됨

public class SinkStage_PopUp : UI_Popup
{
    [Header("UI 컴포넌트 연결")]
    [SerializeField] private Slider sinkSlider;
    [SerializeField] private TextMeshProUGUI valueText; // 일반 Text면 Text로 변경

    [Header("설정")]
    // 싱크 조절 범위 (예: -0.2초 ~ +0.2초)
    [SerializeField] private float minRange = -0.2f;
    [SerializeField] private float maxRange = 0.2f;

    private void Start()
    {
        // 1. 슬라이더의 최소/최대값 설정
        sinkSlider.minValue = minRange;
        sinkSlider.maxValue = maxRange;

        // 2. 현재 저장된 싱크 값으로 슬라이더 초기화
        //    (저장된 값이 없다면 0)
        float currentSink = IngameData.sinkTimer;
        sinkSlider.value = currentSink;
        UpdateText(currentSink);

        // 3. 슬라이더 값 변경 시 실행할 함수 등록
        sinkSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    // 슬라이더를 드래그할 때마다 호출됨
    private void OnSliderValueChanged(float value)
    {
        // 1. 실제 데이터에 반영
        IngameData.sinkTimer = -value;

        // 2. 텍스트 업데이트
        UpdateText(value);

        // (선택사항) 값이 바뀔 때마다 PlayerPrefs에 저장하고 싶다면:
        // IngameData.SaveGameData(); // 이전에 만든 저장 함수 호출
    }

    private void UpdateText(float value)
    {
        // 소수점 3째 자리까지 표시 (예: +0.050 s)
        // 양수면 + 기호 붙이기
        string sign = value >= 0 ? "+" : "";
        if (valueText != null)
        {
            valueText.text = $"{sign}{value:F3} s";
        }
    }
}
