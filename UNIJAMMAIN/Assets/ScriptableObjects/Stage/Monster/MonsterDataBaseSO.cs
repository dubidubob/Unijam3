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
        public Sprite attackedSprite;
    }

    public Sprite dyingEffectSprite;
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
    
    public Sprite GetSprite(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var data))
            return data.sprite;

        Debug.LogWarning($"[MonsterDatabaseSO] {type}에 해당하는 Sprite가 없습니다.");
        return null;
    }

    public Color GetColor(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var data))
            return data.color;

        return Color.white;
    }

    public Sprite GetAttackedSprite(Define.MonsterType type)
    {
        if (_dict != null && _dict.TryGetValue(type, out var data))
            return data.attackedSprite;

        Debug.LogWarning($"[MonsterDatabaseSO] {type}에 해당하는 Sprite가 없습니다.");
        return null;

    }

    public Sprite DyingEffectSprite()
    {

        return dyingEffectSprite;
    }
}