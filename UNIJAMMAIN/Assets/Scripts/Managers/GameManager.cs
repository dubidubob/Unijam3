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
    public int GameStage = 0;

    public Action<int> ComboContinue = null;
    public Action<float> HealthUpdate = null;

    public Action<RankNode> RankUpdate = null;

    public int currentPhase { get; private set; } = 0;
    

    private int Combo = 0;
    private float Health = 100;
    public readonly int MaxHealth = 130;
    private const int IncHealthUnit = 10;
    public BlurController blur;
    public Accuracy accuracy;
    public PlayerActionUI actionUI;
    public MonsterDatabaseSO monster;
    public VfxController vfxController;
    public BeatClock beatClock;

    
    public int perfect = 0;
    // TODO : 이러지 말기
    public Dictionary<WASDType, Queue<GameObject>> attacks = new Dictionary<GamePlayDefine.WASDType, Queue<GameObject>>();
    public void Clear()
    {
        attacks = new Dictionary<WASDType, Queue<GameObject>>();
        currentPhase = 0;
        ComboContinue = null;
        HealthUpdate = null;
        RankUpdate = null;
        Combo = 0;
        Health = 100;
        currentPlayerState = PlayerState.Normal;
        CurrentState = GameState.Stage;
}

    //게임 상태를 나눠서 상태에 따라 스크립트들이 돌아가게 함
    public enum GameState
    {
        Battle,
        Stage,
        Tutorial,
        Die
        
    }

    public enum PlayerState
    {
        Normal,
        GroggyAttack,
        Ready,
        Die
    }
    public GameState CurrentState { get; set; }
    public PlayerState currentPlayerState;

    //인게임 데이터 초기화 
    public void Init()
    {
        Debug.Log("GameManager Init");
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

    bool isADReverse = false;
    public void ReceiveKey(WASDType key)
    {
        if(currentPlayerState==PlayerState.Die) // 사망시 키 받지않기
        {
            return;
        }

        if (isADReverse)
        {
            if (key == WASDType.A)
                key = WASDType.D;
            else if (key == WASDType.D)
                key = WASDType.A;
        }

        if (attacks.ContainsKey(key) && attacks[key].Count > 0)
        { 
            GameObject go = attacks[key].Peek();

            MovingEnemy wasd = go.GetComponent<MovingEnemy>();

            if (wasd.CheckCanDead())
            {
                // 현재 enemy position만 담아서, true
                RankUpdate?.Invoke(
                    new RankNode( EvaluateType.Success, key, go.transform.position));
                attacks[key].Dequeue();
                go.GetComponent<MovingEnemy>().SetDead();
                ComboInc();
            }
        }
        else
        {
            RankUpdate?.Invoke(
                new RankNode(EvaluateType.Wrong, key, null));
            DecHealth(7);
        }
    }

    public void SetADReverse(bool isReverse)
    { 
        isADReverse = isReverse;
    }

    public void AddAttackableEnemy(GamePlayDefine.WASDType key, GameObject go)
    {
        if(go.GetComponent<MovingEnemy>().isKnockbacked) return;
        if (!attacks.ContainsKey(key))
        {
            attacks[key] = new Queue<GameObject>();
        }
        attacks[key].Enqueue(go);
    }
    public bool isComboEffect = false;
    public void ComboInc(int healingValue=1)
    {
        Combo++;
        IncHealth(healingValue); // 체력회복
        if (ComboContinue != null)
        {
            ComboContinue.Invoke(Combo);
        }
        // 콤보에 따라 회복하는거
        if (Combo > 0 && Combo % IncHealthUnit == 0)
        {
            IncHealth(healingValue); //체력 회복
        }
        
        if(Combo>=30)
        {
            if (!isComboEffect)
            {
                blur.ComboEffectOn();
                isComboEffect = true;
            }
        }
        
        if(Combo%10==0)
        {
            int _healtmp=7;
            if (Combo%20==0)
            {
                _healtmp = 9;
                if(Combo%30==0)
                {
                    _healtmp = 12;
                    
                }
            }
            IncHealth(_healtmp); //체력 회복
            blur.ComboEffect();
           
            Managers.Sound.Play("SFX/Combo_Breathe_SFX");
        }
    }
    /// <summary>
    /// 피해를 입히는 정도
    /// </summary>
    /// <param name="value"></param>
    public void PlayerAttacked(int attackValue)
    {
        RankUpdate?.Invoke(
        new RankNode(EvaluateType.Attacked, WASDType.A, null));
        DecHealth(attackValue);
    }
    private bool _comboCheck = false;
    /// <summary>
    /// value값은 체력이 감소되는 양
    /// </summary>
    /// <param name="value"></param>
    private void DecHealth(int value)
    {
        if(currentPlayerState== PlayerState.Ready) { return; } // Ready상태면 무시
        Combo = 0; //무조건 콤보 끊김
        Managers.Game.blur.ComboEffectOff();
        Managers.Game.isComboEffect = false;
        ComboContinue?.Invoke(Combo);

        if (Health > 0)
        {
            Health -= value;
        }
        
        HealthUpdate?.Invoke(Health);
        if(Health<=0) // 게임오버
        {
            GameOver();
        }
    }

    public void IncHealth(float healValue = 1f)
    {
        if (Health <= 0 || Health >= MaxHealth)
            return;

        Health += healValue;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        HealthUpdate?.Invoke(Health);
    }

    public void IncPhase()
    {
        currentPhase++;
    }

    public int GetPhase()
    {
        return currentPhase;
    }

    private void GameOver()
    {
        currentPlayerState = PlayerState.Die;
        actionUI.GameOverAnimation();
        blur.GameOverBlurEffect();
        blur.WaitForGameOver();

        Managers.Sound.BGMFadeOut();
        Managers.Sound.Play("BGM/GameOver_V1");

        Time.timeScale = 0;
        
    }
    /// <summary>
    /// 인게임 창에 들어갔을때 호출. Scene을 호출하자마자 설정.
    /// </summary>
    public void GameStart()
    {
        currentPlayerState = PlayerState.Ready; // normal로 바꿔주는건 StartCountUI에서 담당하자.
        isComboEffect = false;
    }
}
