using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class LocalizationExtractor : Editor
{
    [MenuItem("Tools/Extract All Story Canvases to CSV")]
    public static void ExtractAllToCSV()
    {
        // 1. 프리팹들이 있는 폴더 경로
        string folderPath = "Assets/Resources/Prefabs/Story";
        // 해당 폴더 내의 모든 프리팹 GUID 가져오기
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        if (guids.Length == 0) { Debug.LogError("프리팹을 찾을 수 없습니다! 경로를 확인하세요."); return; }

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Key,Korean(ko),Chinese (Simplified)(zh-Hans),English(en),Japanese (ja)");

        HashSet<string> addedCharKeys = new HashSet<string>();
        List<string> charLines = new List<string>();
        List<string> dialogLines = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = Path.GetFileNameWithoutExtension(path);

            // [중요] 프리팹 수정을 위해 LoadPrefabContents 사용 (2018.3+ 권장 방식)
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            StoryDialog source = prefabRoot.GetComponent<StoryDialog>();

            if (source == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                continue;
            }

            Debug.Log($"처리 중: {prefabName}");

            for (int i = 0; i < source.scenes.Count; i++)
            {
                DialogueScene scene = source.scenes[i];

                // A. 인물 이름 키 추출 및 생성
                if (scene.speakingCharacterData != null)
                {
                    string charKey = $"NAME_{scene.speakingCharacterData.name}";
                    if (!addedCharKeys.Contains(charKey))
                    {
                        charLines.Add($"{charKey},\"{scene.speakingCharacterData.CharacterName}\",,,");
                        addedCharKeys.Add(charKey);
                    }
                }

                // B. 대사 텍스트 키 추출 및 프리팹에 저장
                // 키 형식: STORY_0_StoryCanvas_000
                string dialogKey = $"STORY_{prefabName}_{i:D3}";
                scene.localizationKey = dialogKey;

                string cleanText = string.IsNullOrEmpty(scene.text) ? "" : scene.text.Replace("\"", "\"\"");
                dialogLines.Add($"{dialogKey},\"{cleanText}\",,,");
            }

            // 프리팹 변경사항 저장 및 언로드
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        // CSV 조립
        foreach (var line in charLines) csv.AppendLine(line);
        foreach (var line in dialogLines) csv.AppendLine(line);

        // 결과 저장 (Resources/Localization 폴더가 있어야 함)
        string dirPath = Path.Combine(Application.dataPath, "Resources/Localization");
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        string savePath = Path.Combine(dirPath, "StoryDialogue.csv");
        File.WriteAllText(savePath, csv.ToString(), Encoding.UTF8);

        AssetDatabase.Refresh();
        Debug.Log($"[추출 완료] 총 {guids.Length}개의 프리팹 처리됨. 경로: {savePath}");
    }
}