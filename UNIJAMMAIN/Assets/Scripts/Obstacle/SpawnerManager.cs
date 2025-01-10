using System.Collections;
using UnityEngine;
using static EnemyTypeSO;
using static GamePlayDefine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    AttackType enemyType;
    private 
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
        JustTestTransform(enemy.AttackType, go);
        Debug.Log($"instantiate {enemyType}");
    }

    private void JustTestTransform(AttackType attack, GameObject go)
    {
        string str = "";
        switch (attack)
        {
            case AttackType.arrowLeft:
                str = "LeftArrow";
                break;
            case AttackType.arrowRight:
                str = "RightArrow";
                break;
            case AttackType.arrowUp:
                str = "UpArrow";
                break;
            case AttackType.arrowDown:
                str = "DownArrow";
                break;
        }

        Managers.Game.AddAttackableEnemy(str, go);
    }
}
