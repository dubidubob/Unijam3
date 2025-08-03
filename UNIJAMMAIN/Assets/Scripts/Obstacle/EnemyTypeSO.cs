using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPrefabSO", menuName = "SO/EnemyPrefabSO")]
public class EnemyTypeSO : ScriptableObject
{
    [System.Serializable]
    public struct EnemyData
    { 
        public GamePlayDefine.MovingAttackType AttackType;
        public GameObject go;
        public Vector3 pos;
    }

    [SerializeField] private List<EnemyData> enemies;

    public EnemyData GetEnemies(GamePlayDefine.MovingAttackType attack)
    {
        EnemyData go = new EnemyData();

        foreach (EnemyData enemy in enemies)
        {
            if (enemy.AttackType == attack)
            {
                go = enemy;
            }
        }

        return go;
    }
}
