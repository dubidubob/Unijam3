using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GamePlayDefine;
using Assets.VFXPACK_IMPACT_WALLCOEUR.Scripts;

[Serializable]
public struct RankNode
{
    public EvaluateType RankT;
    public WASDType WASDT;
    public Vector2? Pos;

    public RankNode(EvaluateType t, WASDType w, Vector2? pos)
    { RankT = t; WASDT = w; Pos = pos; }
}

public class GameManager
{
    // Actions
    public Action<int> ComboContinue = null;
    public Action<float> HealthUpdate = null;
    public Action<RankNode> RankUpdate = null;

    // State
    public int currentPhase { get; private set; } = 0;
    public GameState CurrentState { get; set; }
    public PlayerState currentPlayerState;

    // Stats
    private int Combo = 0;
    private float Health = 100;
    public readonly int MaxHealth = 130;
    private const int IncHealthUnit = 10;

    // References
    public BlurController blur;
    public Accuracy accuracy;
    public PlayerActionUI actionUI;
    public MonsterDatabaseSO monster;
    public VfxController vfxController;
    public BeatClock beatClock;
    public PhaseController phaseController;
    public BackGroundController backGroundController;

    // [최적화 1] GameObject 대신 MovingEnemy 컴포넌트를 직접 저장
    public Dictionary<WASDType, Queue<MovingEnemy>> attacks = new Dictionary<WASDType, Queue<MovingEnemy>>();

    // Enums
    public enum GameState { Battle, Stage, Tutorial, Die }
    public enum PlayerState { Normal, GroggyAttack, Ready, Die }

    public bool isADReverse = false;
    private bool isComboEffect = false;
    private bool _comboCheck = false;

    // ==================================================================================
    // Initialize & Cleanup
    // ==================================================================================

    public void Init()
    {
        Clear(); // [중요] 초기화 시 기존 데이터 확실히 정리

        if (SceneManager.GetActiveScene().name == "GamePlayScene")
        {
            CurrentState = GameState.Battle;
            currentPlayerState = PlayerState.Normal;
            Health = MaxHealth;
        }
        else
        {
            CurrentState = GameState.Stage;
        }
        PauseManager.ControlTime(false);
    }

    public void GameStart()
    {
        currentPlayerState = PlayerState.Ready;
        isComboEffect = false;

        // Null Check 추가 (안전 장치)
        if (backGroundController != null) backGroundController.ChapterBackGroundInitialize();
        if (actionUI != null) actionUI.PlayerSpriteInit(IngameData.ChapterIdx);
    }

    public void Clear()
    {
        // 큐 내부 데이터 명시적 초기화
        attacks.Clear();

        currentPhase = 0;
        ComboContinue = null;
        HealthUpdate = null;
        RankUpdate = null;

        Combo = 0;
        Health = 100;
        currentPlayerState = PlayerState.Normal;
        CurrentState = GameState.Stage;
    }

    // ==================================================================================
    // Input Handling (핵심 수정 부분)
    // ==================================================================================

    public void SetADReverse(bool isReverse)
    {
        isADReverse = isReverse;
    }

    public void AddAttackableEnemy(GamePlayDefine.WASDType key, GameObject go)
    {
        // [최적화 2] GetComponent를 여기서 한 번만 수행
        MovingEnemy enemy = go.GetComponent<MovingEnemy>();

        if (enemy == null) return;
        if (enemy.isKnockbacked) return;

        if (!attacks.ContainsKey(key))
        {
            attacks[key] = new Queue<MovingEnemy>();
        }
        attacks[key].Enqueue(enemy);
    }

