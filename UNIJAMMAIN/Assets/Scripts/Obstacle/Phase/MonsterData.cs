using UnityEngine;

[System.Serializable]
public struct MonsterData
{
    public bool isIn;

    public Define.MonsterType monsterType;
    [Min(1)] public int bpm;
    [Min(1)] public double numInRow;
    [Min(0.0001f)] public double speedUpRate;
    [Min(0)] public double startBeatOffset;

    public double Interval => (60d / bpm) / speedUpRate;
    public double MovingToHolderTime => Interval * numInRow;

}

public readonly struct MonsterRuntime
{
    public readonly Define.MonsterType type;
    public readonly double interval;       // = 60/bpm * spawnScale (패턴에 따라)
    public readonly double leadTime;       // = MoveToHolderDuration
    public readonly ISpawnable spawner;

    public MonsterRuntime(Define.MonsterType type, double interval, double leadTime, ISpawnable spawner)
    {
        this.type = type;
        this.interval = interval;
        this.leadTime = leadTime;
        this.spawner = spawner;
    }
}