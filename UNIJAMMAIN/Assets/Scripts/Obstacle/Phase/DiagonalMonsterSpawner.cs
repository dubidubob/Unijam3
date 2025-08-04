using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

public class DiagonalMonsterSpawner : MonoBehaviour, ISpawnable
{
    public Define.MonsterType MonsterType => Define.MonsterType.Diagonal;
    [SerializeField] float boundaryOffset = 1f;
    [SerializeField] Dictionary<DiagonalType, GameObject> allDiagonal;
    private List<int> activatedDiagonalIdx = new List<int>();

    private void Awake()
    {
        if (allDiagonal == null)
        {
            Debug.LogWarning("Place LU, LD, RU, RD in Inspector");
        }

        Managers.Input.KeyArrowcodeAction -= DeActivateEnemy;
        Managers.Input.KeyArrowcodeAction += DeActivateEnemy;

        InvestScreenSize();
    }

    public void Spawn(MonsterData data)
    {
        ActivateEnemy();
    }

    public void ActivateEnemy()
    {
        int idx;
        do
        {
            idx = Random.Range(0, allDiagonal.Count);
        }
        while (!activatedDiagonalIdx.Contains(idx));

        Debug.Assert(allDiagonal.Count <= idx, "Exceeded Num of Diagonal");

        PosAndActivateNode((DiagonalType)idx);
        activatedDiagonalIdx.Add(idx);
    }

    private void DeActivateEnemy(DiagonalType attackType)
    {
        if (activatedDiagonalIdx.Contains((int)attackType))
        {
            activatedDiagonalIdx.Remove((int)attackType);
            allDiagonal[attackType].GetComponent<RangedEnemy>().SetDead();
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
                randX = Random.Range(xMin + boundaryOffset, -boundaryOffset);
                randY = Random.Range(boundaryOffset, yMax - boundaryOffset);
                break;

            case DiagonalType.LeftDown:
                randX = Random.Range(xMin + boundaryOffset, -boundaryOffset);
                randY = Random.Range(yMin + boundaryOffset, -boundaryOffset);
                break;

            case DiagonalType.RightUp:
                randX = Random.Range(boundaryOffset, xMax - boundaryOffset);
                randY = Random.Range(boundaryOffset, yMax - boundaryOffset);
                break;

            case DiagonalType.RightDown:
                randX = Random.Range(boundaryOffset, xMax - boundaryOffset);
                randY = Random.Range(yMin + boundaryOffset, -boundaryOffset);
                break;
        }

        allDiagonal[type].transform.position = new Vector3(randX, randY, 0f);
        allDiagonal[type].SetActive(true);
    }
}