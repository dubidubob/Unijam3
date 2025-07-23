using UnityEngine;
using static GamePlayDefine;

public class MovingEnemySpawner : MonoBehaviour
{
    [SerializeField] MonsterTypeSO enemyTypeSO;
    [SerializeField] private float upDownDebuf = 0.8f;
    private WASDType enemyType;
    private MovingEnemy movingEnemy;
    private Poolable poolable;

    public void InitiateRandomNode(float currentSpeed)
    {
        enemyType = (WASDType)Random.Range(0, (int)WASDType.MaxCnt);
        MonsterTypeSO.MonsterData enemy = enemyTypeSO.GetEnemies(enemyType);

        poolable = Managers.Pool.Pop(enemy.go);
        poolable.gameObject.transform.position = new Vector3(enemy.pos.x, enemy.pos.y, 0);

        currentSpeed = ApplyDebufUpdown(currentSpeed);
        movingEnemy = poolable.gameObject.GetComponent<MovingEnemy>();
        movingEnemy.SetSpeed(currentSpeed);
    }

    private float ApplyDebufUpdown(float currentSpeed)
    {
        if (enemyType == WASDType.W || enemyType == WASDType.S)
        {
            return currentSpeed *= upDownDebuf;
        }
        return currentSpeed;
    }
}
