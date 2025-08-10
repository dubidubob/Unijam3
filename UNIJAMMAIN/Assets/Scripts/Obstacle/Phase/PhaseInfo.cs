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
        [SerializeField]
        private List<MonsterData> monsterDatas;
        public IReadOnlyList<MonsterData> MonsterDatas => monsterDatas;
    }

    [Header("Chapter Phases")]
    //TODO 이거 phase info에 넣기
    //public PhaseMoving phase1 = new PhaseMov1ing(40f, 2f, 2.5f, 0.1f);
    //public PhaseRanged phase2 = new PhaseRanged(40.5f, 2f, 3f, 0.1f, 0.1f);
    //public PhaseRanged phase22 = new PhaseRanged(29.5f, 3f, 2.2f, 0.1f, 0.1f);
    //public PhaseRanged phase3 = new PhaseRanged(50f, 2.2f, 2.7f, 0.1f, 0.1f);

    [SerializeField]
    private List<PhaseInfo> phases;
    public IReadOnlyList<PhaseInfo> Phases => phases;
}