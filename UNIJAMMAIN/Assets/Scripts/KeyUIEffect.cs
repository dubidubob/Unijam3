using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.Collections;
using Assets.VFXPACK_IMPACT_WALLCOEUR.Scripts;
using Cysharp.Threading.Tasks; // UniTask 네임스페이스 추가
using System.Threading; // CancellationTokenSource 사용을 위해 추가
using System;

public class KeyUIEffect : MonoBehaviour
{
    [SerializeField] bool isWasd = true;

    [SerializeField] GamePlayDefine.WASDType type = GamePlayDefine.WASDType.A;
    [SerializeField] Sprite candidate;

    [SerializeField] GamePlayDefine.DiagonalType type2 = GamePlayDefine.DiagonalType.RightUp;

    private Sprite basic;
    private Color baseColor;
    private SpriteRenderer[] sp;

    private GameObject _forInstantiate;
    private ParticleSystem getKeyParticle;
    private ParticleSystem perfectEffectParticle;

    // [UniTask] 끄기 예약을 취소하기 위한 토큰 소스
    private CancellationTokenSource _turnOffCts;

    private void Start()
    {
        // [UniTask] 코루틴 대신 비동기 메서드 호출 (Forget으로 실행 후 대기하지 않음)
        LoadEffectsAsync().Forget();

        if (type2 == GamePlayDefine.DiagonalType.MaxCnt)
        {
            IngameData.OnPerfectEffect -= PlayPerfectEffect;
            IngameData.OnPerfectEffect += PlayPerfectEffect;
        }
    }

    // [UniTask] WaitForSecondsRealtime 대체
    private async UniTaskVoid LoadEffectsAsync()
    {
        // 0.4초 리얼타임 대기 (이 오브젝트가 파괴되면 자동 취소됨)
        await UniTask.Delay(TimeSpan.FromSeconds(0.4f), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());

        // 조건문 정리 (중복 제거)
        if (getKeyParticle == null && type2 == GamePlayDefine.DiagonalType.MaxCnt)
        {
            // 1. 소스 가져오기
            GameObject vfxSource0 = Managers.Game.vfxController.GetVfx(0, transform.position);
            if (vfxSource0 != null)
            {
                GameObject instance0 = Instantiate(vfxSource0, transform);
                instance0.transform.localPosition = Vector3.zero;
                instance0.SetActive(false);
                getKeyParticle = instance0.GetComponent<ParticleSystem>();
            }

            // 2. 퍼펙트 이펙트 소스 가져오기
            GameObject vfxSource1 = Managers.Game.vfxController.GetVfx(1, transform.position);
            if (vfxSource1 != null)
            {
                GameObject instance1 = Instantiate(vfxSource1, transform);
                instance1.transform.localPosition = Vector3.zero;
                instance1.SetActive(false);
                perfectEffectParticle = instance1.GetComponent<ParticleSystem>();
            }
        }
    }

    private void Awake()
    {
        sp = GetComponentsInChildren<SpriteRenderer>(true);

        if (isWasd)
        {
            baseColor = sp[0].color;
            basic = sp[1].sprite;
            sp[2].gameObject.SetActive(false);

            Managers.Input.InputWasd -= TurnUIEffect;
            Managers.Input.InputWasd += TurnUIEffect;
        }
        else
        {
            sp[0].enabled = false;

            Managers.Input.InputDiagonal -= TurnUIEffect;
            Managers.Input.InputDiagonal += TurnUIEffect;
        }
    }

    private void OnDestroy()
    {
        Managers.Input.InputWasd -= TurnUIEffect;
        Managers.Input.InputDiagonal -= TurnUIEffect;
        IngameData.OnPerfectEffect -= PlayPerfectEffect;

        // [UniTask] 진행 중인 끄기 예약이 있다면 취소 및 리소스 해제
        if (_turnOffCts != null)
        {
            _turnOffCts.Cancel();
            _turnOffCts.Dispose();
        }
    }

    private void TurnUIEffect(GamePlayDefine.DiagonalType t)
    {
        if (type2 == t)
        {
            sp[0].enabled = true;
            // [UniTask] Invoke 대체
            ReserveTurnOff();
        }
    }

    bool wasEffectOn = false;
    private void TurnUIEffect(GamePlayDefine.WASDType t)
    {
        if (type == t)
        {
            sp[1].sprite = candidate;
            sp[2].gameObject.SetActive(true);

            // 파티클이 아직 로드되지 않았을 경우(0.4초 전) 에러 방지
            if (getKeyParticle != null)
            {
                if (!wasEffectOn)
                {
                    getKeyParticle.gameObject.SetActive(true);
                    wasEffectOn = true;
                }
                getKeyParticle.Play();
            }

            // [UniTask] Invoke 대체
            ReserveTurnOff();
        }
    }

    // [UniTask] 끄기 예약 로직
    private void ReserveTurnOff()
    {
        // 기존에 끄기 예약이 걸려있다면 취소 (연타 시 깜빡임 방지)
        if (_turnOffCts != null)
        {
            _turnOffCts.Cancel();
            _turnOffCts.Dispose();
        }

        // 새로운 토큰 생성 (Destroy 시 자동으로 캔슬되도록 링크)
        _turnOffCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

        // 비동기 끄기 실행
        TurnOffAsync(_turnOffCts.Token).Forget();
    }

    private async UniTaskVoid TurnOffAsync(CancellationToken token)
    {
        try
        {
            // 0.2초 대기
            await UniTask.Delay(200, cancellationToken: token);
            TurnOff();
        }
        catch (OperationCanceledException)
        {
            // 토큰이 취소되면(연타 or 파괴) 아무것도 하지 않음
        }
    }

    private void TurnOff()
    {
        if (isWasd)
        {
            sp[0].color = baseColor;
            sp[1].sprite = basic;
            sp[2].gameObject.SetActive(false);
        }
        else
        {
            sp[0].enabled = false;
        }
    }

    bool wasPerfectEffectOn = false;

    private void PlayPerfectEffect(GamePlayDefine.WASDType wasdType)
    {
        if (type != wasdType) return;

        // 로딩 전에 호출될 경우 방지
        if (perfectEffectParticle == null) return;

        if (!wasPerfectEffectOn)
        {
            perfectEffectParticle.gameObject.SetActive(true);
            wasPerfectEffectOn = true;
        }
        perfectEffectParticle.Play();
    }
}