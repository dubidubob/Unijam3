using System.Collections;
using UnityEngine;

public class MouseClickMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;

    public Define.MonsterType MonsterType => Define.MonsterType.MouseClick;

    private void Awake()
    {
        LeftOne.SetActive(false);
        RightOne.SetActive(false);

        Managers.Input.MouseAction -= DeactivateMouse;
        Managers.Input.MouseAction += DeactivateMouse;
    }

    private void DeactivateMouse(GamePlayDefine.MouseType mouseType)
    { 
        GameObject deactivateGo = mouseType == GamePlayDefine.MouseType.Left? LeftOne : RightOne;
        deactivateGo.SetActive(false);
    }

    private bool _spawning;

    public void Spawn(MonsterData data)
    {
        float spawnDuration = (float)IngameData.BeatInterval * data.spawnBeat;
        _spawning = true;
        StartCoroutine(DoSpawn(spawnDuration));
    }

    public void UnSpawn()
    { 
        _spawning = false;
        StopAllCoroutines();
    }

    private IEnumerator DoSpawn(float spawnDuration)
    {
        var wait = new WaitForSecondsRealtime(spawnDuration);
        while (_spawning)
        {
            yield return wait;
            ActivateEnemy();
        }
        yield return null;
    }

    public void ActivateEnemy()
    {
        var first = (Random.Range(0, 2) == 0) ? LeftOne : RightOne;
        var second = (first == LeftOne) ? RightOne : LeftOne;

        if (!first.activeSelf) first.SetActive(true);
        else if (!second.activeSelf) second.SetActive(true);
    }
}

