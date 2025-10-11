using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;


public class DiagonalMonsterSpawner : MonoBehaviour, ISpawnable
{
    enum RankState
    {
        Spawned,
        Success,
        Fail
    };

    public Define.MonsterType MonsterType => Define.MonsterType.Diagonal;

    private Dictionary<DiagonalType, GameObject> diagonalDict;

    private List<int> activatedDiagonalIdx = new List<int>();
    private List<int> deactivatedDiagonalIdx = new List<int>();

    private bool _spawning = false;
    private double _lastSpawnTime;
    private float _moveBeat;

    private int attackValue = 20;

    private double _pauseStartTime;
    private void OnEnable()
    {
        InitialDict();

        Managers.Input.InputDiagonal -= DeactivateDiagonal;
        Managers.Input.InputDiagonal += DeactivateDiagonal;
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;
    }

    private void UpdateRankCnt(RankState state)
    {
        switch (state)
        { 
            case RankState.Success:
                IngameData.IncPerfect();
                break;
            case RankState.Fail:
                IngameData.IncAttacked();
                break;
            case RankState.Spawned:
                IngameData.TotalMobCnt++;
                break;
        }
    }

    private void InitialDict()
    {
        DiagonalMonster[] dm = GetComponentsInChildren<DiagonalMonster>(true);
        diagonalDict = new Dictionary<DiagonalType, GameObject>();

        if (dm.Length == 0)
        {
            Debug.LogWarning("Place LU, LD, RU, RD in Inspector");
        }
        int i = 0;
        foreach (var m in dm)
        {
            diagonalDict[m.DiagonalT] = m.gameObject;
            deactivatedDiagonalIdx.Add(i++);
        }
    }

    public void Spawn(MonsterData data)
    {
        float spawnDuration = (float)IngameData.BeatInterval * data.spawnBeat;
        SetLastSpawnTime(data.moveBeat); 
        _spawning = true;
        _moveBeat = data.moveBeat;
        StartCoroutine(DoSpawn(spawnDuration));

    }
    public void UnSpawn()
    {
        _spawning = false;
        StopAllCoroutines();
    }

    private IEnumerator DoSpawn(float spawnDuration)
    {
        yield return new WaitForSeconds((float)IngameData.BeatInterval * 0.5f);
        while (_spawning)
        {
            // dspTime이 lastSpawnTime을 넘으면 코루틴 종료
            if (AudioSettings.dspTime > _lastSpawnTime)
            {
                UnSpawn(); // 확실하게 스폰을 멈추기 위해 UnSpawn 호출
                yield break;
            }

            ActivateEnemy();
            // Time.timeScale의 영향을 받는 WaitForSeconds를 사용해야 Pause가 제대로 동작합니다.
            yield return new WaitForSeconds(spawnDuration);
        }
    }

    int spawnedDiagonalMobCnt = 0;
    public void ActivateEnemy()
    {
        UpdateRankCnt(RankState.Spawned);

        int idx = Random.Range(0, deactivatedDiagonalIdx.Count);
        if (IngameData.ChapterIdx == 0 && spawnedDiagonalMobCnt<2)
        { 
            idx = (spawnedDiagonalMobCnt == 0) ? (int)DiagonalType.RightUp : (int)DiagonalType.LeftDown;
            spawnedDiagonalMobCnt++;
        }

        if (deactivatedDiagonalIdx.Count == 0) return;
        int mIdx = deactivatedDiagonalIdx[idx];
        deactivatedDiagonalIdx.Remove(mIdx);

        diagonalDict[(DiagonalType)mIdx].GetComponent<DiagonalMonster>().SetMovebeat(_moveBeat);
        diagonalDict[(DiagonalType)mIdx].SetActive(true);
        activatedDiagonalIdx.Add(mIdx);
    }

    private void DeactivateDiagonal(DiagonalType attackType)
    {
        if (activatedDiagonalIdx.Contains((int)attackType))
        {
            activatedDiagonalIdx.Remove((int)attackType);
            deactivatedDiagonalIdx.Add((int)attackType);
            UpdateRankCnt(RankState.Success);
            diagonalDict[attackType].GetComponent<DiagonalMonster>().SetDead();
        }
        else
        {
            Managers.Game.PlayerAttacked(attackValue);
        }
    }

    float threshold = 0.4f;
    public void SetLastSpawnTime(float? moveBeat = 8)
    {
        if (IngameData.PhaseDurationSec == 0)
        {
            Debug.LogWarning("Set Up Phase Duration!");
        }
        float _moveBeat = 4 * (float)moveBeat; // 4번 뛰는데, 한 번 뛸 때마다 moveBeat 박자만큼 걸림
        _lastSpawnTime = AudioSettings.dspTime + IngameData.PhaseDurationSec - (IngameData.BeatInterval * _moveBeat) + threshold;
    }

    public void PauseForWhile(bool isStop)
    {
        _spawning = !isStop;

        if (isStop)
        {
            // Pause 시작 시간 기록
            _pauseStartTime = AudioSettings.dspTime;
        }
        else
        {
            // Pause가 풀렸을 때, Pause된 시간만큼 스폰 종료 시간을 뒤로 밀어줌
            if (_pauseStartTime > 0)
            {
                double pausedDuration = AudioSettings.dspTime - _pauseStartTime;
                _lastSpawnTime += pausedDuration;
                _pauseStartTime = 0; // 초기화
            }
        }
    }
}