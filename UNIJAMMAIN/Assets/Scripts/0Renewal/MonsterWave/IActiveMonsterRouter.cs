using UnityEngine;

public interface IActiveMonsterRouter
{
    // ������ ���� ť�� ���
    void Register(GameObject go);

    // Ű ���ȿ� ť���� ����
    bool TryConsume(KeyCode key);
}
