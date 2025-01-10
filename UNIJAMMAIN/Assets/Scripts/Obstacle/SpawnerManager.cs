using System.Collections;
using UnityEngine;
using static EnemyTypeSO;
using static GamePlayDefine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    AttackType enemyType;

    void Start()
    {   
        InitiateRandomNode();
        InitiateRandomNode();
        InitiateRandomNode();
        InitiateRandomNode();
    }
    
    private void InitiateRandomNode()
    {
        enemyType = (AttackType)Random.Range(0, (int)AttackType.MaxCnt);
        EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);
        GameObject go = Instantiate(enemy.go, enemy.pos, Quaternion.identity);
        Debug.Log($"instantiate {enemyType}");
    }
}