    public void ReceiveKey(WASDType key)
    {
        if (currentPlayerState == PlayerState.Die) return;


        // 키 반전 로직
        if (isADReverse)
        {
            if (key == WASDType.A) key = WASDType.D;
            else if (key == WASDType.D) key = WASDType.A;
        }

        // 큐가 존재하고 내용물이 있는지 확인
        if (attacks.ContainsKey(key))
        {
            Queue<MovingEnemy> enemyQueue = attacks[key];

            // 큐의 앞부분에 파괴된(Null) 오브젝트가 있다면 제거 (청소 로직)
            // 재시작 직후나 몬스터가 다른 이유로 죽었을 때 발생하는 MissingReferenceException 방지
            while (enemyQueue.Count > 0 && enemyQueue.Peek() == null)
            {
                enemyQueue.Dequeue();
            }

            // 유효한 적이 남아있는 경우
            if (enemyQueue.Count > 0)
            {
                // GetComponent 없이 바로 접근
                MovingEnemy targetEnemy = enemyQueue.Peek();

                if (targetEnemy.CheckCanDead())
                {
                    RankUpdate?.Invoke(new RankNode(EvaluateType.Success, key, targetEnemy.transform.position));

                    enemyQueue.Dequeue(); // 처리된 적 제거
                    targetEnemy.SetDead();
                    ComboInc();
                    return; // 성공했으므로 종료
                }
                else  // 유효한 적이 존재하지만 처리할 수는 없는 상태
                {
                    return; // 넉백몹일 가능성이 높으므로 종료
                }
            }
        }

        // 여기까지 왔다면 실패한 것 (큐가 비었거나, 유효한 적이 없거나)
        HandleMiss(key);
    }

    private void HandleMiss(WASDType key)
    {
        RankUpdate?.Invoke(new RankNode(EvaluateType.Wrong, key, null));
        DecHealth(7);
    }

    // ==================================================================================
    // Combat Logic (Health & Combo)
    // ==================================================================================

    public void ComboInc(int healingValue = 1)
    {
        if (IngameData._defeatEnemyCount >= 10000)
        {
            Managers.Steam.UnlockAchievement("ACH_COMBO_TOTAL_10000");
        }

        Combo++;
        IncHealth(healingValue);

        ComboContinue?.Invoke(Combo);

        // 주기적 힐링
        if (Combo > 0 && Combo % IncHealthUnit == 0)
        {
            IncHealth(healingValue);
        }

        // 콤보 이펙트 및 업적
        HandleComboEffects();

        // 10단위 추가 힐링 및 사운드
        if (Combo % 10 == 0)
        {
            int bonusHeal = GetBonusHealAmount(Combo);
            IncHealth(bonusHeal);
            blur.ComboEffect();
            Managers.Sound.Play("SFX/Combo_Breathe_SFX");
        }
    }

    private void HandleComboEffects()
    {
        if (Combo >= 30 && !isComboEffect)
        {
            blur.ComboEffectOn();
            isComboEffect = true;
        }

        if (Combo >= 108 && isComboEffect)
        {
            Managers.Steam.UnlockAchievement("ACH_COMBO_108");
        }
    }

    private int GetBonusHealAmount(int currentCombo)
    {
        if (currentCombo % 30 == 0) return 12;
        if (currentCombo % 20 == 0) return 9;
        return 7;
    }

    public void PlayerAttacked(int attackValue)
    {
        RankUpdate?.Invoke(new RankNode(EvaluateType.Attacked, WASDType.A, null));
        DecHealth(attackValue);
    }

    private void DecHealth(int value)
    {
        if (currentPlayerState == PlayerState.Ready) return;

        Combo = 0;
        if (blur != null) blur.ComboEffectOff();
        isComboEffect = false;
        ComboContinue?.Invoke(Combo);

        if (Health > 0)
        {
            Health -= value;
        }

        HealthUpdate?.Invoke(Health);

        if (Health <= 0)
        {
            GameOver();
        }
    }

    public void IncHealth(float healValue = 1f)
    {
        if (Health <= 0 || Health >= MaxHealth) return;

        Health += healValue;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        HealthUpdate?.Invoke(Health);
    }

    // ==================================================================================
    // Phase & Game Over
    // ==================================================================================

    public void IncPhase() => currentPhase++;
    public int GetPhase() => currentPhase;

    private void GameOver()
    {
        currentPlayerState = PlayerState.Die;
        IngameData.Pause = true;
        PauseManager.ControlTime(true);

        if (actionUI != null) actionUI.GameOverAnimation();
        if (blur != null)
        {
            blur.GameOverBlurEffect();
            blur.WaitForGameOver();
        }

        CheckGameOverAchievements();

        Managers.Sound.BGMFadeOut();
        Managers.Sound.Play("BGM/GameOver_V1");

        Time.timeScale = 0;
    }

    private void CheckGameOverAchievements()
    {
        if (beatClock != null)
        {
            double playTime = beatClock.GetCurrentPlayTime();
            if (playTime <= 15.0d)
            {
                Managers.Steam.UnlockAchievement("ACH_DEATH_INSTANT");
                Debug.Log($"[Steam] 15초 내 사망 업적 달성! (기록: {playTime:F2}초)");
            }
        }
    }
}