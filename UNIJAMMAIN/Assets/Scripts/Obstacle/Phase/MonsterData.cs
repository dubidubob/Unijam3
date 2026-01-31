[System.Serializable]
public struct MonsterData
{
    public bool isIn;
    public Define.MonsterType monsterType;
    public string WASD_Pattern;
    public float speedUpRate;
    public float spawnBeat;
    public float moveBeat;
    public bool hiding;
    public MouseEnemy.Dir dir;
    public float cameraActionDuration;
}