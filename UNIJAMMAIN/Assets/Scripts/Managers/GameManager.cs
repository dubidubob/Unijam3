using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public Transform playerTransform;
    public int currentPhase { get; private set; } = 0;
    public Action<int> ComboContinue = null;
    public Action<int> HealthUpdate = null;
    public Action<int> PhaseUpdate = null;
    public Action<string> MissedKeyUpdate = null;
    private int Combo = 0;
    private int Health = 0;
    public readonly int MaxHealth = 100;
    private const int IncHealthUnit = 10;

    public int perfect = 0;
    // TODO : 이러지 말기
    public Dictionary<GamePlayDefine.WASDType, Queue<GameObject>> attacks = new Dictionary<GamePlayDefine.WASDType, Queue<GameObject>>();
    public void Clear()
    {
        attacks = new Dictionary<GamePlayDefine.WASDType, Queue<GameObject>>();
        playerTransform = null;
        currentPhase = 0;
        ComboContinue = null;
        HealthUpdate = null;
        PhaseUpdate = null;
        MissedKeyUpdate = null;
        Combo = 0;
        Health = 0;
}

    //게임 상태를 나눠서 상태에 따라 스크립트들이 돌아가게 함
    public enum GameState
    {
        Battle,
        Stage
        
    }

    public enum PlayerState
    {
        Normal,
        GroggyAttack
    }
    public GameState currentState;
    public PlayerState currentPlayerState;
    //플레이어 죽을 때 실행시킬 함수
    public void PlayerDied()
    {
       
    }

    //인게임 데이터 초기화 
    public void GameStart()
    {
        currentState = GameState.Battle;
        currentPlayerState = PlayerState.Normal;
        playerTransform = GameObject.FindWithTag("Player").transform; // 플레이어의 현재 위치받기
        Health = MaxHealth;
        Time.timeScale = 1f;
    }
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }


    public void ReceiveKey(GamePlayDefine.WASDType key)
    {
        if (attacks.ContainsKey(key) && attacks[key].Count > 0)
        {
            GameObject go = attacks[key].Peek();

            MovingEnemy wasd = go.GetComponent<MovingEnemy>();

            if (wasd.CheckCanDead())
            {
                attacks[key].Dequeue();
                go.GetComponent<MovingEnemy>().SetDead();
                ComboInc();
            }
        }
        else
        {
            //Managers.Tracker.MissedKeyPress(key);
            //MissedKeyUpdate?.Invoke(key);
            DecHealth();
        }
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
        if (ComboContinue != null)
        {
            ComboContinue.Invoke(Combo);
        }
        if (Combo > 0 && Combo % IncHealthUnit == 0)
        {
            IncHealth();
        }
        if(Combo%10==0)
        {
            Managers.Sound.Play("SFX/Combo_Breathe_SFX");
        }
    }

    public void DecHealth()
    {
        Combo = 0; //무조건 콤보 끊김
        ComboContinue?.Invoke(Combo);
        
        if (Health > 0)
        {
            Health -= 6;
        }
        HealthUpdate.Invoke(Health);
    }

    public void IncHealth()
    {
        if ((Health <= 0) || (Health > 10))
            return;

        Health++;
        HealthUpdate.Invoke(Health);
        Debug.Log("Inc Health");
    }

    public void IncPhase()
    {
        currentPhase++;
        PhaseUpdate?.Invoke(currentPhase);
    }

    public int GetPhase()
    {
        return currentPhase;
    }
}
