using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MovingEnemySpawner))]
[RequireComponent(typeof(RangedEnemyActivater))]
[RequireComponent(typeof(MouseEnemyActivater))]

public class SpawnController : MonoBehaviour
{
    public int testMoving = 0;
    public int testRanged = 0;
    public int testTouch = 0;
    public IllustController illustController;
    [SerializeField] bool isMaster = true;
    #region PhaseClasses
    public class PhaseMoving
    {
        //gimic1
        public float movingDefaultSpeed;
        public float movingTargetSpeed;
        public float movingPhaseDuration;
        public float movingIntervalDecRate;
    }

    public class PhaseRanged : PhaseMoving
    {
        //gimic2
        public float rangedDefaultLifetime;
        public float rangedTargetLifetime;
        public float rangedIntervalDecRate;
    }

    public class PhaseMouse : PhaseRanged
    {
        //gimic3
        public int cntMouseInputTwo;
        public float mouseIntervalDecRate;
    }
    #endregion

    public PhaseMoving phase1 = new PhaseMoving();
    public PhaseRanged phase2 = new PhaseRanged();
    public PhaseMouse phase3 = new PhaseMouse();

    private void InitPhases()
    {
        phase1.movingPhaseDuration = 6f;
        phase1.movingDefaultSpeed = 2f;
        phase1.movingTargetSpeed = 2.5f;
        phase1.movingIntervalDecRate = 0.1f;

        phase2.movingPhaseDuration = 18f;
        phase2.movingDefaultSpeed = phase1.movingTargetSpeed; //2.5
        phase2.movingTargetSpeed = 3f;
        phase2.movingIntervalDecRate = 0.1f;
        phase2.rangedDefaultLifetime = 3f;
        phase2.rangedTargetLifetime = 2f;
        phase2.rangedIntervalDecRate = 0.1f;

        phase3.movingPhaseDuration = 12f;
        phase3.movingDefaultSpeed = phase2.movingTargetSpeed; //3
        phase3.movingTargetSpeed = 3.5f;
        phase3.movingIntervalDecRate = 0.1f;
        phase3.rangedDefaultLifetime = phase2.rangedTargetLifetime; //2
        phase3.rangedTargetLifetime = 1f;
        phase3.rangedIntervalDecRate = 0.1f;
        phase3.cntMouseInputTwo = 2;
        phase3.mouseIntervalDecRate = 0.1f;
    }

    [HideInInspector]
    public float initialInterval = 1.5f;
    [HideInInspector]
    public float rangeDebuf = 0.8f;
    [HideInInspector]
    public float updownDebuf = 0.7f;

    private MovingEnemySpawner movingEnemySpawner;
    private RangedEnemyActivater rangedEnemyActivater;
    private MouseEnemyActivater mouseEnemyActivater;

    private float currentMovingInterval;
    private float currentRangedInterval;
    private float currentMouseInterval;
    
    private float movingTimeElapsed = 0f;    // 속도가 증가하는 데 사용될 누적 시간
    private float rangedTimeElapsed = 0f;

    private float currentSpeed;        // 각 스폰 시점에 적용할 현재 속도
    private float currentLifetime;
    private float currentMouseActivateCnt;

    private bool isPaused = false;
    private void Awake()
    {
        // 필요한 컴포넌트 가져오기
        movingEnemySpawner = GetComponent<MovingEnemySpawner>();
        rangedEnemyActivater = GetComponent<RangedEnemyActivater>();
        mouseEnemyActivater = GetComponent<MouseEnemyActivater>();

        Managers.Input.SettingpopAction -= ControlTime;
        Managers.Input.SettingpopAction += ControlTime;
        Managers.Game.HealthUpdate -= CheckDie;
        Managers.Game.HealthUpdate += CheckDie;
    }

    private void Start()
    {
        InitPhases();
        currentMovingInterval = initialInterval;
        currentRangedInterval = initialInterval;
        currentMouseInterval = initialInterval * 3f;

        currentSpeed = phase1.movingDefaultSpeed;
        currentLifetime = phase2.rangedDefaultLifetime;
        currentMouseActivateCnt = phase3.cntMouseInputTwo;
        StartCoroutine(PhaseRoutine());
    }

    //private void Clear()
    //{
    //    StopAllCoroutines(); //corountine clear
    //    Managers.Pool.Clear(); //moving enemy clear
    //    foreach (Transform child in this.transform)
    //    { 
    //        child.gameObject.SetActive(false);
    //    }
    //}

    private void CheckDie(int health)
    {
        if (health <= 0 && !isMaster)
        {
            Pause();
            illustController.ShowIllust(GamePlayDefine.IllustType.Fail);

            Managers.UI.ShowPopUpUI<GameOver>();
            return;
        }
    }

    public void InitAgain()
    {
        Resume();
        
        movingTimeElapsed = 0f;    // 속도가 증가하는 데 사용될 누적 시간
        rangedTimeElapsed = 0f;

        currentMovingInterval = initialInterval;
        currentRangedInterval = initialInterval;
        currentMouseInterval = initialInterval*0.25f;

        currentSpeed = phase1.movingDefaultSpeed;
        currentLifetime = phase2.rangedDefaultLifetime;
        currentMouseActivateCnt = phase3.cntMouseInputTwo;
        StartCoroutine(PhaseRoutine());
    }

