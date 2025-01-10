using System.Collections;
using UnityEngine;
using static GamePlayDefine;

public class MovingEnemySpawner : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] float firstPhaseDuration = 5f;
    [SerializeField] float secondPhaseDuration = 5f;
    [SerializeField] float thirdPhaseDuration = 2f;
    [SerializeField] float testData = 0;

    [SerializeField] private float initialInterval = 0.5f; // �ʱ� ���� ����(��)

    private float currentInterval;
    private MovingAttackType enemyType;

    private void Awake()
    {
        //for (int i = 0; i < (int)MovingAttackType.MaxCnt; i++)
        //{
        //    var enemy = enemyTypeSO.GetEnemies((MovingAttackType)i);
        //    Managers.Pool.CreatePool(enemy.go);
        //}
    }
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
            yield return new WaitForSeconds(currentInterval);
            InitiateRandomNode();

            remainingTime -= currentInterval;
            testData = remainingTime;
        }
    }

    private void InitiateRandomNode()
    {
        enemyType = (MovingAttackType)Random.Range(0, (int)MovingAttackType.MaxCnt);
        EnemyTypeSO.EnemyData enemy = enemyTypeSO.GetEnemies(enemyType);

        Poolable poolable = Managers.Pool.Pop(enemy.go);
        poolable.gameObject.transform.position = new Vector3(enemy.pos.x, enemy.pos.y, 0);

        //GameObject go = Instantiate(enemy.go, enemy.pos, Quaternion.identity);
        Debug.Log($"instantiate {enemyType}");
    }
}
