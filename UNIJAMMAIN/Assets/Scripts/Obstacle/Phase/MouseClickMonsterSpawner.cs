using System.Collections;
using UnityEngine;

public class MouseClickMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;

    public Define.MonsterType MonsterType => Define.MonsterType.MouseClick;
    private double _lastSpawnTime;

    private double _pauseStartTime;
    private void Awake()
    {
        LeftOne.SetActive(false);
        RightOne.SetActive(false);

        Managers.Input.InputMouse -= DeactivateMouse;
        Managers.Input.InputMouse += DeactivateMouse;
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;
    }

    private void OnDestroy()
    {
        Managers.Input.InputMouse -= DeactivateMouse;
        PauseManager.IsPaused -= PauseForWhile;
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
        SetLastSpawnTime();
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
        // Time.timeScale의 영향을 받는 WaitForSeconds를 사용해야 Pause가 제대로 동작합니다.
        var wait = new WaitForSeconds(spawnDuration);
        while (_spawning)
        {
            if (AudioSettings.dspTime > _lastSpawnTime)
            {
                UnSpawn();
                yield break;
            }

            ActivateEnemy();
            yield return wait;
        }
    }


    public void ActivateEnemy()
    {
        var first = (Random.Range(0, 2) == 0) ? LeftOne : RightOne;
        var second = (first == LeftOne) ? RightOne : LeftOne;

        if (!first.activeSelf) first.SetActive(true);
        else if (!second.activeSelf) second.SetActive(true);
    }

    private float threshold = 2f;
    public void SetLastSpawnTime(float? _=null)
    {
        if (IngameData.PhaseDurationSec == 0)
        {
            Debug.LogWarning("Set Up Phase Duration!");
        }
        _lastSpawnTime = AudioSettings.dspTime + IngameData.PhaseDurationSec - threshold;
    }

    public void PauseForWhile(bool isStop)
    {
        _spawning = !isStop;

        if (isStop)
        {
            // Pause 시작 시간 기록
            _pauseStartTime = AudioSettings.dspTime;
        }
        else
        {
            // Pause가 풀렸을 때, Pause된 시간만큼 스폰 종료 시간을 뒤로 밀어줌
            if (_pauseStartTime > 0)
            {
                double pausedDuration = AudioSettings.dspTime - _pauseStartTime;
                _lastSpawnTime += pausedDuration;
                _pauseStartTime = 0; // 초기화
            }
        }
    }
}

