using System.Collections;
using UnityEngine;
using static EnemyTypeSO;
using static GamePlayDefine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] float defaultTime = 20f;
    AttackType enemyType;

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
}
