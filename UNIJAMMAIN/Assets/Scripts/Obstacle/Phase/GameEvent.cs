using System.Collections.Generic;
using UnityEngine;

// ��� ���� �̺�Ʈ�� ���� �������̽� �Ǵ� �߻� Ŭ����
[System.Serializable]
public abstract class GameEvent
{
    [Min(0f)] public float durationBeat;
    [Min(0f)] public float startDelayBeat;
    [Min(1f)] public float bpm;
}

// ���� ���� ����� ���� �̺�Ʈ
[System.Serializable]
public class PhaseEvent : GameEvent
{
    public bool isFlipAD;
    [SerializeField] private List<MonsterData> monsterDatas;
    public IReadOnlyList<MonsterData> MonsterDatas => monsterDatas;
}

// Ʃ�丮�� �Ǵ� ���丮 ������ ���� �̺�Ʈ
[System.Serializable]
public class TutorialEvent : GameEvent
{
    [SerializeField] private TextInfo[] steps;
    public IReadOnlyList<TextInfo> Steps => steps;
    public void RecalculateDuration()
    {
        durationBeat = 0f;
        float sum = 0f;
        if (steps != null)
        {
            for (int i = 0; i < steps.Length; i++)
                sum += Mathf.Max(0f, steps[i].delayBeat);
        }
        startDelayBeat = sum;
    }
}

[System.Serializable]
public struct TextInfo
{
    public string textContents;
    public float delayBeat;
}