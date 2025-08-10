using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
public class MainUIs : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboTxt;
    [SerializeField] private Slider healthSilder;
    [SerializeField] private Slider comboSilder;
    [SerializeField] private Image Combobangsa;
    [SerializeField] private List<Sprite> Bgs;
    [SerializeField] private Image bgPlace;
    [SerializeField] private BlurController blurController;
    private int MaxHealth;
    private float healthValue;
    void Awake()
    {
        Managers.Game.ComboContinue -= UpdateCombo;
        Managers.Game.ComboContinue += UpdateCombo;
        Managers.Game.HealthUpdate -= HealthDec;
        Managers.Game.HealthUpdate += HealthDec;
        Managers.Game.PhaseUpdate -= ChangeBg;
        Managers.Game.PhaseUpdate += ChangeBg;

        MaxHealth = Managers.Game.MaxHealth;

        ChangeBg(Managers.Game.GetPhase());
    }

    private void HealthDec(int health)
    {
        // 임시로 4단계로 표현
        healthValue = (float)health / (float)MaxHealth;
        blurController.SetBlur(healthValue); // 체력값을 비교하여 블러표시

        /* 기존 방식 -> slide value 표시
        healthSilder.value = (float)health / (float)MaxHealth;

        if (healthSilder.value == 0)
        {
            healthSilder.fillRect.gameObject.SetActive(false);
        }
        else
        {
            healthSilder.fillRect.gameObject.SetActive(true);
        }
        */
    }

    private void UpdateCombo(int combo)
    {
        if (combo > 0 && combo % 10 == 0)
        {
            ShowComboBangsa();
        }

        UpdateComboSlide(combo);

        comboTxt.text = combo.ToString();
        comboTxt.transform
            .DOScale(Vector3.one * 1.2f, 0.2f)
            .OnComplete(() =>
                comboTxt.transform.DOScale(Vector3.one, 0.1f)
            );
    }

    private void UpdateComboSlide(int combo)
    {
        int value = combo % 10;
        comboSilder.value = (float)value / 10f;
        if (comboSilder.value == 0)
        {
            comboSilder.fillRect.gameObject.SetActive(false);
        }
        else
        {
            comboSilder.fillRect.gameObject.SetActive(true);
        }
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

    private void ChangeBg(int phase)
    {
        if (Bgs.Count <= phase)
            return;

        if (phase > 0 && bgPlace!=null)
            bgPlace.sprite = Bgs[phase - 1];
    }
}


