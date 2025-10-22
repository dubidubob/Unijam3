// ISpawnable.cs

public interface ISpawnable
{
    /**
     * 개별 스폰 작업(페이지)을 식별하고 제어하기 위한 핸들(Handle) 인터페이스입니다.
     * 이 인터페이스는 ISpawnable 내부에 중첩되어 정의됩니다. (public interface ISpawnInstance)
     */
    public interface ISpawnInstance
    {
        /**
         * 이 인스턴스(페이지)에 속한 몬스터의 스폰을 중지시킵니다.
         */
        void Stop();
    }

    Define.MonsterType MonsterType { get; }

    ISpawnInstance Spawn(MonsterData data);

 
    void PauseForWhile(bool isStop);

    
    void UnSpawnAll();
}