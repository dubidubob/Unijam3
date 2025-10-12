using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class MainUIs : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboTxt;
    [SerializeField] private Image Combobangsa;
    [SerializeField] private List<Sprite> Bgs;
    [SerializeField] private Image bgPlace;
    [SerializeField] private BlurController blurController;
    [SerializeField] private SpriteRenderer downDarkEffect;
    private int MaxHealth;
    private float beforeHealth = 100;
    void Awake()
    {
        Managers.Game.ComboContinue -= UpdateCombo;
        Managers.Game.ComboContinue += UpdateCombo;
        Managers.Game.HealthUpdate -= HealthChange;
        Managers.Game.HealthUpdate += HealthChange;

        MaxHealth = Managers.Game.MaxHealth;
        
        Combobangsa.SetNativeSize();

        ChangeBg(Managers.Game.GetPhase());
     

    }

    private void OnDestroy()
    {
        Managers.Game.ComboContinue -= UpdateCombo;
        Managers.Game.HealthUpdate -= HealthChange;
        transform.DOKill();
    }


    IEnumerator GoDownDarkEffectAnimation()
    {
        yield return new WaitForSeconds(3f);
        float animationDuration = 1.5f;
        // 1. 시작하기 전에 크기를 0으로 설정
        downDarkEffect.transform.localScale = Vector3.zero;

        // 2. 활성화 (만약 비활성화 상태였다면)
        downDarkEffect.gameObject.SetActive(true);

        // 3. 목표 스케일 값 설정 (이 값은 스프라이트의 원래 크기에 따라 조절해야 합니다)
        // 예시: 원래 크기가 1x1 픽셀짜리 네모라면, 
        // x: 400, y: 55 로 설정하면 원하는 크기가 됩니다.
        Vector3 targetScale = new Vector3(400f, 55f, 1f);

        // 4. DOScale을 사용해 애니메이션 실행
        // 지정한 시간(animationDuration) 동안 targetScale로 부드럽게 변경됩니다.
        downDarkEffect.transform.DOScale(targetScale, animationDuration)
                                .SetEase(Ease.OutQuad); // 부드러운 효과 (다양한 Ease 옵션 선택 가능)

        // 코루틴이 즉시 종료되지 않도록 애니메이션이 끝날 때까지 기다립니다.
        yield return new WaitForSeconds(animationDuration);

        // (선택) 애니메이션이 끝난 후 다른 작업을 여기에 추가할 수 있습니다.
        Debug.Log("애니메이션 종료!");
    }

    private void HealthChange(float health)
    {
 
        if (blurController != null)
        {
            blurController.SetBlur(health,MaxHealth); // 체력값을 비교하여 블러표시
        }

        if (Managers.Game.currentPlayerState == GameManager.PlayerState.Normal) // 현재 일반 몹과 상호작용 가능한 상태, 데미지를 받는 상태일때.
        {
            if(blurController!=null)
                blurController.SetBlur(health,MaxHealth); // 체력값을 비교하여 블러표시


            // 피해 입엇을때 효과와 몬스터 처치시 회복의 효과관련하여 관리 

            if (beforeHealth > health && blurController!=null) // 피해 입음
            {
                // 피해 받는 효과
                blurController.ShowDamageEffect();
            }
            else // 몬스터 처치
            {
                // 피해 회복 효과 있다면 적용
            }

            beforeHealth = health;
        }
    }

    private void UpdateCombo(int combo)
    {
        float dx = 0f; // 콤보 달성시 추가로 더해질 effect
        
        if (combo > 0 && combo % 10 == 0)
        {
            Combobangsa.gameObject.SetActive(true);
            Invoke("Deactivate", 0.3f);
            dx = 0.5f;
        }
        

        comboTxt.text = combo.ToString();
        comboTxt.transform
            .DOScale(Vector3.one * (1.2f+dx), 0.2f)
            .OnComplete(() =>
                comboTxt.transform.DOScale(Vector3.one, 0.1f)
            );

    }

    private void Deactivate()
    {
        Combobangsa.gameObject.SetActive(false);
    }

    private void ChangeBg(int phase)
    {
        if (Bgs.Count <= phase)
            return;

        if (phase > 0 && bgPlace!=null)
            bgPlace.sprite = Bgs[phase - 1];
    }
}


