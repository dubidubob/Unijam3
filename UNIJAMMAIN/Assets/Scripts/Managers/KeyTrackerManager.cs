using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTrackerManager
{

    int DownCounts = 3;

    // 키별로 누른 횟수를 추적할 사전
    public Dictionary<string, int> keyPressCounts = new Dictionary<string, int>
    {
        { "W", 0 },
        { "A", 0 },
        { "S", 0 },
        { "D", 0 },
        { "LeftUp",0 },
        { "LeftDown",0 },
        { "RightUp", 0 },
        { "RightDown",0 }
    };

    // 블록 처리 기준 횟수
    private int maxPressCount = 4;

    // WASD 가 잘못 눌렸을 때 실행할 동작
    public void MissedKeyPress(string key)
    {
        if (keyPressCounts[key] < maxPressCount)
        {
            keyPressCounts[key]++;
            Debug.Log($"{key} Misspressed {keyPressCounts[key]} times");
        }
        else
        {
            Managers.Game.KeyBlock(key);
            Debug.Log($"{key} is blocked.");
        }
    }

    // 특정 키의 누른 횟수를 초기화하는 메서드
    public void ResetKeyPress(string key)
    {
        if (keyPressCounts.ContainsKey(key))
        {
            keyPressCounts[key] = 0;
            Debug.Log($"{key} press count reset.");
        }
    }

    public void GoingDownCounts()
    {

    }
}
