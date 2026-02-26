using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class LocaleSetting : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private List<Image> langButtonImages; // 버튼들의 Image 컴포넌트를 순서대로 할당 (KO, EN, CH, JA)
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

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
        UpdateButtonVisuals();
    }

    /// <summary>
    /// 언어 버튼의 OnClick 이벤트 함수
    /// public enum Language
    ///{ Korean, English, Chinese,Japanese}
    /// </summary>
    public void ChangeLanguageByIndex(int index)
    {
        ChangeLanguage((Language)index);
    }

    public void ChangeLanguage(Language language)
    {
        string localeCode = GetCodeFromLanguage(language);
        var targetLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            LocalizationManager.SetLanguage(language);
        }
    }

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

    private string GetCodeFromLanguage(Language lang)
    {
        switch (lang)
        {
            case Language.Korean: return "ko";
            case Language.English: return "en";
            case Language.Japanese: return "ja";
            case Language.Chinese: return "zh-Hans";
            default: return "en";
        }
    }
}