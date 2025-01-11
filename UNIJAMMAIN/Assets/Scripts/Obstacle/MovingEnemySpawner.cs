using System.Collections;
using UnityEngine;
using static GamePlayDefine;

public class MovingEnemySpawner : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] private float upDownDebuf = 0.8f;
    private MovingAttackType enemyType;
    private MovingEnemy movingEnemy;
    private Poolable poolable;

    public void InitiateRandomNode(float currentSpeed, float rangeDebuf, float updownDebuf)
    {
        enemyType = (MovingAttackType)Random.Range(0, (int)MovingAttackType.MaxCnt);
        EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);

        poolable = Managers.Pool.Pop(enemy.go);
        poolable.gameObject.transform.position = new Vector3(enemy.pos.x, enemy.pos.y, 0);

        upDownDebuf = updownDebuf;
        currentSpeed = ApplyDebufUpdown(currentSpeed);
        movingEnemy = poolable.gameObject.GetComponent<MovingEnemy>();
        movingEnemy.SetSpeed(currentSpeed, rangeDebuf);

        //GameObject go = Instantiate(enemy.go, enemy.pos, Quaternion.identity);
        Debug.Log($"instantiate {enemyType}");
    }

    private float ApplyDebufUpdown(float currentSpeed)
    {
        if (enemyType == MovingAttackType.W || enemyType == MovingAttackType.S)
        {
            return currentSpeed *= upDownDebuf;
        }
        return currentSpeed;
    }
}
