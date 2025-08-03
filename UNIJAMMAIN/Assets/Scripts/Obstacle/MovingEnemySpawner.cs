using UnityEngine;
using static GamePlayDefine;

public class MovingEnemySpawner : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    private MovingAttackType enemyType;
    private MovingEnemy movingEnemy;
    private Poolable poolable;

    public void InitiateRandomNode(float movingDuration)
    {
        enemyType = (MovingAttackType)Random.Range(0, (int)MovingAttackType.MaxCnt);
        EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);

        poolable = Managers.Pool.Pop(enemy.go);
        poolable.gameObject.transform.position = new Vector3(enemy.pos.x, enemy.pos.y, 0);

        movingEnemy = poolable.gameObject.GetComponent<MovingEnemy>();
        movingEnemy.SetSpeed(movingDuration);
    }
}
