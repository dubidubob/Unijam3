using UnityEngine;
using static GamePlayDefine;

public class WASDMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    private MovingEnemy movingEnemy;
    private Poolable poolable;

    Define.MonsterType ISpawnable.MonsterType => Define.MonsterType.WASD;

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
