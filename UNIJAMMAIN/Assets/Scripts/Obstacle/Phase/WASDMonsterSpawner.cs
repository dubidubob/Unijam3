using UnityEngine;
using static GamePlayDefine;

public class WASDMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    private MovingAttackType enemyType;
    private MovingEnemy movingEnemy;
    private Poolable poolable;

    public Define.MonsterType MonsterType => throw new System.NotImplementedException();

    // Deprecated
    public void InitiateRandomNode(float movingDuration)
    {
        var data = new MonsterData
        {
            monsterType = Define.MonsterType.WASD,
            moveToHolderDuration = movingDuration
        };
        Spawn(data);
    }

    public void Spawn(MonsterData data)
    {
        MovingAttackType enemyType = (MovingAttackType)Random.Range(0, (int)MovingAttackType.MaxCnt);
        EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);

        poolable = Managers.Pool.Pop(enemy.go);
        poolable.gameObject.transform.position = new Vector3(enemy.pos.x, enemy.pos.y, 0);

        movingEnemy = poolable.gameObject.GetComponent<MovingEnemy>();
        movingEnemy.SetSpeed(data.moveToHolderDuration);
    }
}
