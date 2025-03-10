using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MovingEnemySpawner))]
[RequireComponent(typeof(RangedEnemyActivater))]
[RequireComponent(typeof(MouseEnemyActivater))]

public class SpawnController : MonoBehaviour
{
    public IllustController illustController;
    [SerializeField] bool isMaster = true;
    [SerializeField] GameObject black;
    [SerializeField] GameObject balladang;
    [SerializeField] GameObject player;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject MainUI;
    [SerializeField] GameObject mouse;
    #region PhaseClasses
    public class PhaseMoving
    {
        //gimic1
        public float phaseDuration;

        public float movingDefaultSpeed;
        public float movingTargetSpeed;
        public float movingIntervalDecRate;

        public PhaseMoving(float phaseDuration, float movingDefaultSpeed, float movingTargetSpeed, float movingIntervalDecRate)
        {
            this.phaseDuration = phaseDuration;
            this.movingDefaultSpeed = movingDefaultSpeed;
            this.movingTargetSpeed = movingTargetSpeed;
            this.movingIntervalDecRate = movingIntervalDecRate;
        }
    }

    public class PhaseRanged : PhaseMoving
    {
        //gimic2
        public float rangedIntervalDecRate;

        public PhaseRanged(float phaseDuration, float movingDefaultSpeed, float movingTargetSpeed, float movingIntervalDecRate, float rangedIntervalDecRate)
            : base(phaseDuration, movingDefaultSpeed, movingTargetSpeed, movingIntervalDecRate) 
        {
            this.phaseDuration = phaseDuration;

            this.movingDefaultSpeed = movingDefaultSpeed;
            this.movingTargetSpeed = movingTargetSpeed;
            this.movingIntervalDecRate = movingIntervalDecRate;

            this.rangedIntervalDecRate = rangedIntervalDecRate;
        }
    }
    #endregion

    public PhaseMoving phase1 = new PhaseMoving(40f, 2f, 2.5f, 0.1f);
    public PhaseRanged phase2 = new PhaseRanged(40.5f, 2f, 3f, 0.1f, 0.1f);
    public PhaseRanged phase22 = new PhaseRanged(29.5f, 3f, 2.2f, 0.1f, 0.1f);
    public PhaseRanged phase3 = new PhaseRanged(50f, 2.2f, 2.7f, 0.1f, 0.1f);


    private MovingEnemySpawner movingEnemySpawner;
    private RangedEnemyActivater rangedEnemyActivater;
    private MouseEnemyActivater mouseEnemyActivater;

    private float currentMovingInterval;
    private float currentRangedInterval;

    private float initialInterval = 1.5f;
    private float movingTimeElapsed = 0f;
    private float rangedTimeElapsed = 0f;

    private float currentSpeed;        

    private bool isPaused = false;
    private bool NotDead = true;
    private void Awake()
    {
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
        currentMovingInterval = initialInterval;
        currentRangedInterval = initialInterval;

        currentSpeed = phase1.movingDefaultSpeed;
        pausePanel.SetActive(false);
        StartCoroutine(PhaseRoutine());
    }

