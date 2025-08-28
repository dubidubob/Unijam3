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
            blurController.SetBlur(health,MaxHealth); // ü�°��� ���Ͽ� ��ǥ��
        }

        if (Managers.Game.currentPlayerState == GameManager.PlayerState.Normal) // ���� �Ϲ� ���� ��ȣ�ۿ� ������ ����, �������� �޴� �����϶�.
        {
            if(blurController!=null)
                blurController.SetBlur(health,MaxHealth); // ü�°��� ���Ͽ� ��ǥ��


            // ���� �Ծ����� ȿ���� ���� óġ�� ȸ���� ȿ�������Ͽ� ���� 

            if (beforeHealth > health && blurController!=null) // ���� ����
            {
                // ���� �޴� ȿ��
                blurController.ShowDamageEffect();
            }
            else // ���� óġ
            {
                // ���� ȸ�� ȿ�� �ִٸ� ����
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


