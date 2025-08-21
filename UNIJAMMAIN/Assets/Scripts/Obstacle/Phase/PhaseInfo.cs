using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 해당 페이즈에 대한 정보를 갖고 있다.
/// </summary>
[CreateAssetMenu(fileName = "Chapter", menuName = "SO/Chapter")]
public class ChapterSO : ScriptableObject
{
    [System.Serializable]
    public struct PhaseInfo
    {
        public float duration;
        public float startDelay;
        public float bpm;
        [SerializeField]
        private List<MonsterData> monsterDatas;
        public IReadOnlyList<MonsterData> MonsterDatas => monsterDatas;
    }

    [SerializeField]
    private List<PhaseInfo> phases;
    public IReadOnlyList<PhaseInfo> Phases => phases;
}