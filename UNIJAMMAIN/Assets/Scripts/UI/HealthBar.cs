using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
public class MainUIs : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboTxt;
    [SerializeField] private Slider healthSilder;
    private int MaxHealth;
    void Start()
    {
        Managers.Game.ComboContinue -= UpdateCombo;
        Managers.Game.ComboContinue += UpdateCombo;
        Managers.Game.HealthUpdate -= HealthDec;
        Managers.Game.HealthUpdate += HealthDec;

        MaxHealth = Managers.Game.MaxHealth;
    }

    private void UpdateCombo(int combo)
    {
        comboTxt.text = combo.ToString();
        comboTxt.transform
            .DOScale(Vector3.one * 1.2f, 0.2f)
            .OnComplete(() =>
                comboTxt.transform.DOScale(Vector3.one, 0.1f)
            );
    }

    private void HealthDec(int health)
    {
        healthSilder.value = (float)health / (float)MaxHealth;
        Debug.Log($"healthSilder.value {healthSilder.value}");
        if (healthSilder.value == 0)
        { 
            healthSilder.fillRect.gameObject.SetActive(false);
        }
    }
}

