public interface ISpawnable
{
    Define.MonsterType MonsterType { get; }
    void Spawn(MonsterData data);
    void UnSpawn();
    void SetLastSpawnTime(float? beat);
}