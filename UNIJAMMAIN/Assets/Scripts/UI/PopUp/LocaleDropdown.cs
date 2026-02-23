using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization; // 추가

public class LocaleDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        var availableLocales = LocalizationSettings.AvailableLocales.Locales;

        dropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentLocaleIndex = 0;

        for (int i = 0; i < availableLocales.Count; i++)
        {
            var locale = availableLocales[i];

            options.Add(locale.LocaleName);

            if (LocalizationSettings.SelectedLocale == locale)
            {
                currentLocaleIndex = i;
            }
        }

        dropdown.AddOptions(options);

        dropdown.value = currentLocaleIndex;

        dropdown.onValueChanged.AddListener(LocaleSelected);
    }

    // 드롭다운 값이 변경될 때마다 호출되는 함수
    private void LocaleSelected(int index)
    {
        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

        // 유니티 공식 시스템 변경
        LocalizationSettings.SelectedLocale = selectedLocale;

        // 커스텀 매니저(LocalizationManager) 변경
        SyncCustomManager(selectedLocale);
    }

    // 로케일 식별자(en, ko 등)를 기반으로 Enum을 매칭시키는 함수
    private void SyncCustomManager(Locale locale)
    {
        string code = locale.Identifier.Code.ToLower(); // "ko", "en", "ja", "zh" 등

        if (code.Contains("ko")) LocalizationManager.SetLanguage(Language.Korean);
        else if (code.Contains("en")) LocalizationManager.SetLanguage(Language.English);
        else if (code.Contains("ja")) LocalizationManager.SetLanguage(Language.Japanese);
        else if (code.Contains("zh")) LocalizationManager.SetLanguage(Language.Chinese);
    }

}
