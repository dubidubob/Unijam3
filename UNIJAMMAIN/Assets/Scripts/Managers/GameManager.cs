using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private Dictionary<string, Queue<Transform>> attacks = new Dictionary<string, Queue<Transform>>();

    public void ReceiveKey(string key)
    {
        Debug.Log($"key pressed : {key}");
      //queue   
    }

    public void ReceiveTrans(string key, Transform transform)
    { 
        //해당 객체의 특정 스크립트 함수를 실행시키면 지가 알아서
    }
}
