using System.Collections.Generic;
using UnityEngine;

// TODO : Interface로 monster type 만들기!
[CreateAssetMenu(fileName = "EnemyPrefabSO", menuName = "SO/EnemyPrefabSO")]
public class MonsterTypeSO : ScriptableObject
{
    [System.Serializable]
    public struct MonsterData
    { 
        public GamePlayDefine.WASDType AttackType;
        public GameObject go;
        public Vector3 pos;
    }

    [SerializeField] private List<MonsterData> enemies;

    public MonsterData GetEnemies(GamePlayDefine.WASDType attack)
    {
        MonsterData monster = new MonsterData();

        foreach (MonsterData enemy in enemies)
        {
            if (enemy.AttackType == attack)
                monster = enemy;
        }

        return monster;
    }
}
