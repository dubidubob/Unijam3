using UnityEngine;
using UnityEditor;

// �� Drawer�� MonsterData Ÿ���� ���� ������ Unity�� �˷��ݴϴ�.
[CustomPropertyDrawer(typeof(MonsterData))]
public class MonsterDataDrawer : PropertyDrawer
{
    // �ν����Ϳ� UI�� �׸��� ���� �Լ��Դϴ�.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // �� ���� ���̸� �̸� ����մϴ�.
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        // �ʵ� ������ ����
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // ���� Y ��ġ
        float currentY = position.y;

        // isin, monsterType �ʵ�� �׻� ���̵��� �׸��ϴ�.
        // �� ������Ƽ(����)�� �̸����� ã�ƿɴϴ�.
        var isInProp = property.FindPropertyRelative("isIn");
        var monsterTypeProp = property.FindPropertyRelative("monsterType");

        // UI�� �׸� �簢�� ������ �����ϰ� ������Ƽ �ʵ带 �׸��ϴ�.
        Rect isInRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(isInRect, isInProp);
        currentY += singleLineHeight + spacing;

        Rect monsterTypeRect = new Rect(position.x, currentY, position.width, singleLineHeight);
        EditorGUI.PropertyField(monsterTypeRect, monsterTypeProp);
        currentY += singleLineHeight + spacing;

        //  --- ���Ⱑ �ٽ� ���� ---
        // monsterType enum�� ���� ���õ� ���� �����ɴϴ�.
        Define.MonsterType selectedType = (Define.MonsterType)monsterTypeProp.enumValueIndex;

        // ���� ���õ� Ÿ�Կ� ����, �ʵ带 �׸��ϴ�.
        if (selectedType == Define.MonsterType.WASD
            || selectedType == Define.MonsterType.WASDDash
            || selectedType == Define.MonsterType.WASDFIFO
            || selectedType == Define.MonsterType.WASDHiding
            || selectedType == Define.MonsterType.Knockback
            )
        {
            var bossNameProp = property.FindPropertyRelative("WASD_Pattern");
            Rect bossNameRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(bossNameRect, bossNameProp);
            currentY += singleLineHeight + spacing;
        }

        // ������ �ʵ���� �׸��ϴ�.
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

    // �������� �ʵ尡 �߰�/���ŵǹǷ� ��ü ���̸� �ٽ� �������� �մϴ�.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = 0;
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // �⺻������ 6���� �ʵ尡 �����Ƿ� ���̸� ����մϴ�.
        totalHeight = (singleLineHeight * 6) + (spacing * 5);

        var monsterTypeProp = property.FindPropertyRelative("monsterType");
        Define.MonsterType selectedType = (Define.MonsterType)monsterTypeProp.enumValueIndex;

        // ���� Boss Ÿ���� ���õǾ��ٸ�, bossName �ʵ��� ���̸� �߰��մϴ�.
        if (selectedType == Define.MonsterType.WASD)
        {
            totalHeight += singleLineHeight + spacing;
        }

        return totalHeight;
    }
}