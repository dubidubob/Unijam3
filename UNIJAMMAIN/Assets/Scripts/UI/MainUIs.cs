using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainUIs : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboTxt;
    [SerializeField] private Image Combobangsa;
    [SerializeField] private List<Sprite> Bgs;
    [SerializeField] private Image bgPlace;
    [SerializeField] private BlurController blurController;
    private int MaxHealth;
    private float beforeHealth = 100;
    void Awake()
    {
        Managers.Game.ComboContinue -= UpdateCombo;
        Managers.Game.ComboContinue += UpdateCombo;
        Managers.Game.HealthUpdate -= HealthChange;
        Managers.Game.HealthUpdate += HealthChange;
        Managers.Game.PhaseUpdate -= ChangeBg;
        Managers.Game.PhaseUpdate += ChangeBg;

        MaxHealth = Managers.Game.MaxHealth;
        
        Combobangsa.SetNativeSize();

        ChangeBg(Managers.Game.GetPhase());

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
        if (combo > 0 && combo % 10 == 0)
        {
            Combobangsa.gameObject.SetActive(true);
        }

        comboTxt.text = combo.ToString();
        comboTxt.transform
            .DOScale(Vector3.one * 1.2f, 0.2f)
            .OnComplete(() =>
                comboTxt.transform.DOScale(Vector3.one, 0.1f)
            );
    }

    private void ChangeBg(int phase)
    {
        if (Bgs.Count <= phase)
            return;

        if (phase > 0 && bgPlace!=null)
            bgPlace.sprite = Bgs[phase - 1];
    }
}


