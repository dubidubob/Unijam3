using System.Collections;
using UnityEngine;

// TODO : poolable 해야하고, input에서 삭제하고, spawner에서 생성하는 거
public abstract class BaseMonsterSpawner : MonoBehaviour, IActiveMonsterRouter
{
    [SerializeField] MonsterTypeSO enemyTypeSO;
    private Poolable poolable;

    public float AppearTiming;

    //timer 동안 생성
    private IEnumerator MovingTimer() { yield return null; }

    public void Register(GameObject go)
    {
        // object pool에서 가져와서
        // _router queue에 넣는다.
    }

    public bool TryConsume(KeyCode key)
    {
        return false;
    }
}
