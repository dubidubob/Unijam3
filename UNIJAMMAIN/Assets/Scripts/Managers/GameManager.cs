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
    public readonly int MaxHealth = 10;
    private const int IncHealthUnit = 10;

    public void Clear()
    {
        attacks = new Dictionary<string, Queue<GameObject>>();
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
        CameraMoving,
        Battle,
        Store,
        Bless,
    }
    public GameState currentState;
    //플레이어 죽을 때 실행시킬 함수
    public void PlayerDied()
    {
       
    }
    //인게임 데이터 초기화 
    public void GameStart()
    {
        currentState = GameState.Battle;
        Debug.Log("코드실행완료");
        playerTransform = GameObject.FindWithTag("Player").transform; // 플레이어의 현재위치받기
        Health = MaxHealth;
        Time.timeScale = 1f;
    }
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    private Dictionary<string, Queue<GameObject>> attacks = new Dictionary<string, Queue<GameObject>>();

    public bool ReceiveKey(string key)
    {
        Debug.Log($"key pressed : {key}");
        if (attacks.ContainsKey(key) && attacks[key].Count > 0)
        {
            GameObject go = attacks[key].Dequeue();

            switch (UnityEngine.Random.Range(0, 2))
            {
                case 0:
                    {
                        Managers.Sound.Play("Sounds/SFX/WASD_Glass_Broken_SFX_2");
                        break;
                    }
                case 1:
                    {
                        Managers.Sound.Play("Sounds/SFX/WASD_Glass_Broken_SFX_1");
                        break;
                    }
            }
            if (go == null)
            {
                Debug.LogError("큰일난오류?");
                return false;
            }
            ComboInc();
            go.GetComponent<MovingEnemy>().SetDead();

            if (key == "A")
            {
                switch (UnityEngine.Random.Range(0, 2))
                {
                    case 0:
                        {
                            Managers.Sound.Play("Sounds/SFX/AKey_Action_SFX_1");
                            break;
                        }
                    case 1:
                        {
                            Managers.Sound.Play("Sounds/SFX/AKey_Action_SFX_1");
                            break;
                        }
                }

            }
            else if (key == "D")
            {
                switch (UnityEngine.Random.Range(0, 4))
                {
                    case 0:
                        {
                            Managers.Sound.Play("Sounds/SFX/DKey_Action_SFX_1");
                            break;
                        }
                    case 1:
                        {
                            Managers.Sound.Play("Sounds/SFX/DKey_Action_SFX_1");
                            break;
                        }
                    case 2:
                        {
                            Managers.Sound.Play("Sounds/SFX/DKey_TR_BL_Action_SFX_1");
                            break;
                        }

                }
            }
            else if (key == "W")
            {
                switch (UnityEngine.Random.Range(0, 2))
                {
                    case 0:
                        {
                            Managers.Sound.Play("Sounds/SFX/WKey_Action_SFX_1");
                            break;
                        }
                    case 1:
                        {
                            Managers.Sound.Play("Sounds/SFX/WKey_Action_SFX_1");
                            break;
                        }

                }

                switch (UnityEngine.Random.Range(0, 2))
                {
                    case 0:
                        {
                            Managers.Sound.Play("Sounds/SFX/W,SKey_Action_Shout_SFX_1");
                            break;
                        }
                    case 1:
                        {
                            Managers.Sound.Play("Sounds/SFX/W,SKey_Action_Shout_SFX_2");
                            break;
                        }

                }
                }
            else if (key == "S")
            {
                switch (UnityEngine.Random.Range(0, 2))
                {
                    case 0:
                        {
                            Managers.Sound.Play("Sounds/SFX/SKey_Action_SFX_1");
                            break;
                        }
                    case 1:
                        {
                            Managers.Sound.Play("Sounds/SFX/SKey_Action_SFX_1");
                            break;
                        }
                }
                switch (UnityEngine.Random.Range(0, 2))
                {
                    case 0:
                        {
                            Managers.Sound.Play("Sounds/SFX/W,SKey_Action_Shout_SFX_1");
                            break;
                        }
                    case 1:
                        {
                            Managers.Sound.Play("Sounds/SFX/W,SKey_Action_Shout_SFX_2");
                            break;
                        }

                }
            }
           

            return true;
        }

        Managers.Tracker.MissedKeyPress(key);
        MissedKeyUpdate.Invoke(key);
        DecHealth();
        return false;
    }

    public void AddAttackableEnemy(string key, GameObject go)
    {
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
            Debug.Log("Invoke ComboContinue");
            ComboContinue.Invoke(Combo);
        }
        if (Combo > 0 && Combo % IncHealthUnit == 0)
        {
            IncHealth();
        }
        Debug.Log(Combo); 
        if(Combo%10==0)
        {
            Debug.Log("콤보 진입");
            Managers.Sound.Play("SFX/Combo_Breathe_SFX");
        }
    }

    public void DecHealth()
    {
        Combo = 0; //무조건 콤보 끊김
        ComboContinue?.Invoke(Combo);
        
        if (Health > 0)
        {
            Health--;
        }
        HealthUpdate.Invoke(Health);
        Debug.Log("Dec Health");
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
