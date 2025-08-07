using UnityEngine;

[RequireComponent(typeof(PhaseManager))]
[RequireComponent(typeof(WASDMonsterSpawner))]
public class QA : MonoBehaviour
{
    PhaseManager phaseManager;
    WASDMonsterSpawner WASDMonsterSpawner;

    // phase Manager, 변경 버튼 누르면
    private float startDelay = 0.12f, phaseDuration = 1000f;
    private MonsterData monsterData =
        new MonsterData {
            isIn = true,
            monsterType = Define.MonsterType.WASD,
            numInRow = 1,
            spawnDuration = 1f,
            moveToHolderDuration = 1f,
            speedUpRate = 1f
        };

    // wasdspawncontroller, 변경 버튼 누르면
    private Vector2 sizeDiffRate = new Vector2(0.8f, 1.2f);
    private int[] idx = { 0, 1, 2, 3 };
    private int maxCnt = 3;

    private void Awake()
    {
        phaseManager = GetComponent<PhaseManager>();
        WASDMonsterSpawner = GetComponent<WASDMonsterSpawner>();
    }

    private void Start()
    {
        ChangeOnWASDSpawnController();
        ChangeOnPhaseManager();
    }

    public void ReloadScene() { ReloadScene(); }

    public void ChangeOnPhaseManager()
    {
        phaseManager.QAPhaseVariance(startDelay, phaseDuration, monsterData);
    }

    public void ChangeOnWASDSpawnController()
    {
        WASDMonsterSpawner.QAUpdateVariables(sizeDiffRate, idx, maxCnt);
    }
}
