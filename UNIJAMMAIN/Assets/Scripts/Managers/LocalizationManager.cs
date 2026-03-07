using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Steamworks;

public enum Language
{
    Korean,
    English,
    Chinese,
    Japanese
}

public static class LocalizationManager
{
    // key -> (langKey -> value)
    static Dictionary<string, Dictionary<Language, string>> Localization_Table = new Dictionary<string, Dictionary<Language, string>>();
    public static event Action OnLanguageChanged;


    // Load CSV from Resources (can be called at runtime or editor)
   // 기존 Load 함수 대신 LoadAll 폴더 로드 방식으로 변경
    public static void LoadAll(string folderPath = "Localization")
    {
        // 1. 데이터를 담을 딕셔너리를 한 번만 초기화합니다.
        Localization_Table.Clear();

        // 2. 해당 폴더(Resources/Localization) 내의 모든 텍스트 에셋(CSV)을 가져옵니다.
        TextAsset[] csvAssets = Resources.LoadAll<TextAsset>(folderPath);
        
        if (csvAssets == null || csvAssets.Length == 0)
        {
            Debug.LogError($"Localization CSV files not found at Resources/{folderPath}");
            return;
        }

        // 3. 가져온 모든 CSV 파일을 순회하며 하나의 story_table에 합칩니다.
        foreach (TextAsset csvAsset in csvAssets)
        {
            // BOM 제거
            string raw = csvAsset.text;
            raw = raw.Replace("\uFEFF", "");

            using (StringReader reader = new StringReader(raw))
            {
                string headerLine = reader.ReadLine();
                if (string.IsNullOrEmpty(headerLine)) continue; // 빈 파일은 건너뜀

                var headers = SplitCsv(headerLine);
                
                // Map header indices to Language enum (기존 코드와 동일)
                int keyIdx = IndexOfIgnoreCase(headers, "Key");
                int koIdx = IndexOfContains(headers, "Korean"); 
                int enIdx = IndexOfContains(headers, "English");
                int zhIdx = IndexOfContains(headers, "Chinese");
                if (zhIdx == -1) zhIdx = IndexOfContains(headers, "zh-Hans");
                int jaIdx = IndexOfContains(headers, "Japanese");

                if (koIdx == -1) koIdx = IndexOfIgnoreCase(headers, "Korean(ko)");
                if (enIdx == -1) enIdx = IndexOfIgnoreCase(headers, "English(en)");
                if (jaIdx == -1) jaIdx = IndexOfIgnoreCase(headers, "Japanese (ja)");

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var row = SplitCsv(line);
                    if (row.Count <= keyIdx || string.IsNullOrWhiteSpace(row[keyIdx])) continue;

                    string key = row[keyIdx].Trim();

                 


                    // 핵심: 파일이 달라도 키가 딕셔너리에 없다면 새로 생성하여 추가
                    if (!Localization_Table.ContainsKey(key)) 
                    {
                        Localization_Table[key] = new Dictionary<Language, string>();
                    }

                    if (koIdx >= 0 && koIdx < row.Count) Localization_Table[key][Language.Korean] = row[koIdx];
                    if (enIdx >= 0 && enIdx < row.Count) Localization_Table[key][Language.English] = row[enIdx];
                    if (zhIdx >= 0 && zhIdx < row.Count) Localization_Table[key][Language.Chinese] = row[zhIdx];
                    if (jaIdx >= 0 && jaIdx < row.Count) Localization_Table[key][Language.Japanese] = row[jaIdx];
                }
            }
        }

