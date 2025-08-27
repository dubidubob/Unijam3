public interface ISpawnable
{
    Define.MonsterType MonsterType { get; }
    void Spawn(MonsterData data);
    void UnSpawn();
}

public interface ISpawnManageable
{
    void Deactivate();
}