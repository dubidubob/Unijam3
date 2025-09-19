using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MonsterDefine
{
    public Define.MonsterType monsterType;
    public Color color;
    public Sprite sprite;

    // ====== ���� ���� �κ� ======
    private static Dictionary<Define.MonsterType, MonsterDefine> _dict;

    // �ʱ�ȭ �Լ� (���� ���� �� �� ���� ����)
    public static void Init(List<MonsterDefine> defines)
    {
        _dict = new Dictionary<Define.MonsterType, MonsterDefine>();
        foreach (var def in defines)
        {
            if (!_dict.ContainsKey(def.monsterType))
                _dict.Add(def.monsterType, def);
        }
    }

    public static Sprite GetSprite(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var def))
            return def.sprite;

        Debug.LogWarning($"[MonsterDefine] {type}�� �ش��ϴ� Sprite�� �����ϴ�.");
        return null;
    }

    public static Color GetColor(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var def))
            return def.color;

        return Color.white;
    }
}