    #region ControlTime
    private void ControlTime()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }
    #endregion

    private IEnumerator PhaseRoutine()
    {
        PhaseMoving[] phases = { phase1, phase2, phase3 };

        for(int i = 1; i<=phases.Length; i++)
        {
            if (i == 1)
            {
                Pause();
                yield return illustController.ShowIllust(GamePlayDefine.IllustType.Num);
                Resume();
            }
            Debug.LogWarning($"phase {i} start!");
            Managers.Game.IncPhase();
            movingTimeElapsed = 0f;    // 속도가 증가하는 데 사용될 누적 시간
            rangedTimeElapsed = 0f;
            yield return StartCoroutine(RunPhase(phases[i-1], i));
            Debug.LogWarning($"phase {i} end!");
            Pause();
            if (i == 1)
            {
                yield return illustController.ShowIllust(GamePlayDefine.IllustType.Phase1End);
                Resume();
            }
            else if (i == 2)
            {
                yield return illustController.ShowIllust(GamePlayDefine.IllustType.Phase2End);
                Resume();
            }
            else
            {
                yield return illustController.ShowIllust(GamePlayDefine.IllustType.Success);
                break;
            }
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

    private IEnumerator RunTouch(PhaseMouse phase) 
    {
        float remainingTime = phase.movingPhaseDuration;
        bool isDecIntervalChecked = false;
        int cnt = phase.cntMouseInputTwo;

        while (remainingTime > 0)
        {
            if (remainingTime < phase.movingPhaseDuration * 0.5 && !isDecIntervalChecked) //phase 시간의 반이 지나가면 간격이 준다.
            {
                currentMouseInterval -= phase.mouseIntervalDecRate;
                isDecIntervalChecked = true;
            }

            yield return new WaitForSeconds(currentMouseInterval);
            if(cnt > 0) cnt--;
            bool isTwhoOkay = cnt <= 0 ? true : false;
            testTouch++;
            Debug.Log($"different two okay? : {isTwhoOkay}, {testTouch}");
            GetMouseEnemy(isTwhoOkay);

            remainingTime -= currentMouseInterval;
        }
    }

    private IEnumerator RunRanged(PhaseRanged phase) 
    {
        float remainingTime = phase.movingPhaseDuration;
        bool isDecIntervalChecked = false;

        while (remainingTime > 0)
        {
            if (remainingTime < phase.movingPhaseDuration * 0.5 && !isDecIntervalChecked) //phase 시간의 반이 지나가면 간격이 준다.
            {
                currentRangedInterval -= phase.rangedIntervalDecRate;
                isDecIntervalChecked = true;
            }

            yield return new WaitForSeconds(currentRangedInterval);
            currentLifetime = UpdateCurrentRange(phase.movingPhaseDuration, phase.rangedDefaultLifetime, phase.rangedTargetLifetime);
            testRanged++;
            // Debug.Log($"different lifetime? : {currentLifetime}, {testRanged}");
            GetRangedEnemy(currentLifetime);

            remainingTime -= currentRangedInterval;
        }
    }
    private IEnumerator RunMoving(PhaseMoving phase)
    {
        float remainingTime = phase.movingPhaseDuration;
        bool isDecIntervalChecked = false;

        while (remainingTime > 0)
        {
            if (remainingTime < phase.movingPhaseDuration * 0.5 && !isDecIntervalChecked) //phase 시간의 반이 지나가면 간격이 준다.
            {
                currentMovingInterval -= phase.movingIntervalDecRate;
                isDecIntervalChecked = true;
            }

            yield return new WaitForSeconds(currentMovingInterval);
            currentSpeed = UpdateCurrentMoving(phase.movingPhaseDuration, phase.movingDefaultSpeed, phase.movingTargetSpeed);
            testMoving++;
            //Debug.Log($"different speed? : {currentSpeed}, {testMoving}");
            GetMovingEnemy(currentSpeed, rangeDebuf, updownDebuf);

            remainingTime -= currentMovingInterval;
        }
    }

    private float UpdateCurrentRange(float timeToTargetData, float defaultData, float targetData)
    {
        if (rangedTimeElapsed < timeToTargetData)
        {
            rangedTimeElapsed += currentRangedInterval;

            float t = Mathf.Clamp01(rangedTimeElapsed / timeToTargetData);
            return Mathf.Lerp(defaultData, targetData, t);
        }

        return targetData;
    }


    private float UpdateCurrentMoving(float timeToTargetData, float defaultData, float targetData)
    {
        if (movingTimeElapsed < timeToTargetData)
        {
            movingTimeElapsed += currentMovingInterval;

            float t = Mathf.Clamp01(movingTimeElapsed / timeToTargetData);
            return Mathf.Lerp(defaultData, targetData, t);
        }

        return targetData;
    }


    private void GetMovingEnemy(float currentSpeed, float rangeDebuf, float updownDebuf)
    {
        movingEnemySpawner.InitiateRandomNode(currentSpeed, rangeDebuf, updownDebuf);
    }

    private void GetRangedEnemy(float currentLifetime)
    {
        rangedEnemyActivater.ActivateEnemy(currentLifetime);
    }

    private void GetMouseEnemy(bool canTwo)
    {
        mouseEnemyActivater.ActivateRandomPanel(canTwo);
    }
}