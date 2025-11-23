using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.Collections;
using Assets.VFXPACK_IMPACT_WALLCOEUR.Scripts;

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

    private void Start()
    {
        StartCoroutine(waitForEffectLoading(0.4f));
        if (type2 == GamePlayDefine.DiagonalType.MaxCnt)
        {
            IngameData.OnPerfectEffect -= PlayPerfectEffect;
            IngameData.OnPerfectEffect += PlayPerfectEffect;
        }
    }

   public IEnumerator waitForEffectLoading(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

       

        if (getKeyParticle == null&&type2==GamePlayDefine.DiagonalType.MaxCnt)
        {
            if (getKeyParticle == null && type2 == GamePlayDefine.DiagonalType.MaxCnt)
            {
                // 1. 소스 가져오기
                GameObject vfxSource0 = Managers.Game.vfxController.GetVfx(0, transform.position);
                if (vfxSource0 != null)
                {
                    // [수정] 생성된 인스턴스를 변수(instance0)에 담습니다.
                    GameObject instance0 = Instantiate(vfxSource0, transform);
                    instance0.transform.localPosition = Vector3.zero; // 위치 초기화 (필요시)
                    instance0.SetActive(false); // 끄고 시작
                    getKeyParticle = instance0.GetComponent<ParticleSystem>();
                }

                // 2. 퍼펙트 이펙트 소스 가져오기
                GameObject vfxSource1 = Managers.Game.vfxController.GetVfx(1, transform.position);
                if (vfxSource1 != null)
                {
                    // [수정] 생성된 인스턴스를 변수(instance1)에 담습니다.
                    GameObject instance1 = Instantiate(vfxSource1, transform);
                    instance1.transform.localPosition = Vector3.zero;
                    instance1.SetActive(false);
                    perfectEffectParticle = instance1.GetComponent<ParticleSystem>();
                }
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
    }

    private void TurnUIEffect(GamePlayDefine.DiagonalType t)
    {
        if (type2 == t)
        {
            sp[0].enabled = true;
            Invoke("TurnOff", 0.2f);
        }
    }

    bool wasEffectOn = false;
    private void TurnUIEffect(GamePlayDefine.WASDType t)
    {

        if (type == t)
        {
            // sp[0].color = new Color32(0xFF, 0xFB, 0x37, 0xFF);
            sp[1].sprite = candidate;
            sp[2].gameObject.SetActive(true);
            if(!wasEffectOn)
            {
                getKeyParticle.gameObject.SetActive(true);
                wasEffectOn = true;
            }
            getKeyParticle.Play();
            Invoke("TurnOff", 0.2f);
            
           
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
        // [중요] 내 키 타입과 맞는지 확인해야 모든 키에서 이펙트가 터지는 것을 방지합니다.
        if (type != wasdType) return;

        if (!wasPerfectEffectOn)
        {
            Debug.Log($"{gameObject.name}");
            perfectEffectParticle.gameObject.SetActive(true);
            wasPerfectEffectOn = true;
        }
        perfectEffectParticle.Play();
    }
}
