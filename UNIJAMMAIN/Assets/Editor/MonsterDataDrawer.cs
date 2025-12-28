using UnityEngine;
using UnityEditor;

// 이 Drawer가 MonsterData 타입을 위한 것임을 Unity에 알려줍니다.
[CustomPropertyDrawer(typeof(MonsterData))]
public class MonsterDataDrawer : PropertyDrawer
{
    // 인스펙터에 UI를 그리는 메인 함수입니다.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 한 줄의 높이를 미리 계산합니다.
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        // 필드 사이의 간격
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // 현재 Y 위치
        float currentY = position.y;

        // isin, monsterType 필드는 항상 보이도록 그립니다.
        // 각 프로퍼티(변수)를 이름으로 찾아옵니다.
        var isInProp = property.FindPropertyRelative("isIn");
        var monsterTypeProp = property.FindPropertyRelative("monsterType");

        // UI를 그릴 사각형 영역을 설정하고 프로퍼티 필드를 그립니다.
        Rect isInRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(isInRect, isInProp);
        currentY += singleLineHeight + spacing;

        Rect monsterTypeRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(monsterTypeRect, monsterTypeProp);
        currentY += singleLineHeight + spacing;

        //  --- 여기가 핵심 로직 ---
        // monsterType enum의 현재 선택된 값을 가져옵니다.
        Define.MonsterType selectedType = (Define.MonsterType)monsterTypeProp.enumValueIndex;

        // 만약 선택된 타입에 따라, 필드를 그립니다.
        if (selectedType == Define.MonsterType.WASD
            || selectedType == Define.MonsterType.WASDDash
            || selectedType == Define.MonsterType.WASDFIFO
            || selectedType == Define.MonsterType.WASDHiding
            || selectedType == Define.MonsterType.Knockback
            || selectedType == Define.MonsterType.Diagonal
            )
        {
            var bossNameProp = property.FindPropertyRelative("WASD_Pattern");
            Rect bossNameRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(bossNameRect, bossNameProp);
            currentY += singleLineHeight + spacing;
        }

        // 나머지 필드들을 그립니다.
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

    // 동적으로 필드가 추가/제거되므로 전체 높이를 다시 계산해줘야 합니다.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = 0;
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // 기본적으로 6개의 필드가 있으므로 높이를 계산합니다.
        totalHeight = (singleLineHeight * 6) + (spacing * 5);

        var monsterTypeProp = property.FindPropertyRelative("monsterType");
        Define.MonsterType selectedType = (Define.MonsterType)monsterTypeProp.enumValueIndex;

        // 만약 Boss 타입이 선택되었다면, bossName 필드의 높이를 추가합니다.
        if (selectedType == Define.MonsterType.WASD)
        {
            totalHeight += singleLineHeight + spacing;
        }

        return totalHeight;
    }
}