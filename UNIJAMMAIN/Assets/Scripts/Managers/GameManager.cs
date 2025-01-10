using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public Transform playerTransform;
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
    }
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    private Dictionary<string, Queue<GameObject>> attacks = new Dictionary<string, Queue<GameObject>>();

    public void ReceiveKey(string key)
    {
        Debug.Log($"key pressed : {key}");
        if (attacks.ContainsKey(key) && attacks[key].Count > 0)
        {
            GameObject go = attacks[key].Dequeue();
            if (go == null)
            {
                return;
            }

            go.GetComponent<Enemy>().SetDead();
            return;
        }

        DecHealth();
    }

    public void AddAttackableEnemy(string key, GameObject go)
    {
        if (!attacks.ContainsKey(key))
        {
            attacks[key] = new Queue<GameObject>();
        }
        attacks[key].Enqueue(go);
    }

    private void DecHealth()
    {
        Debug.Log("Dec Health");
    }
}
