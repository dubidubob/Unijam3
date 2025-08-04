using UnityEngine;

public class MonsterData
{
    public Define.MonsterType monsterType;
    public float spawnDuration;
    public float moveToHolderDuration;

    public float GetSpawnDuration()
    {
        // TODO : Diagonal Random�� ���...?
        bool DiagonalRandom = false;
        if (DiagonalRandom)
        {
            int step = Random.Range(8, 31);  // 31�� exclusive �� 8..30             
            spawnDuration = step * 0.1f;
        }
        return spawnDuration;
    }
}