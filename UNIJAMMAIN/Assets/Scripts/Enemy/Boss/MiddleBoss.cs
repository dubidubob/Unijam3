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

    // 피해 이펙트 관련
    private bool isCoolDown;
    public Transform cameraTransform; // 흔들릴 카메라 Transform
    public float shakeStrength = 0.4f; // 흔들림 강도
    public float shakeDuration = 0.1f; // 흔들림 지속 시간

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
    IEnumerator StartChickenPattern(int i = 0) // 보스의 치킨 패턴 실행
    {
        nowBossChicken = Managers.UI.ShowPopUpBossPaternChicken<BossPaternChicken>();
        nowBossChicken.SetTimer(chickenTime[i]);
        nowBossChicken.SetArrowCount(chickenPatternCount[i]);
        nowBossChicken.SetMiddleBoss(this);
        StartCoroutine(nowBossChicken.StartPattern());

        yield return null;
    }

    public void EndChickenPattern() // 치킨 패턴 종료
    {
        nowPatternNumber++;
        if(nowPatternNumber>=chickenPatternMax) //패턴을 끝냈다면.
        {
            EndAllChickenPattern();
            return;
        }
        StartCoroutine(StartChickenPattern(nowPatternNumber));
    }

    public void InCorrectChickenPattern(int number) // 잘못된 패턴을 눌러서 틀렸을때, number은 틀린 갯수
    {
        nowIncorrectCount += number; // 틀린갯수 넘버 추가
        if(nowIncorrectCount>=canAllowCount) // 실패
        {
            // HP에 절반에 해당하는 데미지
        }
    }

    public void EndAllChickenPattern() // 모든 치킨패턴이 끝났을때
    {
        switch(nowIncorrectCount)
        {
            case 0: // 실패카운트가 0회 ,
                Managers.Game.perfect += 10;
                groggyTime = 5;
                // 그로기 5초

                break;

            case 1:
                Managers.Game.perfect += 8;
                groggyTime = 4;
                break;
                    // 그로기 4초
            case 2:
                Managers.Game.perfect += 5;
                groggyTime = 2; // 그로기 2초
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
        bossHP -= 1; // 데미지 1

        AttackEffect();

    }

    private void AttackEffect()
    {
        isCoolDown = true;
        attackedImage.DOKill();
        cameraTransform.DOKill(); // 이전 흔들림 중단

        // 피해 효과 UI 페이드
        Sequence seq = DOTween.Sequence();
        seq.Append(attackedImage.DOFade(0.8f, 0.1f));
        seq.Append(attackedImage.DOFade(0f, 0.3f));

        // 카메라 흔들림 효과 추가
        cameraTransform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: 8, // 흔들리는 횟수
            randomness: 90,
            snapping: false,
            fadeOut: true
        );

        seq.OnComplete(() => isCoolDown = false);
    }
}
