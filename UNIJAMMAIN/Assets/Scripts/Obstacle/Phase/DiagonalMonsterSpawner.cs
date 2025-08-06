using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

public class DiagonalMonsterSpawner : MonoBehaviour, ISpawnable
{
    public Define.MonsterType MonsterType => Define.MonsterType.Diagonal;
    [SerializeField] float boundaryOffset = 1f;

    private Dictionary<DiagonalType, GameObject> diagonalDict;

    private List<int> activatedDiagonalIdx = new List<int>();
    private List<int> deactivatedDiagonalIdx = new List<int>();

    private void Awake()
    {
        InitialDict();

        Managers.Input.KeyArrowcodeAction -= DeActivateEnemy;
        Managers.Input.KeyArrowcodeAction += DeActivateEnemy;

        InvestScreenSize();
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
        ActivateEnemy();
    }

    public void ActivateEnemy()
    {
        int idx = Random.Range(0, deactivatedDiagonalIdx.Count);
        int mIdx = deactivatedDiagonalIdx[idx];
        deactivatedDiagonalIdx.Remove(mIdx);

        PosAndActivateNode((DiagonalType)mIdx);
        activatedDiagonalIdx.Add(mIdx);
    }

    private void DeActivateEnemy(DiagonalType attackType)
    {
        if (activatedDiagonalIdx.Contains((int)attackType))
        {
            activatedDiagonalIdx.Remove((int)attackType);
            deactivatedDiagonalIdx.Add((int)attackType);
            diagonalDict[attackType].GetComponent<DiagonalMonster>().SetDead();
        }
        else
        {
            Managers.Game.DecHealth();
            Managers.Tracker.MissedKeyPress(attackType.ToString());
        }
    }

    float xMin, xMax, yMin, yMax;
    private void InvestScreenSize()
    {
        Camera cam = Camera.main;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        xMin = -halfWidth;
        xMax = halfWidth;
        yMin = -halfHeight;
        yMax = halfHeight;
    }
    private void PosAndActivateNode(DiagonalType type)
    {
        float randX = 0f;
        float randY = 0f;

        switch (type)
        {
            case DiagonalType.LeftUp:
                randX = UnityEngine.Random.Range(xMin + boundaryOffset, -boundaryOffset);
                randY = UnityEngine.Random.Range(boundaryOffset, yMax - boundaryOffset);
                break;

            case DiagonalType.LeftDown:
                randX = UnityEngine.Random.Range(xMin + boundaryOffset, -boundaryOffset);
                randY = UnityEngine.Random.Range(yMin + boundaryOffset, -boundaryOffset);
                break;

            case DiagonalType.RightUp:
                randX = UnityEngine.Random.Range(boundaryOffset, xMax - boundaryOffset);
                randY = UnityEngine.Random.Range(boundaryOffset, yMax - boundaryOffset);
                break;

            case DiagonalType.RightDown:
                randX = UnityEngine.Random.Range(boundaryOffset, xMax - boundaryOffset);
                randY = UnityEngine.Random.Range(yMin + boundaryOffset, -boundaryOffset);
                break;
        }

        diagonalDict[type].transform.position = new Vector3(randX, randY, 0f);
        diagonalDict[type].SetActive(true);
    }
}