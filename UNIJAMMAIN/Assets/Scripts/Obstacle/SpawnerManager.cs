using System.Collections;
using UnityEngine;
using static EnemyTypeSO;
using static EnemyTypeSO2;
using static GamePlayDefine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] EnemyTypeSO2 enemyTypeS02;
    [SerializeField] float defaultTime = 20f;
    AttackType enemyType;
    RangedAttackType rangedEnemyType;

    private void OnEnable()
    {
        StartCoroutine(TestPhase());
    }
    private IEnumerator TestPhase()
    {
        float curTime = defaultTime;
        while (defaultTime > 0) {
            yield return new WaitForSeconds(0.5f);
            InitiateRandomNode();
            InitiateRandomNode2();
            defaultTime--;
        }
    }

    private void InitiateRandomNode()
    {
        enemyType = (AttackType)Random.Range(0, (int)AttackType.MaxCnt);
        EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);
        GameObject go = Instantiate(enemy.go, enemy.pos, Quaternion.identity);
        Debug.Log($"instantiate {enemyType}");
    }
    private void InitiateRandomNode2()
    {
        rangedEnemyType = (RangedAttackType)Random.Range(0, (int)RangedAttackType.MaxCnt);
        EnemyData2 enemy = enemyTypeS02.GetEnemies(rangedEnemyType);
        GameObject go = Instantiate(enemy.go, enemy.pos, Quaternion.identity);
        Debug.Log($"instantiate {enemyType}");
    }
}
