using UnityEngine;

[System.Serializable]
public struct MonsterData
{
    public bool isIn;
    public Define.MonsterType monsterType;
    [Min(1)] public int bpm;
    [Min(1)] public double numInRow;
    [Min(0.0001f)] public double speedUpRate;
    public double SpawnDuration => (60d / bpm) / speedUpRate;
    public double MoveToHolderDuration => SpawnDuration * numInRow;

}