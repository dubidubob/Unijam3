using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class PopUpMainScene : UI_Popup
{
    public TMP_Text GuideText;
    public TMP_Text GuideTextBald;
    public TMP_Text Guide;
    public TMP_Text UpText;
    public TMP_Text MiddleText;
    public TMP_Text DownText;
    private string text;

    private bool gameStartClicked = false;
    private bool gameOptionClicked = false;
    public GameObject GameStartBlank;
    public GameObject GameStart;

    public string getText0, getText1;
    public Image FillingBoot;


    private float delay = 0.05f;
    enum Buttons
    {
        GameOption,
        GameOut,
        GameStart
    }
    private void Start()
    {
        Init();
        text = getText0;

    }

    public override void Init()
    {
        base.Init();
        SetResolution();
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.GameOption).gameObject.AddUIEvent(GameOptionClicked);
        GetButton((int)Buttons.GameOut).gameObject.AddUIEvent(GameOut);
        GetButton((int)Buttons.GameStart).gameObject.AddUIEvent(GameStartClicked);
        if (!Managers.Sound._audioSources[(int)Define.Sound.BGM].isPlaying)
        {
            Managers.Sound.Play("Sounds/BGM/Main_Bgm");
        }
    }
    

    IEnumerator textPrint(float delay=0.15f)
    {
        yield return new WaitForSeconds(1f);
        Guide = GuideTextBald;
        for (int i = 0; i < 2; i++)
        {   
            int count = 0; // 초기화
            while (count != text.Length) // 모두출력할때까지 다른행위 못함.
            {
                if (count < text.Length)
                {
                    Guide.text += text[count].ToString(); // 텍스트 천천히 출력중
                    count++;
                }

                yield return new WaitForSeconds(delay); // 출력속도 조절
            }
            for (int j = 0; j < 4; j++) // 모두 출력뒤 잠시 딜레이
            {
                yield return new WaitForSeconds(delay * 2);
            }

            Guide.text += "\n"; // 초기화
            if (i == 0) text = getText1; // 입력값 받기
            Guide = GuideText;
        }

        text = getText0;
        StartCoroutine(FillingBootGo());
        yield return null;


    }
    private IEnumerator FillingBootGo()
    {
        float elapsedTime = 0f;
        float startValue = FillingBoot.fillAmount;  // 시작 값은 현재 fillAmount

        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.deltaTime;  // 경과 시간 증가
            FillingBoot.fillAmount = Mathf.Lerp(startValue, 1f, elapsedTime / 0.2f);  // fillAmount 변화
            yield return null;  // 다음 프레임까지 대기
        }

        // fillAmount가 1로 정확히 설정됨
        FillingBoot.fillAmount = 1f;
    }



    public void DoTweenScaleUp(Transform transform, float upScaleAmount = 10f)
    {
        // transform.DOScale(Vector3.one * upScaleAmount, 0.2f);
        //   .OnComplete(() => transform.DOScale(Vector3.one, 0.2f));

        transform.DOScaleY(upScaleAmount, 0.5f);
        transform.DOLocalMoveY(60, 0.5f);
        GameStart.transform.DOLocalMoveY(360, 0.5f);
        
    }

    public void DoTweenScaleDown(Transform transform, float downScaleAmount = 1f)
    {
        transform.DOScaleY(downScaleAmount, 0.5f);
        transform.DOLocalMoveY(-190, 0.5f);
        GameStart.transform.DOLocalMoveY(-190, 0.5f);
    }


    void GameStartClicked(PointerEventData eventData)
    {
        if (gameStartClicked == false)
        {
            UpText.text = "";
            MiddleText.text = "게임시작";
            MiddleText.color = Color.red;
            DownText.text = "메인으로";

            DoTweenScaleUp(GameStartBlank.transform);

            StartCoroutine(textPrint(delay));

            gameStartClicked = true;
        }
        else
        {
            //아무것도안함.
                
         };

    }

    void GameOptionClicked(PointerEventData eventData)
    {
        if (gameStartClicked == true) // Nothing
        {
            Managers.Scene.LoadScene(Define.Scene.GamePlayScene);
            return;
        }
        else //이건 옵션버튼
        {
            DoTweenScaleUp(GameStartBlank.transform);
            UpText.text = "";
            MiddleText.text = "";
            DownText.text = "메인으로";
            gameOptionClicked = true;
        }

    }
    void GameOut(PointerEventData eventData)
    {
        if(gameStartClicked == true) // 이건 이제 메인으로 버튼임.
        {
            GuideTextBald.text = "";
            FillingBoot.fillAmount = 0;
            DoTweenScaleDown(GameStartBlank.transform);
            StopAllCoroutines();
            GuideText.text = "";
            UpText.text = "게임시작";
            MiddleText.text = "게임설정";
            MiddleText.color = Color.black;
            DownText.text = "나가기";
            gameStartClicked = false;
            text = getText0;


        }
        else if(gameOptionClicked==true) // 이건 이제 옵션나가기버튼임
        {
            DoTweenScaleDown(GameStartBlank.transform);
            UpText.text = "게임시작";
            MiddleText.text = "게임설정";
            MiddleText.color = Color.black;
            DownText.text = "나가기";
            gameStartClicked = false;
        }
    }
}
