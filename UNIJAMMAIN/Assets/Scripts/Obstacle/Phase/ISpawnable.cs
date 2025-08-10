public interface ISpawnable
{
    Define.MonsterType MonsterType { get; }
    void Spawn(MonsterData data);
}