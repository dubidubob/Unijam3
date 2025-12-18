using UnityEditor;
using UnityEngine;

// Assets/Editor/ChapterSOEditor.cs
#if UNITY_EDITOR

[CustomEditor(typeof(ChapterSO))]
public class ChapterSOEditor : Editor
{
    SerializedProperty musicPath;
    SerializedProperty startbeat;
    SerializedProperty delayPaddingTick;
     SerializedProperty phases;
    SerializedProperty backGroundSprite; // Sprite 타입 필드
    SerializedProperty backGroundGraySprite; // Sprite 타입 필드
    SerializedProperty colorPalette;
    SerializedProperty sinkStage;

    void OnEnable()
    {
        musicPath = serializedObject.FindProperty("MusicPath");
        phases = serializedObject.FindProperty("phases");
        startbeat = serializedObject.FindProperty("StartBeat");
        delayPaddingTick = serializedObject.FindProperty("DelayPaddingTick");
        backGroundSprite = serializedObject.FindProperty("backGroundSprite"); // 필드 이름과 일치하도록 수정
        backGroundGraySprite = serializedObject.FindProperty("backGroundGraySprite"); // 필드 이름과 일치하도록 수정
        colorPalette = serializedObject.FindProperty("colorPalette");
        sinkStage = serializedObject.FindProperty("SinkStage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(musicPath);
        EditorGUILayout.PropertyField(startbeat);
        EditorGUILayout.PropertyField(delayPaddingTick);
        EditorGUILayout.PropertyField(phases, true);
        EditorGUILayout.PropertyField(backGroundSprite); // 인스펙터에 Sprite 필드 표시
        EditorGUILayout.PropertyField(backGroundGraySprite); // 인스펙터에 Sprite 필드 표시
        EditorGUILayout.PropertyField(colorPalette);
        EditorGUILayout.PropertyField(sinkStage);

        EditorGUILayout.Space(8);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add PhaseEvent"))
                AddNew<PhaseEvent>();

            if (GUILayout.Button("Add TutorialEvent"))
                AddNew<TutorialEvent>();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void AddNew<T>() where T : class, new()
    {
        int i = phases.arraySize;
        phases.arraySize++;
        var elem = phases.GetArrayElementAtIndex(i);
        elem.managedReferenceValue = new T();   // ✅ 구체 타입 즉시 주입
    }
}
#endif