        Debug.Log($"Localization Loaded: {Localization_Table.Count} keys from {csvAssets.Length} files.");
    }

    public static Language CurrentLanguage
    {
        get
        {
            // 유니티 공식 SelectedLocale의 Identifier 가져옴
            var currentLocale = LocalizationSettings.SelectedLocale;
            if (currentLocale == null) return Language.English;

            string code = currentLocale.Identifier.Code;
            return GetLanguageFromCode(code);
        }
    }

    // Locale 코드를 Language enum으로 변환하는 헬퍼 함수
    private static Language GetLanguageFromCode(string code)
    {
        if (code.StartsWith("ko")) return Language.Korean;
        if (code.StartsWith("ja")) return Language.Japanese;
        if (code.StartsWith("zh")) return Language.Chinese;
        return Language.English;
    }
    // 스팀 설정 언어를 가져와 게임 언어로 세팅합니다. (게임 초기화 시 1회 호출 추천)
    public static void SyncWithSteamLanguage()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("[Localization] 스팀이 초기화되지 않아 기본 언어를 사용합니다.");
            return;
        }

        // Steam API를 통해 현재 스팀 클라이언트(또는 게임 속성)에 설정된 언어 코드를 가져옴
        string steamLang = SteamApps.GetCurrentGameLanguage();
        Language targetLang = Language.English; // 기본값

        // Steam 언어 코드를 게임의 Language Enum으로 매핑
        switch (steamLang)
        {
            case "korean": targetLang = Language.Korean; break;
            case "japanese": targetLang = Language.Japanese; break;
            case "schinese": // 간체
            case "tchinese": // 번체
                targetLang = Language.Chinese; break;
            case "english":
            default:
                targetLang = Language.English; break;
        }

        Debug.Log($"[Localization] Steam 언어 감지됨: {steamLang}. 게임 언어를 {targetLang}로 설정합니다.");
        ChangeLanguage(targetLang);
    }

    // 언어 변경 로직 중앙화 (LocaleSetting에 있던 로직을 이쪽으로 가져옴)
    public static void ChangeLanguage(Language language)
    {
        string localeCode = GetCodeFromLanguage(language);
        var targetLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;

            // 이벤트 발생시켜 UI(LocaleSetting 등) 업데이트 유도
            OnLanguageChanged?.Invoke();
        }
        else
        {
            Debug.LogError($"[Localization] Locale을 찾을 수 없습니다: {localeCode}");
        }
    }


    // 언어 변경 (UI용)
    public static void SetLanguage(Language lang)
    {
        OnLanguageChanged?.Invoke();
    }

    // 키로 번역 가져오기 (fallback 사용)
    public static string Get(string key, string fallback = "")
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.Log($"{key}를 찾을 수 없습니다. by null");
            return fallback; }

        if (Localization_Table.TryGetValue(key, out var perLang))
        {
            if (perLang.TryGetValue(CurrentLanguage, out var val))
            {
                if (!string.IsNullOrEmpty(val)) return val;
            }
        }

        return fallback;
    }

    // CSV 파서 (따옴표/콤마 안전)
    static List<string> SplitCsv(string line)
    {
        List<string> result = new List<string>();
        if (line == null) return result;
        bool inQuotes = false;
        StringBuilder cur = new StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                // 두 개의 연속된 쌍따옴표는 하나의 쌍따옴표로 처리
                if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(cur.ToString());
                cur.Clear();
            }
            else
            {
                cur.Append(c);
            }
        }
        result.Add(cur.ToString());
        return result;
    }

    static int IndexOfIgnoreCase(List<string> arr, string target)
    {
        for (int i = 0; i < arr.Count; i++)
            if (string.Equals(arr[i].Trim(), target, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    static int IndexOfContains(List<string> arr, string token)
    {
        for (int i = 0; i < arr.Count; i++)
            if (!string.IsNullOrEmpty(arr[i]) && arr[i].IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) return i;
        return -1;
    }

    // 특정 키가 테이블에 존재하는지 확인하는 기능
    public static bool HasKey(string key)
    {
        return Localization_Table.ContainsKey(key);
    }
    // Language Enum -> Locale Code 변환 헬퍼 (기존 LocaleSetting에서 이동)
    private static string GetCodeFromLanguage(Language lang)
    {
        switch (lang)
        {
            case Language.Korean: return "ko";
            case Language.Japanese: return "ja";
            case Language.Chinese: return "zh-Hans";
            case Language.English: default: return "en";
        }
    }

}
