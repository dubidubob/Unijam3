using System.Collections.Generic;
using UnityEngine;

public class MouseClickMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;

    public Define.MonsterType MonsterType => Define.MonsterType.MouseClick;

    public void Spawn(MonsterData data)
    {
        if (Random.Range(0, 2) == 0)
            LeftOne.SetActive(true);
        else RightOne.SetActive(true);
    }
}

