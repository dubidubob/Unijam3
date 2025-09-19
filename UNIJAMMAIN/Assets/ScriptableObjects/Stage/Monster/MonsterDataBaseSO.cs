using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Monster/Database")]
public class MonsterDatabaseSO : ScriptableObject
{
    [System.Serializable]
    public class MonsterData
    {
        public Define.MonsterType monsterType;
        public Color color;
        public Sprite sprite;
    }

    public List<MonsterData> monsters;

    // ====== ���� ���ٿ� ======
    private static Dictionary<Define.MonsterType, MonsterData> _dict;

    // �ʱ�ȭ �Լ� (�� ���� ����)
    public void Init()
    {
        if (_dict != null) return;

        _dict = new Dictionary<Define.MonsterType, MonsterData>();
        foreach (var m in monsters)
        {
            if (!_dict.ContainsKey(m.monsterType))
                _dict.Add(m.monsterType, m);
        }
    }

    // ���� �Լ��� �ٷ� ���� ����
    public static Sprite GetSprite(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var data))
            return data.sprite;

        Debug.LogWarning($"[MonsterDatabaseSO] {type}�� �ش��ϴ� Sprite�� �����ϴ�.");
        return null;
    }

    public static Color GetColor(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var data))
            return data.color;

        return Color.white;
    }
}