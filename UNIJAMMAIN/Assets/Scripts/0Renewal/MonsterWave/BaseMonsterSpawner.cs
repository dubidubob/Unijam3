using System.Collections;
using UnityEngine;

// TODO : poolable �ؾ��ϰ�, input���� �����ϰ�, spawner���� �����ϴ� ��
public abstract class BaseMonsterSpawner : MonoBehaviour, IActiveMonsterRouter
{
    [SerializeField] MonsterTypeSO enemyTypeSO;
    private Poolable poolable;

    public float AppearTiming;

    //timer ���� ����
    private IEnumerator MovingTimer() { yield return null; }

    public void Register(GameObject go)
    {
        // object pool���� �����ͼ�
        // _router queue�� �ִ´�.
    }

    public bool TryConsume(KeyCode key)
    {
        return false;
    }
}
