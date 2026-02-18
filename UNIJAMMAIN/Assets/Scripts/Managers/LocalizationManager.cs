using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

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
    static Dictionary<string, Dictionary<Language, string>> table = new Dictionary<string, Dictionary<Language, string>>();

    public static Language CurrentLanguage = Language.Korean;
    public static event Action OnLanguageChanged;

    // Load CSV from Resources (can be called at runtime or editor)
    public static void Load(string resourcesPath = "Localization/StoryDialogue")
    {
        table.Clear();

        TextAsset csvAsset = Resources.Load<TextAsset>(resourcesPath);
        if (csvAsset == null)
        {
            Debug.LogError($"Localization CSV not found at Resources/{resourcesPath}");
            return;
        }

        // BOM 제거
        string raw = csvAsset.text;
        raw = raw.Replace("\uFEFF", "");

        using (StringReader reader = new StringReader(raw))
        {
            string headerLine = reader.ReadLine();
            if (string.IsNullOrEmpty(headerLine))
            {
                Debug.LogError("Localization CSV header is empty");
                return;
            }

            var headers = SplitCsv(headerLine);
            // Map header indices to Language enum (대소문자/부분매칭 허용)
            int keyIdx = IndexOfIgnoreCase(headers, "Key");
            int koIdx = IndexOfContains(headers, "Korean"); // "Korean(ko)"
            int enIdx = IndexOfContains(headers, "English");
            int zhIdx = IndexOfContains(headers, "Chinese");
            if (zhIdx == -1)
                zhIdx = IndexOfContains(headers, "zh-Hans");
            int jaIdx = IndexOfContains(headers, "Japanese");

            // Fallback heuristic if above failed: try specific exact labels
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
                if (!table.ContainsKey(key)) table[key] = new Dictionary<Language, string>();

                if (koIdx >= 0 && koIdx < row.Count) table[key][Language.Korean] = row[koIdx];
                if (enIdx >= 0 && enIdx < row.Count) table[key][Language.English] = row[enIdx];
                if (zhIdx >= 0 && zhIdx < row.Count) table[key][Language.Chinese] = row[zhIdx];
                if (jaIdx >= 0 && jaIdx < row.Count) table[key][Language.Japanese] = row[jaIdx];
            }
        }

        Debug.Log($"Localization Loaded: {table.Count} keys.");
    }

    // 언어 변경 (UI용)
    public static void SetLanguage(Language lang)
    {
        CurrentLanguage = lang;
        OnLanguageChanged?.Invoke();
    }

    // 키로 번역 가져오기 (fallback 사용)
    public static string Get(string key, string fallback = "")
    {
        if (string.IsNullOrEmpty(key)) return fallback;

        if (table.TryGetValue(key, out var perLang))
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
}
