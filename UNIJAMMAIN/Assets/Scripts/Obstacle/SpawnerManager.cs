using System.Collections;
using UnityEngine;
using static EnemyTypeSO;
using static EnemyTypeSO2;
using static GamePlayDefine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] float firstPhaseDuration = 5f;
    [SerializeField] float secondPhaseDuration = 5f;
    [SerializeField] float thirdPhaseDuration = 2f;
    [SerializeField] float testData = 0;

    [SerializeField] private float initialInterval = 1f; // �ʱ� ���� ����(��)

    private float currentInterval;
    private AttackType enemyType;

    private void OnEnable()
    {
        currentInterval = initialInterval;
        StartCoroutine(PhaseRoutine());
    }

    private IEnumerator PhaseRoutine()
    {
        float[] phases = { firstPhaseDuration, secondPhaseDuration, thirdPhaseDuration };

        foreach (float phaseDuration in phases)
        {
            yield return StartCoroutine(RunPhase(phaseDuration));
        }
    }

    private IEnumerator RunPhase(float phaseDuration)
    {
        float remainingTime = phaseDuration;

        while (remainingTime > 0)
        {
            // ������ ���ݸ��� ��带 ����
            yield return new WaitForSeconds(currentInterval);
            InitiateRandomNode();

            // ���� �ð� ������Ʈ
            remainingTime -= currentInterval;
            testData = remainingTime;
        }
    }

    private void InitiateRandomNode()
    {
        enemyType = (AttackType)Random.Range(0, (int)AttackType.MaxCnt);
        EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);
        GameObject go = Instantiate(enemy.go, enemy.pos, Quaternion.identity);
        go.GetComponent<MovingEnemy>();
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
