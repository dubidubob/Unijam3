using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class LocaleSetting : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private List<Image> langButtonImages; // 버튼 순서: KO, EN, CH, JA
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color inactiveColor = Color.gray;

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateButtonVisuals;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateButtonVisuals;
    }

    private void Start()
    {

        // 현재 설정된 언어에 맞춰 버튼 색상 업데이트
        UpdateButtonVisuals();
    }

    /// <summary>
    /// 언어 버튼의 OnClick 이벤트 함수
    /// 버튼에 매개변수로 0(KO), 1(EN), 2(CH), 3(JA)를 넣어주세요.
    /// </summary>  
    public void ChangeLanguageByIndex(int index)
    {
        // 플레이어가 직접 클릭했으므로 true를 기본값으로 넘겨 설정이 저장되게 함
        LocalizationManager.ChangeLanguage((Language)index, true);

        Application.Quit();
    }

    // 중복되던 ChangeLanguage(), GetCodeFromLanguage() 제거 완료 (매니저 로직 사용)

    private void UpdateButtonVisuals()
    {
        if (langButtonImages == null || langButtonImages.Count == 0) return;

        int currentIndex = (int)LocalizationManager.CurrentLanguage;

        for (int i = 0; i < langButtonImages.Count; i++)
        {
            if (langButtonImages[i] == null) continue;

            langButtonImages[i].color = (i == currentIndex) ? activeColor : inactiveColor;
        }
    }
}