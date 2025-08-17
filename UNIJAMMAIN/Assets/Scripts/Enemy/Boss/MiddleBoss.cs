using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MiddleBoss : MonoBehaviour
{
    int bossHP = 1000;
    BossPaternChicken nowBossChicken;

    public Image attackedImage;

    [SerializeField] float[] chickenTime;
    [SerializeField] int[] chickenPatternCount;

    int chickenPatternMax = 3;
    int nowPatternNumber = 0;

    [SerializeField] int canAllowCount = 3;
    private int nowIncorrectCount =0;
    private int groggyTime;
    private bool groggyBool = false;

    // ���� ����Ʈ ����
    private bool isCoolDown;
    public Transform cameraTransform; // ��鸱 ī�޶� Transform
    public float shakeStrength = 0.4f; // ��鸲 ����
    public float shakeDuration = 0.1f; // ��鸲 ���� �ð�

    private void FixedUpdate()
    {
        if(groggyBool)
        {
            if(Input.GetKey(KeyCode.A))
            {
                AttackedBoss();
            }
            if(Input.GetKeyDown(KeyCode.D))
            {
                AttackedBoss();
            }
        }
    }
    private void Start()
    {
        //test
        StartCoroutine(StartChickenPattern(0));
        chickenPatternMax = chickenPatternCount.Length;
    }
    IEnumerator StartChickenPattern(int i = 0) // ������ ġŲ ���� ����
    {
        nowBossChicken = Managers.UI.ShowPopUpBossPaternChicken<BossPaternChicken>();
        nowBossChicken.SetTimer(chickenTime[i]);
        nowBossChicken.SetArrowCount(chickenPatternCount[i]);
        nowBossChicken.SetMiddleBoss(this);
        StartCoroutine(nowBossChicken.StartPattern());

        yield return null;
    }

    public void EndChickenPattern() // ġŲ ���� ����
    {
        nowPatternNumber++;
        if(nowPatternNumber>=chickenPatternMax) //������ ���´ٸ�.
        {
            EndAllChickenPattern();
            return;
        }
        StartCoroutine(StartChickenPattern(nowPatternNumber));
    }

    public void InCorrectChickenPattern(int number) // �߸��� ������ ������ Ʋ������, number�� Ʋ�� ����
    {
        nowIncorrectCount += number; // Ʋ������ �ѹ� �߰�
        if(nowIncorrectCount>=canAllowCount) // ����
        {
            // HP�� ���ݿ� �ش��ϴ� ������
        }
    }

    public void EndAllChickenPattern() // ��� ġŲ������ ��������
    {
        switch(nowIncorrectCount)
        {
            case 0: // ����ī��Ʈ�� 0ȸ ,
                Managers.Game.perfect += 10;
                groggyTime = 5;
                // �׷α� 5��

                break;

            case 1:
                Managers.Game.perfect += 8;
                groggyTime = 4;
                break;
                    // �׷α� 4��
            case 2:
                Managers.Game.perfect += 5;
                groggyTime = 2; // �׷α� 2��
                break;

            default: break;
        }

        StartCoroutine(GoGroggyTime(groggyTime));

    }

    IEnumerator GoGroggyTime(int time)
    {
        groggyBool = true;
        Managers.Game.currentPlayerState = GameManager.PlayerState.GroggyAttack;

        yield return new WaitForSeconds(time);

        Managers.Game.currentPlayerState = GameManager.PlayerState.Normal;
        groggyBool = false;
    }

    private void AttackedBoss()
    {
        bossHP -= 1; // ������ 1

        AttackEffect();

    }

    private void AttackEffect()
    {
        isCoolDown = true;
        attackedImage.DOKill();
        cameraTransform.DOKill(); // ���� ��鸲 �ߴ�

        // ���� ȿ�� UI ���̵�
        Sequence seq = DOTween.Sequence();
        seq.Append(attackedImage.DOFade(0.8f, 0.1f));
        seq.Append(attackedImage.DOFade(0f, 0.3f));

        // ī�޶� ��鸲 ȿ�� �߰�
        cameraTransform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: 8, // ��鸮�� Ƚ��
            randomness: 90,
            snapping: false,
            fadeOut: true
        );

        seq.OnComplete(() => isCoolDown = false);
    }
}
