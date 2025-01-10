using System.Collections;
using UnityEngine;
using static EnemyTypeSO;
using static GamePlayDefine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] float firstPhaseDuration = 5f;
    [SerializeField] float secondPhaseDuration = 5f;
    [SerializeField] float thirdPhaseDuration = 2f;
    [SerializeField] float testData = 0;

    [SerializeField] private float initialInterval = 1f; // 초기 공격 간격(초)

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
            // 지정된 간격마다 노드를 생성
            yield return new WaitForSeconds(currentInterval);
            InitiateRandomNode();

            // 남은 시간 업데이트
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
}
