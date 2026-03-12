using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MonsterData))]
public class MonsterDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float currentY = position.y;

        // 1. 공통 기본 필드 (isIn, monsterType)
        var isInProp = property.FindPropertyRelative("isIn");
        var monsterTypeProp = property.FindPropertyRelative("monsterType");

        Rect isInRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(isInRect, isInProp);
        currentY += singleLineHeight + spacing;

        Rect monsterTypeRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(monsterTypeRect, monsterTypeProp);
        currentY += singleLineHeight + spacing;

        // --- 조건부 로직 ---
        Define.MonsterType selectedType = (Define.MonsterType)monsterTypeProp.enumValueIndex;

        // 조건 1: MouseClick이 "아닐 때" (WASD_Pattern)
        if (selectedType != Define.MonsterType.MouseClick)
        {
            var wasdProp = property.FindPropertyRelative("WASD_Pattern");
            Rect wasdRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(wasdRect, wasdProp);
            currentY += singleLineHeight + spacing;
        }
        // 조건 2: MouseClick "일 때"
        else
        {
            // 2-1. dir
            var dirProp = property.FindPropertyRelative("dir");
            Rect dirRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(dirRect, dirProp);
            currentY += singleLineHeight + spacing;

            // 2-2. cameraActionDuration
            var cameraActionProp = property.FindPropertyRelative("cameraActionDuration");
            Rect cameraActionRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(cameraActionRect, cameraActionProp);
            currentY += singleLineHeight + spacing;

            // 2-3. floatDuration
            var floatDurationProp = property.FindPropertyRelative("floatDuration");
            Rect floatDurationRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(floatDurationRect, floatDurationProp);
            currentY += singleLineHeight + spacing;

            // 2-4. [신규 추가] waitForRespondBeat
            var waitBeatProp = property.FindPropertyRelative("waitForRespondBeat");
            Rect waitBeatRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(waitBeatRect, waitBeatProp);
            currentY += singleLineHeight + spacing;
        }

        // 3. 나머지 공통 필드
        var speedUpRateProp = property.FindPropertyRelative("speedUpRate");
        Rect speedUpRateRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(speedUpRateRect, speedUpRateProp);
        currentY += singleLineHeight + spacing;

        var spawnBeatProp = property.FindPropertyRelative("spawnBeat");
        Rect spawnBeatRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(spawnBeatRect, spawnBeatProp);
        currentY += singleLineHeight + spacing;

        var moveBeatProp = property.FindPropertyRelative("moveBeat");
        Rect moveBeatRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(moveBeatRect, moveBeatProp);
        currentY += singleLineHeight + spacing;

        var hidingProp = property.FindPropertyRelative("hiding");
        Rect hidingRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(hidingRect, hidingProp);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // 기본 공통 필드 개수 (isIn, monsterType, speedUpRate, spawnBeat, moveBeat, hiding) = 6개
        int fieldCount = 6;

        var monsterTypeProp = property.FindPropertyRelative("monsterType");
        if (monsterTypeProp != null)
        {
            Define.MonsterType selectedType = (Define.MonsterType)monsterTypeProp.enumValueIndex;

            if (selectedType != Define.MonsterType.MouseClick)
            {
                // WASD_Pattern (1개)
                fieldCount += 1;
            }
            else
            {
                // dir, cameraActionDuration, floatDuration, waitForRespondBeat (총 4개)
                fieldCount += 4;
            }
        }

        return (singleLineHeight * fieldCount) + (spacing * (fieldCount - 1));
    }
}