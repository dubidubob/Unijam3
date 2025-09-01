using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GamePlayDefine;


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
    public Action<int> ComboContinue = null;
    public Action<float> HealthUpdate = null;

    public Action<RankNode> RankUpdate = null;

    public int currentPhase { get; private set; } = 0;

    private int Combo = 0;
    private float Health = 100;
    public readonly int MaxHealth = 100;
    private const int IncHealthUnit = 10;
    public BlurController blur;
    public Accuracy accuracy;
    public PlayerActionUI actionUI;
   

    private float healingValue = 2.5f; // 회복하는 양
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
}

    //게임 상태를 나눠서 상태에 따라 스크립트들이 돌아가게 함
    public enum GameState
    {
        Battle,
        Stage,
        Die
        
    }

    public enum PlayerState
    {
        Normal,
        GroggyAttack,
        Die
    }
    public GameState CurrentState { get; set; }
    public PlayerState currentPlayerState;

    //인게임 데이터 초기화 
    public void Init()
    {
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
            
        Time.timeScale = 1f;
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
            DecHealth();
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

    public void ComboInc()
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
        if(Combo%10==0)
        {
            blur.ComboEffect();
            IncHealth(healingValue*3); //체력 회복
            Managers.Sound.Play("SFX/Combo_Breathe_SFX");
        }
    }

    public void PlayerAttacked()
    {
        RankUpdate?.Invoke(
    new RankNode(EvaluateType.Attacked, WASDType.A, null));
        DecHealth();
    }

    private void DecHealth()
    {
        Combo = 0; //무조건 콤보 끊김
        ComboContinue?.Invoke(Combo);

        if (Health > 0)
        {
            Health -= 5;
        }
        
        HealthUpdate?.Invoke(Health);
        if(Health<=0) // 게임오버
        {
            GameOver();
        }
    }

    public void IncHealth(float healValue = 2.5f)
    {
        if ((Health <= 0) || (Health > 100))
            return;

        Health += healValue; // 체력회복 3
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
        Debug.Log("사망!");
        currentPlayerState = PlayerState.Die;
        actionUI.GameOverAnimation();
    }
}
