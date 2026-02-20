using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;

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
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}
