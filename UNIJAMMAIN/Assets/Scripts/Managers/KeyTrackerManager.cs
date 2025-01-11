using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTrackerManager
{

    int DownCounts = 3;

    // Ű���� ���� Ƚ���� ������ ����
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

    // ��� ó�� ���� Ƚ��
    private int maxPressCount = 4;

    // WASD �� �߸� ������ �� ������ ����
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

    // Ư�� Ű�� ���� Ƚ���� �ʱ�ȭ�ϴ� �޼���
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
