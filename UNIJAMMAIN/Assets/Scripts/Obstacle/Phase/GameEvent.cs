using System.Collections.Generic;
using UnityEngine;

// 모든 게임 이벤트를 위한 인터페이스 또는 추상 클래스
[System.Serializable]
public abstract class GameEvent
{
    public bool isIn;
    [Min(0f)] public float durationBeat;
    [Min(0f)] public float startDelayBeat;
    [Min(1f)] public float bpm;
    [Min(1f)] public float timeScale=1;
    public float preGenerateBeat;
    public int extensionCreateBeat;
}

// 리듬 게임 페이즈를 위한 이벤트
[System.Serializable]
public class PhaseEvent : GameEvent
{
    public bool isFlipAD;
    [SerializeField] public List<MonsterData> monsterDatas;
    public IReadOnlyList<MonsterData> MonsterDatas => monsterDatas;
}

// 튜토리얼 또는 스토리 섹션을 위한 이벤트
[System.Serializable]
public class TutorialEvent : GameEvent
{
    [Header("선택지는 첫번째 인덱스 내용이 성공, 두번째 인덱스 내용이 실패")]
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
    public string[] textContents;
    public int monsterCutline;
    public float delayBeat;
    public CharacterData characterData;
    public Tutorial_PopUp.dir dir;
}