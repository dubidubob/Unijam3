using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
/**
 * [수정됨]
 * ISpawnable 인터페이스의 변경된 시그니처를 구현합니다.
 * 이 클래스는 이제 여러 개의 'MouseClickPatternInstance'를 관리하는 매니저 역할을 합니다.
 */
public class MouseClickMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;


    public Define.MonsterType MonsterType => Define.MonsterType.MouseClick;



    // [NEW] 현재 활성화된 모든 스폰 패턴(페이지)을 관리하는 리스트
    private List<MouseClickPatternInstance> _activePatterns = new List<MouseClickPatternInstance>();
    // [NEW] 리스트 순회 중 안전하게 제거하기 위한 목록
    private List<MouseClickPatternInstance> _patternsToRemove = new List<MouseClickPatternInstance>();

    [Header("Sequence Settings")]
    [SerializeField] private MouseSequenceData sequenceSettings; // 인스펙터에서 설정
    // 현재 활성화된 적을 제어하기 위해 프로퍼티나 변수로 노출
    public MouseEnemy CurrentEnemy { get; private set; }

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

    // --- (DeactivateMouse, ActivateEnemy 등 기존 로직은 대부분 유지) ---
    #region Existing Logic
    private void DeactivateMouse(GamePlayDefine.MouseType mouseType)
    {
        GameObject deactivateGo = mouseType == GamePlayDefine.MouseType.Left ? LeftOne : RightOne;
        deactivateGo.SetActive(false);
    }

    // [MODIFIED] 이 메서드는 이제 모든 인스턴스에 의해 공유됩니다.
    public void ActivateEnemy()
    {
        var first = (UnityEngine.Random.Range(0, 2) == 0) ? LeftOne : RightOne;
        var second = (first == LeftOne) ? RightOne : LeftOne;

        if (!first.activeSelf) first.SetActive(true);
        else if (!second.activeSelf) second.SetActive(true);
    }
    #endregion

    // ==================================================
    // [REFACTORED] ISpawnable 구현
    // ==================================================

    /**
     * [MODIFIED]
     * 새 스폰 패턴 인스턴스를 생성하고 리스트에 추가한 뒤 반환합니다.
     */
    public ISpawnable.ISpawnInstance Spawn(MonsterData data)
    {
        // 생성 시 설정값(sequenceSettings)을 같이 넘김
        MouseClickPatternInstance instance = new MouseClickPatternInstance(this, data, sequenceSettings);
        _activePatterns.Add(instance);
        instance.StartSpawning(data);
        return instance;
    }

    // 인스턴스가 호출할 헬퍼 메서드
    /// <summary>
    /// true라면 왼쪽 false라면 오른쪽
    /// </summary>
    /// <param name="isLeft"></param>
    public void ActivateEnemyForSequence(bool isLeft)
    {
        // 예시: 왼쪽/오른쪽 중 하나를 켜고 CurrentEnemy로 할당
        GameObject target = isLeft ? LeftOne : RightOne;
        target.SetActive(true);
        CurrentEnemy = target.GetComponent<MouseEnemy>();
    }

    /**
     * [MODIFIED]
     * 모든 활성 패턴을 중지하고 리스트를 비웁니다.
     */
    public void UnSpawnAll()
    {
        StopAllCoroutines();
        _activePatterns.Clear();
        _patternsToRemove.Clear();

        // (선택적) 활성화된 몬스터 강제 비활성화
        LeftOne.SetActive(false);
        RightOne.SetActive(false);
    }

    /**
     * [MODIFIED]
     * 모든 활성 패턴에 대해 일시정지를 적용합니다.
     */
    public void PauseForWhile(bool isStop)
    {
        double dspTime = AudioSettings.dspTime;
        foreach (var pattern in _activePatterns)
        {
            pattern.PauseForWhile(isStop, dspTime);
        }
    }

    /**
     * [NEW]
     * 인스턴스가 스스로 중지할 때 관리 리스트에서 제거하기 위해 호출됩니다.
     */
    public void RemovePattern(MouseClickPatternInstance pattern)
    {
        _patternsToRemove.Add(pattern);
    }

    /**
     * [NEW]
     * 스포너는 이제 Update문에서 실행중인 인스턴스를 관리합니다.
     */
    private void Update()
    {
        // 중지 요청된 패턴들(스스로 Stop()을 호출한)을 리스트에서 제거
        if (_patternsToRemove.Count > 0)
        {
            foreach (var pattern in _patternsToRemove)
            {
                _activePatterns.Remove(pattern);
            }
            _patternsToRemove.Clear();
        }
    }
}


// =======================================================================
// [NEW] 스폰 패턴(페이지)의 개별 인스턴스를 관리하는 헬퍼 클래스
// =======================================================================
public class MouseClickPatternInstance : ISpawnable.ISpawnInstance
{
    private MouseClickMonsterSpawner _parent;
    private MonsterData _data;
    private bool _spawning;
    private double _lastSpawnTime;
    private Coroutine _spawnCoroutine;
    private double _pauseStartTime;
    private float threshold = 2f;
    // 카메라 원본 회전값 저장용 (복귀를 위해)
    private Quaternion _defaultCameraRot;
    private Coroutine _sequenceCoroutine;

    private MouseSequenceData _seqData; // 위에서 정의한 설정값
    public MouseClickPatternInstance(MouseClickMonsterSpawner parent, MonsterData data,MouseSequenceData seqData)
    {
        _parent = parent;
        _data = data;
        _seqData = seqData;
        _spawning = true;

        _defaultCameraRot = Camera.main.transform.rotation;
        // [MOVED] SetLastSpawnTime 로직을 인스턴스 생성 시 처리
        if (IngameData.PhaseDurationSec == 0)
        {
            Debug.LogWarning("Set Up Phase Duration!");
        }
        _lastSpawnTime = AudioSettings.dspTime + IngameData.PhaseDurationSec - threshold;
    }
    private CancellationTokenSource _cts;

