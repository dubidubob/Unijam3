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

        // 1. 기본 필드 그리기 (isIn, monsterType)
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

        // 조건 1: MouseClick이 아닐 때 -> WASD_Pattern 표시
        if (selectedType != Define.MonsterType.MouseClick)
        {
            var bossNameProp = property.FindPropertyRelative("WASD_Pattern");
            Rect bossNameRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(bossNameRect, bossNameProp);
            currentY += singleLineHeight + spacing;
        }

        if (selectedType == Define.MonsterType.MouseClick)
        {

            var dirProp = property.FindPropertyRelative("dir");

            Rect dirRect = new Rect(position.x, currentY, position.width, singleLineHeight);

            EditorGUI.PropertyField(dirRect, dirProp);

            currentY += singleLineHeight + spacing;

        }


        // 조건 2: MouseClick 일 때 -> cameraActionDuration 표시
        if (selectedType == Define.MonsterType.MouseClick)
        {
            // MonsterData 클래스에 'cameraActionDuration' 변수가 있어야 합니다.
            var cameraActionProp = property.FindPropertyRelative("cameraActionDuration");

            Rect cameraActionRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(cameraActionRect, cameraActionProp);
            currentY += singleLineHeight + spacing;
        }

        // 2. 나머지 공통 필드 그리기
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

        // 기본 필드 6개 (isIn, monsterType, speedUpRate, spawnBeat, moveBeat, hiding)
        int fieldCount = 6;

        var monsterTypeProp = property.FindPropertyRelative("monsterType");
        if (monsterTypeProp != null)
        {
            Define.MonsterType selectedType = (Define.MonsterType)monsterTypeProp.enumValueIndex;

            // MouseClick이 아니면 WASD_Pattern 때문에 +1
            if (selectedType != Define.MonsterType.MouseClick)
            {
                fieldCount++;
            }
            // MouseClick 이면 cameraActionDuration 때문에 +1
            else if (selectedType == Define.MonsterType.MouseClick)
            {
                fieldCount++;
            }
        }

        return (singleLineHeight * fieldCount) + (spacing * (fieldCount - 1));
    }
}