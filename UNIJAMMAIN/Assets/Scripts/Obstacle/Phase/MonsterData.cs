[System.Serializable]
public struct MonsterData
{
    public bool isIn;
    public Define.MonsterType monsterType;
    public float spawnDuration;
    public int numInRow;
    public float speedUpRate;

    public float MovingToHolderTime =>spawnDuration*numInRow;
}