using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

public class DiagonalMonsterSpawner : MonoBehaviour, ISpawnable, ISpawnManageable
{
    public Define.MonsterType MonsterType => Define.MonsterType.Diagonal;
    [SerializeField] float boundaryOffset = 1f;

    private Dictionary<DiagonalType, GameObject> diagonalDict;

    private List<int> activatedDiagonalIdx = new List<int>();
    private List<int> deactivatedDiagonalIdx = new List<int>();

    private bool _spawning = false;
    private void Awake()
    {
        InitialDict();

        Managers.Input.InputDiagonal -= DeactivateDiagonal;
        Managers.Input.InputDiagonal += DeactivateDiagonal;
    }

    private void OnDestroy()
    {
        Managers.Input.InputDiagonal -= DeactivateDiagonal;
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
        _spawning = true;
        StartCoroutine(DoSpawn(spawnDuration));
    }
    public void UnSpawn()
    {
        _spawning = false;
        StopAllCoroutines();
    }

    private IEnumerator DoSpawn(float spawnDuration)
    {
        while (_spawning)
        {
            yield return new WaitForSecondsRealtime(spawnDuration);
            ActivateEnemy();
        }
        yield return null;
    }

    public void ActivateEnemy()
    {
        int idx = Random.Range(0, deactivatedDiagonalIdx.Count);
        if (deactivatedDiagonalIdx.Count == 0) return;
        int mIdx = deactivatedDiagonalIdx[idx];
        deactivatedDiagonalIdx.Remove(mIdx);

        diagonalDict[(DiagonalType)mIdx].SetActive(true);
        activatedDiagonalIdx.Add(mIdx);
    }

    private void DeactivateDiagonal(DiagonalType attackType)
    {
        if (activatedDiagonalIdx.Contains((int)attackType))
        {
            activatedDiagonalIdx.Remove((int)attackType);
            deactivatedDiagonalIdx.Add((int)attackType);
            diagonalDict[attackType].GetComponent<DiagonalMonster>().SetDead();
        }
        else
        {
            Managers.Game.PlayerAttacked();
        }
    }

    public void Deactivate()
    {
        activatedDiagonalIdx = new List<int>();
        
    }
}