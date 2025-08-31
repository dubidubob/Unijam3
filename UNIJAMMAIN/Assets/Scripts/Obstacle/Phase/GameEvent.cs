using System.Collections.Generic;
using UnityEngine;

// ��� ���� �̺�Ʈ�� ���� �������̽� �Ǵ� �߻� Ŭ����
[System.Serializable]
public abstract class GameEvent
{
    public float durationBeat;
    public float startDelayBeat;
    public float bpm;
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
    public string[] tutorialText;
}