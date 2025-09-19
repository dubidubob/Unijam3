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

    // ====== 정적 접근용 ======
    private static Dictionary<Define.MonsterType, MonsterData> _dict;

    // 초기화 함수 (한 번만 실행)
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

    // 정적 함수로 바로 접근 가능
    public static Sprite GetSprite(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var data))
            return data.sprite;

        Debug.LogWarning($"[MonsterDatabaseSO] {type}에 해당하는 Sprite가 없습니다.");
        return null;
    }

    public static Color GetColor(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var data))
            return data.color;

        return Color.white;
    }
}