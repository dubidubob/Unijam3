using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class KeyTrackerManager
{
    // Ű���� ���� Ƚ���� ������ ����
    public Dictionary<string, int> keyPressCounts = new Dictionary<string, int>
    {
        { "W", 0 },
        { "A", 0 },
        { "S", 0 },
        { "D", 0 },
        { "LeftUp", 0 },
        { "LeftDown",0 },
        { "RightUp", 0 },
        { "RightDown", 0 }
    };
    public Action<string> KeyArrowMissed = null;
    public Action<string> KeyFree = null;
    // ��� ó�� ���� Ƚ��
    private int maxPressCount = 4;

    public void Clear()
    {
        keyPressCounts = new Dictionary<string, int>{
            { "W", 0 },
            { "A", 0 },
            { "S", 0 },
            { "D", 0 },
            { "LeftUp", 0 },
            { "LeftDown",0 },
            { "RightUp", 0 },
            { "RightDown",0 }
        };
        KeyArrowMissed = null;
        KeyFree = null;
    }
    // WASD �� �߸� ������ �� ������ ����
    public void MissedKeyPress(string key)
    {
        if (keyPressCounts[key] < maxPressCount)
        {
            keyPressCounts[key]++;
            Debug.Log($"{key} Misspressed {keyPressCounts[key]} times");
            

            if (keyPressCounts[key] == maxPressCount )
            {
                KeyArrowMissed.Invoke(key);
                StaticCoroutine.StartStaticCoroutine(BlockRealease(key));
                Debug.Log($"{key} is blocked.");
            }
        }
    }
    private IEnumerator BlockRealease(string key)
    {
        Debug.Log("�ڷ�ƾ�� �����ϱ��մϴ�.");
        float CountTime = 2f;
        while(CountTime>=0)
        {
            CountTime -= Time.deltaTime;
            yield return null;

            //Managers.Game.KeyBl7ock(key);
            Debug.Log($"{key} is blocked.");

        }
        keyPressCounts[key] = 0; // ī��Ʈ �ʱ�ȭ
        Debug.Log("BlockCount�� �ʱ�ȭ �Ǿ����ϴ�");
        KeyFree.Invoke(key);
    }

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
