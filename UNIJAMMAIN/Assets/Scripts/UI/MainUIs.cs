using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
public class MainUIs : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboTxt;
    [SerializeField] private Slider healthSilder;
    [SerializeField] private Image Combobangsa;
    [SerializeField] private List<Sprite> Bgs;
    [SerializeField] private Image bgPlace;
    private int MaxHealth;
    void Start()
    {
        Managers.Game.ComboContinue -= UpdateCombo;
        Managers.Game.ComboContinue += UpdateCombo;
        Managers.Game.HealthUpdate -= HealthDec;
        Managers.Game.HealthUpdate += HealthDec;
        Managers.Game.PhaseUpdate -= ChangeBg;
        Managers.Game.PhaseUpdate += ChangeBg;

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

    private void ShowComboBangsa()
    {
        Combobangsa.SetNativeSize();
        Combobangsa.gameObject.SetActive(true);

        Camera camera = Camera.main;
        camera.DOShakeRotation(
            duration: 1.5f,
            strength: new Vector3(0, 0, 90f),
            vibrato: 10,
            randomness: 15f,
            fadeOut: true
        ).SetEase(Ease.OutQuad)
        .OnComplete(()=> { Combobangsa.gameObject.SetActive(false); });
    }

    private void HealthDec(int health)
    {
        healthSilder.value = (float)health / (float)MaxHealth;
        
        if (healthSilder.value == 0)
        { 
            healthSilder.fillRect.gameObject.SetActive(false);
        }
    }

    private void ChangeBg(int phase)
    {
        if (phase > 0)
            bgPlace.sprite = Bgs[phase - 1];
        else
            Debug.LogWarning($"phase index out {phase}");
    }
}


