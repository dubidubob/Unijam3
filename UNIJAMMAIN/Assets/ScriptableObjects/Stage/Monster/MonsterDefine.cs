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

    // ====== 정적 관리 부분 ======
    private static Dictionary<Define.MonsterType, MonsterDefine> _dict;

    // 초기화 함수 (게임 시작 시 한 번만 실행)
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

        Debug.LogWarning($"[MonsterDefine] {type}에 해당하는 Sprite가 없습니다.");
        return null;
    }

    public static Color GetColor(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var def))
            return def.color;

        return Color.white;
    }
}
