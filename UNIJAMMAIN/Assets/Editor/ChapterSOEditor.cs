using UnityEditor;
using UnityEngine;

// Assets/Editor/ChapterSOEditor.cs
#if UNITY_EDITOR

[CustomEditor(typeof(ChapterSO))]
public class ChapterSOEditor : Editor
{
    SerializedProperty musicPath;
    SerializedProperty phases;

    void OnEnable()
    {
        musicPath = serializedObject.FindProperty("MusicPath");
        phases = serializedObject.FindProperty("phases");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(musicPath);
        EditorGUILayout.PropertyField(phases, true);

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