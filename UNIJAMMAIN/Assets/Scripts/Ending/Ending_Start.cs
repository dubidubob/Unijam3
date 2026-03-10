using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro; // TextMeshPro를 사용하신다면 Text 대신 TextMeshProUGUI로 변경하세요.
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;

public class Ending_Start : MonoBehaviour
{
    EndingController endingController;

    [SerializeField] public TMP_Text textUp;
    [SerializeField] public TMP_Text textDown;
    [SerializeField] public List<Sprite> stamina_effects;
    [SerializeField] public Image image_stamina;
    [SerializeField] public Image blackPanelBack;
    [SerializeField] public Image blackPanelFront;

    private bool isClickable = false; // 중복 클릭 및 연출 중 클릭 방지

    // 버튼 이벤트나 EventTrigger 등에서 호출되도록 public으로 변경
    public void OnClick()
    {
        if (!isClickable) return; // 연출이 끝  나야 클릭 가능
        isClickable = false;      // 한 번 클릭되면 다시 클릭되지 않도록 처리

        endingController.PlayEndingSequence().Forget();
    }

    public void ConnectWithController(EndingController controller)
    {
        endingController = controller;
        textUp.text = "";
        textDown.text = "";

        StartAction_0().Forget();
    }

    public async UniTask StartAction_0() // 진입시 이미 암전상태 
    {
        await blackPanelFront.DOFade(0f, 1f).ToUniTask();

        // 1. 초기 텍스트 투명도 0 세팅 (보이지 않는 상태)
        textUp.color = new Color(textUp.color.r, textUp.color.g, textUp.color.b, 0f);
        textDown.color = new Color(textDown.color.r, textDown.color.g, textDown.color.b, 0f);
        isClickable = false;

        // 2. textUp 첫 번째 대사 페이드인
        // ※ LocalizationManager는 사용하시는 현지화 매니저 클래스로 가정했습니다.
        textUp.text = LocalizationManager.Get("Ending_Start_0");
      
        await textUp.DOFade(1f, 1.5f).ToUniTask();

        // 3. 잠깐 기다렸다가 (1초 대기)
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

        // (자연스러운 전환을 위해 기존 텍스트 잠시 페이드아웃)
        await textUp.DOFade(0f, 1f).ToUniTask();

        // 4. textUp 두 번째 대사 페이드인 
        textUp.text = LocalizationManager.Get("Ending_Start_1");
        await textUp.DOFade(1f, 1.5f).ToUniTask();

        // 5. 잠깐 기다렸다가 (1초 대기)
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

        // (자연스러운 전환을 위해 기존 텍스트 잠시 페이드아웃)
        await textUp.DOFade(0f, 0.5f).ToUniTask();
        

        // 6. textUp, textDown 두 개 동시에 페이드인
        textUp.text = LocalizationManager.Get("Ending_Start_2");
      
        Debug.Log($"textUp.text= {textUp.text}");
        var fadeUpi = blackPanelBack.DOFade(150f / 255f, 1f).ToUniTask();
        var fadeUp = textUp.DOFade(1f, 1f).ToUniTask();
        

        await UniTask.WhenAll(fadeUpi,fadeUp); // 두 페이드인 애니메이션이 끝날 때까지 동시 대기

        textDown.text = LocalizationManager.Get("Ending_Start_3");
        textDown.DOFade(1f, 0.5f).ToUniTask();

        isClickable = true;
    }
}