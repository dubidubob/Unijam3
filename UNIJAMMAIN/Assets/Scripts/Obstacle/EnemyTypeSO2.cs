using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPrefabSO2", menuName = "SO/EnemyPrefabSO2")]
public class EnemyTypeSO2 : ScriptableObject
{
    [System.Serializable]
    public struct EnemyData2
    { 
        public GamePlayDefine.RangedAttackType AttackType;
        public GameObject go;
        public Vector3 pos;
    }

    [SerializeField] private List<EnemyData2> enemies;

    public EnemyData2 GetEnemies(GamePlayDefine.RangedAttackType attack)
    {
        EnemyData2 go = new EnemyData2();

        foreach (EnemyData2 enemy in enemies)
        {
            if (enemy.AttackType == attack)
                go = enemy;
        }

        return go;
    }
}