    public void StartSpawning(MonsterData data)
    {
        // 기존 CTS 정리
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        _cts = new CancellationTokenSource();

        // [핵심 수정 1] 
        // 부모(_parent)가 파괴되면 자동으로 취소되도록 토큰을 연결(LinkedToken)합니다.
        // 이렇게 하면 씬이 바뀌거나 오브젝트가 파괴될 때 Task도 즉시 멈춥니다.
        CancellationToken parentDestroyToken = _parent.GetCancellationTokenOnDestroy();
        CancellationToken linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, parentDestroyToken).Token;


        // 현재 비트에서 spawnBeat 이후에 정확히 출현하게끔 설정.
        // _data.spawnBeat 뒤에 출현하게끔 설정 
        // 연결된 토큰을 전달

        // 만약에 오브젝트가 파괴되었을때 이 기다림을 어떻게 멈출지도 알아서 생각
        RunSequence(linkedToken,data).Forget();
    }

    private async UniTaskVoid RunSequence(CancellationToken token, MonsterData data)
    {
        MouseEnemy myEnemy = null;
        bool sizeChanged = false;

        try
        {
            float secPerBeat = 60f / (float)IngameData.GameBpm;
            await UniTask.Delay(TimeSpan.FromSeconds(_data.spawnBeat * secPerBeat), cancellationToken: token);

            _parent.ActivateEnemyForSequence(data.dir == MouseEnemy.Dir.Left);
            myEnemy = _parent.CurrentEnemy;
            if (myEnemy == null) return;


            myEnemy.PlayFloatAction();
            await UniTask.Delay(TimeSpan.FromSeconds((float)IngameData.BeatInterval *_data.floatDuration), cancellationToken: token);
            CameraController.SetMonsterMode(true, _seqData.enlargementSize);

            System.Action slamImpactAction = () =>
            {
                int leftOrRight = (myEnemy.dir == MouseEnemy.Dir.Left) ? -1 : 1;
                Camera.main.transform.DOKill();
                // 회전 상태 고정
                Camera.main.transform.DORotate(new Vector3(0, 0, _seqData.tiltAngle * leftOrRight), 0.15f).SetEase(Ease.OutBack);
                Camera.main.transform.DOShakePosition(0.4f, 1.5f, 10);
            };
            myEnemy.PlaySlamAction(_seqData.slamAnimDuration, slamImpactAction);

            // [확대 상태 고정]
            Camera.main.DOKill(false);
            Camera.main.DOOrthoSize(CameraController.TargetBaseSize, _seqData.slamAnimDuration * 0.3f).SetEase(Ease.OutQuad);

            // [핵심] 지정된 duration 동안 아무도 카메라를 건드리지 못하게 대기
            await UniTask.Delay(TimeSpan.FromSeconds(_seqData.slamAnimDuration), cancellationToken: token);
            await UniTask.Delay(TimeSpan.FromSeconds((float)IngameData.BeatInterval * data.cameraActionDuration), cancellationToken: token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            // 이 인스턴스가 끝날 때 몬스터가 더 이상 없다면 잠금 해제
            // (만약 멀티 몬스터라면 별도의 카운팅이 필요하지만, 현재는 이 시퀀스 종료 시점에 맞춰 복구)
            CameraOriginalAction();
            Stop();
        }
    }

    public void CameraOriginalAction()
    {
        Camera.main.transform.DOKill();
        Camera.main.DOKill();

        Camera.main.transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutSine);
        Camera.main.DOOrthoSize(5f, 0.5f).SetEase(Ease.OutSine).OnComplete(()=>
        {
            CameraController.SetMonsterMode(false); // 카메라 액션까지 모두 끝나면 이제 원래대로 받기
        });
    }

    /* 이 인스턴스를 중지합니다.
    */
    public void Stop()
    {
        // 1. UniTask 취소
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

      
        // 4. 리스트에서 제거
        _parent.RemovePattern(this);
    }

    public void PauseForWhile(bool isStop, double dspTime)
    {
        if (isStop)
        {
            _pauseStartTime = dspTime;
        }
        else
        {
            if (_pauseStartTime > 0)
            {
                double pausedDuration = dspTime - _pauseStartTime;
                _lastSpawnTime += pausedDuration;
                _pauseStartTime = 0;
            }
        }
    }

    
    /**
     * [MOVED]
     * 스폰 코루틴 로직 (기존과 거의 동일)
     */
    private IEnumerator DoSpawn(float spawnDuration)
    {
        var wait = new WaitForSeconds(spawnDuration);
        while (_spawning)
        {
            if (AudioSettings.dspTime > _lastSpawnTime)
            {
                Stop(); // 이 인스턴스만 중지
                yield break;
            }

            _parent.ActivateEnemy(); // 부모의 공용 메서드 호출
            yield return wait;
        }
    }
    
}

[System.Serializable]
public class MouseSequenceData
{
    [Header("Timings (Beats)")]
    public int floatBeats = 8;        // 생성 후 둥둥 떠있는 시간 (박자)
    public int tiltHoldBeats = 24;    // 화면이 기울어진 상태 유지 시간 (박자)

    [Header("Timings (Fixed Seconds)")]
    public float slamAnimDuration = 2f;   // 내리찍는 애니메이션 시간 (초)
    public float recoverDuration = 1.0f;    // 카메라가 원래대로 돌아오는 시간 (초)

    [Header("Settings")]
    public float tiltAngle = 4f;     // 카메라 기울기 각도
    public float enlargementSize = 4f;
}