using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MovingEnemySpawner))]
[RequireComponent(typeof(RangedEnemyActivater))]

public class SpawnController : MonoBehaviour
{
    private MovingEnemySpawner movingEnemySpawner;
    private RangedEnemyActivater rangedEnemyActivater;

    private class PhaseMoving
    {
        //gimic1
        public float movingDefaultSpeed;
        public float movingTargetSpeed;
        public float movingPhaseDuration;
        public float movingIntervalDecRate;
    }

    private class PhaseRanged : PhaseMoving
    {
        //gimic2
        public float rangedDefaultLifetime;
        public float rangedTargetLifetime;
        public float rangedIntervalDecRate;
    }

    private class PhaseMouse : PhaseRanged
    {
        //gimic3
        public bool canMouseInputTwo;
        public float mouseIntervalDecRate;
    }

    PhaseMoving phase1 = new PhaseMoving();
    PhaseRanged phase2 = new PhaseRanged();
    PhaseMouse phase3 = new PhaseMouse();

    private void InitPhases()
    {
        phase1.movingPhaseDuration = 12f;
        phase1.movingDefaultSpeed = 2f;
        phase1.movingTargetSpeed = 2.5f;
        phase1.movingIntervalDecRate = 0.1f;

        phase2.movingPhaseDuration = 12f;
        phase2.movingDefaultSpeed = phase1.movingTargetSpeed; //2.5
        phase2.movingTargetSpeed = 3f;
        phase2.movingIntervalDecRate = 0.1f;
        phase2.rangedDefaultLifetime = 3f;
        phase2.rangedTargetLifetime = 2f;
        phase2.rangedIntervalDecRate = 0.1f;

        phase3.movingPhaseDuration = 6f;
        phase3.movingDefaultSpeed = phase2.movingTargetSpeed; //3
        phase3.movingTargetSpeed = 3.5f;
        phase3.movingIntervalDecRate = 0.1f;
        phase3.rangedDefaultLifetime = phase2.rangedTargetLifetime; //2
        phase3.rangedTargetLifetime = 1f;
        phase3.rangedIntervalDecRate = 0.1f;
        phase3.canMouseInputTwo = false;
        phase3.mouseIntervalDecRate = 0.1f;
    }

    [SerializeField] private float initialInterval = 1.5f;

    private float currentMovingInterval;
    private float currentRangedInterval;
    
    private float movingTimeElapsed = 0f;    // �ӵ��� �����ϴ� �� ���� ���� �ð�
    private float rangedTimeElapsed = 0f;

    private float currentSpeed;        // �� ���� ������ ������ ���� �ӵ�
    private float currentLifetime;
    
    private void Awake()
    {
        // �ʿ��� ������Ʈ ��������
        movingEnemySpawner = GetComponent<MovingEnemySpawner>();
        rangedEnemyActivater = GetComponent<RangedEnemyActivater>();
    }

    private void Start()
    {
        InitPhases();
        currentMovingInterval = initialInterval;
        currentRangedInterval = initialInterval;
        currentSpeed = phase1.movingDefaultSpeed;
        StartCoroutine(PhaseRoutine());
    }

    private IEnumerator PhaseRoutine()
    {
        PhaseMoving[] phases = { phase1, phase2, phase3 };

        for(int i = 1; i<=phases.Length; i++)
        {
            Debug.Log($"phase {i} start!");
            yield return StartCoroutine(RunPhase(phases[i-1], i));
            Debug.Log($"phase {i} end!");
            //illust thing
            //yield return new WaitForSeconds(2f); // for illust
        }
    }

    private IEnumerator RunPhase(PhaseMoving phase, int phaseNum)
    {
        if (phaseNum >= 3)//phase 3
            StartCoroutine(RunTouch((PhaseMouse)phase));
        if (phaseNum >= 2)//phase 2, 3
            StartCoroutine(RunRanged((PhaseRanged)phase));
        if (phaseNum >= 1)//phase 1, 2, 3
            StartCoroutine(RunMoving((PhaseMoving)phase));

        yield return new WaitForSeconds(phase.movingPhaseDuration);
    }

    private IEnumerator RunTouch(PhaseMouse phase) { yield return null; }
    private IEnumerator RunRanged(PhaseRanged phase) 
    {
        float remainingTime = phase.movingPhaseDuration;
        bool isDecIntervalChecked = false;

        while (remainingTime > 0)
        {
            if (remainingTime < phase.movingPhaseDuration * 0.5 && !isDecIntervalChecked) //phase �ð��� ���� �������� ������ �ش�.
            {
                currentRangedInterval -= phase.rangedIntervalDecRate;
                isDecIntervalChecked = true;
            }

            yield return new WaitForSeconds(currentRangedInterval);
            currentLifetime = UpdateCurrentData(rangedTimeElapsed, phase.movingPhaseDuration, phase.rangedDefaultLifetime, phase.rangedTargetLifetime);
            GetRangedEnemy(currentLifetime);

            remainingTime -= currentMovingInterval;
        }
    }
    private IEnumerator RunMoving(PhaseMoving phase)
    {
        float remainingTime = phase.movingPhaseDuration;
        bool isDecIntervalChecked = false;

        while (remainingTime > 0)
        {
            if (remainingTime < phase.movingPhaseDuration * 0.5 && !isDecIntervalChecked) //phase �ð��� ���� �������� ������ �ش�.
            {
                currentMovingInterval -= phase.movingIntervalDecRate;
                isDecIntervalChecked = true;
            }

            yield return new WaitForSeconds(currentMovingInterval);
            currentSpeed = UpdateCurrentData(movingTimeElapsed, phase.movingPhaseDuration, phase.movingDefaultSpeed, phase.movingTargetSpeed);
            GetMovingEnemy(currentSpeed);

            remainingTime -= currentMovingInterval;
        }
    }

    private float UpdateCurrentData(float timeElapsed, float timeToTargetData, float defaultData, float targetData)
    {
        if (timeElapsed < timeToTargetData)
        {
            timeElapsed += currentMovingInterval;

            float t = Mathf.Clamp01(timeElapsed / timeToTargetData);
            return Mathf.Lerp(defaultData, targetData, t);
        }

        return targetData;
    }

    private void GetMovingEnemy(float currentSpeed)
    {
        movingEnemySpawner.InitiateRandomNode(currentSpeed);
    }

    private void GetRangedEnemy(float currentLifetime)
    {
        rangedEnemyActivater.ActivateEnemy(currentLifetime);
    }
}

