using UnityEngine;

public interface IActiveMonsterRouter
{
    // 스폰된 몬스터 큐에 등록
    void Register(GameObject go);

    // 키 들어옴에 큐에서 삭제
    bool TryConsume(KeyCode key);
}