    private void CheckDie(int health)
    {
        if (health <= 0 && !isMaster &&NotDead)
        {
            Pause();
            StopAllCoroutines();
            //애니메이션으로 죽어야함.
            player.SetActive(false);
            MainUI.SetActive(false);
            mouse.SetActive(false);
            Resume();
            balladang.SetActive(true);
            Managers.Sound.Play("Sounds/SFX/GameOver_Man_Falling");
            Managers.Sound.Stop(Managers.Sound._audioSources[(int)Define.Sound.BGM]);
            NotDead = false;
            StartCoroutine(Delay());

            return;
        }
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(2.5f);
        Managers.UI.ShowPopUpUI<GameOver>();
    }
    #region ControlTime
    private void ControlTime()
    {
        if (isPaused)
        {
            pausePanel.SetActive(false);
            Resume();
        }
        else
        {
            pausePanel.SetActive(true);
            Pause();
        }
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
        PhaseMoving[] phases = { phase1, phase2, phase22, phase3 };

        for(int i = 1; i<=phases.Length; i++)
        {
            if (i == 1)
            {
                Pause();
                if (illustController != null)
                {
                    Managers.UI.ShowPopUpUI<S1_PopUp>();
                    yield return new WaitForSecondsRealtime(6f);
                    yield return illustController.ShowIllust(GamePlayDefine.IllustType.Num);//숫자 나옴
                }
                Resume();
            }
            Debug.LogWarning($"phase {i} start!");

            if(i!=3)
                Managers.Game.IncPhase();

            movingTimeElapsed = 0f;   
            rangedTimeElapsed = 0f;

            yield return StartCoroutine(RunPhase(phases[i-1], i));

            Debug.LogWarning($"phase {i} end!");
            
            Pause();
            if (i == 4)
            {
                Managers.Scene.LoadScene("GoodEnding");
                break;
            }
            else if (i == 1)
            {
                Managers.UI.ShowPopUpUI<S2_PopUp>();
                yield return new WaitForSecondsRealtime(8f);
            }
            else if (i == 3)
            {
                Managers.UI.ShowPopUpUI<S3_PopUp>();
                yield return new WaitForSecondsRealtime(8f);
            }
            Resume();
        }
    }

    private IEnumerator RunPhase(PhaseMoving phase, int phaseNum)
    {
        if (phaseNum >= 4)//phase 4
            StartCoroutine(RunTouch((PhaseRanged)phase));
        if (phaseNum >= 2)//phase 2, 3, 4
            StartCoroutine(RunRanged((PhaseRanged)phase));
        if (phaseNum >= 1)//phase 1, 2, 3, 4
            StartCoroutine(RunMoving((PhaseMoving)phase));

        yield return new WaitForSeconds(phase.phaseDuration);
    }

    private IEnumerator RunTouch(PhaseRanged phase) 
    {
        float remainingTime = phase.phaseDuration;

        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(10f);
            GetMouseEnemy();

            remainingTime -= 10f;
        }
    }

    private IEnumerator RunRanged(PhaseRanged phase) 
    {
        float remainingTime = phase.phaseDuration;
        bool isDecIntervalChecked = false;

        while (remainingTime > 0)
        {
            if (remainingTime < phase.phaseDuration * 0.5 && !isDecIntervalChecked) 
            {
                currentRangedInterval -= phase.rangedIntervalDecRate;
                isDecIntervalChecked = true;
            }

            yield return new WaitForSeconds(currentRangedInterval);
            GetRangedEnemy();

            remainingTime -= currentRangedInterval;
        }
    }

    private IEnumerator RunMoving(PhaseMoving phase)
    {
        float remainingTime = phase.phaseDuration;
        bool isDecIntervalChecked = false;

        while (remainingTime > 0)
        {
            if (remainingTime < phase.phaseDuration * 0.5 && !isDecIntervalChecked) //phase �ð��� ���� �������� ������ �ش�.
            {
                currentMovingInterval -= phase.movingIntervalDecRate;
                isDecIntervalChecked = true;
            }

            yield return new WaitForSeconds(currentMovingInterval);
            currentSpeed = UpdateCurrentMoving(phase.phaseDuration, phase.movingDefaultSpeed, phase.movingTargetSpeed);
            GetMovingEnemy(currentSpeed);

            remainingTime -= currentMovingInterval;
        }
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


    private void GetMovingEnemy(float currentSpeed)
    {
        movingEnemySpawner.InitiateRandomNode(currentSpeed);
    }

    private void GetRangedEnemy()
    {
        rangedEnemyActivater.ActivateEnemy();
    }

    private void GetMouseEnemy()
    {
        mouseEnemyActivater.ActivateRandomPanel();
    }
}