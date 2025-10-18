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
        // 1. �����ϱ� ���� ũ�⸦ 0���� ����
        downDarkEffect.transform.localScale = Vector3.zero;

        // 2. Ȱ��ȭ (���� ��Ȱ��ȭ ���¿��ٸ�)
        downDarkEffect.gameObject.SetActive(true);

        // 3. ��ǥ ������ �� ���� (�� ���� ��������Ʈ�� ���� ũ�⿡ ���� �����ؾ� �մϴ�)
        // ����: ���� ũ�Ⱑ 1x1 �ȼ�¥�� �׸���, 
        // x: 400, y: 55 �� �����ϸ� ���ϴ� ũ�Ⱑ �˴ϴ�.
        Vector3 targetScale = new Vector3(400f, 55f, 1f);

        // 4. DOScale�� ����� �ִϸ��̼� ����
        // ������ �ð�(animationDuration) ���� targetScale�� �ε巴�� ����˴ϴ�.
        downDarkEffect.transform.DOScale(targetScale, animationDuration)
                                .SetEase(Ease.OutQuad); // �ε巯�� ȿ�� (�پ��� Ease �ɼ� ���� ����)

        // �ڷ�ƾ�� ��� ������� �ʵ��� �ִϸ��̼��� ���� ������ ��ٸ��ϴ�.
        yield return new WaitForSeconds(animationDuration);

        // (����) �ִϸ��̼��� ���� �� �ٸ� �۾��� ���⿡ �߰��� �� �ֽ��ϴ�.
        Debug.Log("�ִϸ��̼� ����!");
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
        float dx = 0f; // �޺� �޼��� �߰��� ������ effect
        
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


