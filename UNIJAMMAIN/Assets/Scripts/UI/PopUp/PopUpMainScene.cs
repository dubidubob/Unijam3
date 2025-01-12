using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Audio;
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

    public GameObject Option;
    public Sprite[] sprites;
    public Image OptionBGM, OptionSFX;
    private int BgmLevel = 2;
    private int SFXLevel = 2;

    private float delay = 0.05f;
    private bool canout = true;

    enum Buttons
    {
        GameOption,
        GameOut,
        GameStart,
        BGMControll,
        SFXControll
    }
    private void Start()
    {
        Init();
        text = getText0;
       // Managers.UI.ShowPopUpUI<S1_PopUp>();
    }

    public override void Init()
    {
        base.Init();
        SetResolution();
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.GameOption).gameObject.AddUIEvent(GameOptionClicked);
        GetButton((int)Buttons.GameOut).gameObject.AddUIEvent(GameOut);
        GetButton((int)Buttons.GameStart).gameObject.AddUIEvent(GameStartClicked);
        GetButton((int)Buttons.BGMControll).gameObject.AddUIEvent(BGMClicked);
        GetButton((int)Buttons.SFXControll).gameObject.AddUIEvent(SFXClicked);

        Option.SetActive(false);


        if (!Managers.Sound._audioSources[(int)Define.Sound.BGM].isPlaying)
        {
            Managers.Sound.Play("Sounds/BGM/Main_Bgm",Define.Sound.BGM);
            Managers.Sound._audioSources[1].volume = 0.4f;
            Managers.Sound._audioSources[2].volume = 0.4f;
           
        }
    }
    
    void BGMClicked(PointerEventData eventData)
    {
        Managers.Sound.Play("Sounds/SFX/Setting_Volume_Button_SFX");
        BgmLevel++;
        if (BgmLevel == 5) { BgmLevel = 0; }
        OptionBGM.sprite = sprites[BgmLevel];
        Managers.Sound._audioSources[1].volume = 0.2f*BgmLevel;
        
    }

    void SFXClicked(PointerEventData eventData)
    {
        Managers.Sound.Play("Sounds/SFX/Setting_Volume_Button_SFX");
        SFXLevel++;
        if(SFXLevel==5) { SFXLevel = 0; }
        OptionSFX.sprite = sprites[SFXLevel];
        Managers.Sound._audioSources[2].volume = 0.2f * SFXLevel;

    }

    IEnumerator textPrint(float delay=0.15f)
    {
        yield return new WaitForSeconds(0.3f);
        Guide = GuideTextBald;
        for (int i = 0; i < 2; i++)
        {   
            int count = 0; // �ʱ�ȭ
            while (count != text.Length) // �������Ҷ����� �ٸ����� ����.
            {
                if (count < text.Length)
                {
                    Guide.text += text[count].ToString(); // �ؽ�Ʈ õõ�� �����
                    count++;
                }

                yield return new WaitForSeconds(delay); // ��¼ӵ� ����
            }
            for (int j = 0; j < 4; j++) // ��� ��µ� ��� ������
            {
                yield return new WaitForSeconds(delay * 2);
            }

            Guide.text += "\n"; // �ʱ�ȭ
            if (i == 0) text = getText1; // �Է°� �ޱ�
            Guide = GuideText;
        }

        text = getText0;
        StartCoroutine(FillingBootGo());
        yield return null;


    }
    private IEnumerator FillingBootGo()
    {
        float elapsedTime = 0f;
        float startValue = FillingBoot.fillAmount;  // ���� ���� ���� fillAmount

        while (elapsedTime < 0.4f)
        {
            elapsedTime += Time.deltaTime;  // ��� �ð� ����
            FillingBoot.fillAmount = Mathf.Lerp(startValue, 1f, elapsedTime / 0.4f);  // fillAmount ��ȭ
            yield return null;  // ���� �����ӱ��� ���
        }

        // fillAmount�� 1�� ��Ȯ�� ������
        FillingBoot.fillAmount = 1f;
    }



    public void DoTweenScaleUpInStart(Transform transform, float upScaleAmount = 10f)
    {
        // transform.DOScale(Vector3.one * upScaleAmount, 0.2f);
        //   .OnComplete(() => transform.DOScale(Vector3.one, 0.2f));
        
        transform.DOScaleY(upScaleAmount, 0.5f);
        transform.DOLocalMoveY(60, 0.5f);
        canout = false;
        GameStart.transform.DOLocalMoveY(360, 0.5f)
            .OnComplete(() =>
            {
                canout = true;
            }
                );
   
    }
    public void DoTweenScaleUp(Transform transform, float upScaleAmount = 10f)
    {
        // transform.DOScale(Vector3.one * upScaleAmount, 0.2f);
        //   .OnComplete(() => transform.DOScale(Vector3.one, 0.2f));

        transform.DOScaleY(upScaleAmount, 0.5f);
        transform.DOLocalMoveY(60, 0.5f);
        canout = false;
        GameStart.transform.DOLocalMoveY(360, 0.5f)
            .OnComplete(() =>
            {
                Option.SetActive(true);
                canout = true;
            }
                );

    }

    public void DoTweenScaleDown(Transform transform, float downScaleAmount = 1f)
    {
        transform.DOScaleY(downScaleAmount, 0.5f);
        transform.DOLocalMoveY(-190, 0.5f);
        GameStart.transform.DOLocalMoveY(-190, 0.5f);
    }


    void GameStartClicked(PointerEventData eventData)
    {
        Managers.Sound.Play("Sounds/SFX/Main_Button_SFX");
        if (gameStartClicked == false)
        {
            
            UpText.text = "";
            MiddleText.text = "���ӽ���";
            MiddleText.color = Color.red;
            DownText.text = "��������";

            DoTweenScaleUpInStart(GameStartBlank.transform);

            StartCoroutine(textPrint(delay));

            gameStartClicked = true;
        }
        else
        {
            //�ƹ��͵�����.
                
         };

    }

    void GameOptionClicked(PointerEventData eventData)
    {
         Managers.Sound.Play("Sounds/SFX/Main_Button_SFX");
        if (gameStartClicked == true) // Nothing
        {
            Managers.Scene.LoadScene(Define.Scene.GamePlayScene);
            return;
        }
        else //�̰� �ɼǹ�ư
        {
            DoTweenScaleUp(GameStartBlank.transform);
            UpText.text = "";
            MiddleText.text = "";
            DownText.text = "��������";
            gameOptionClicked = true;
        }

    }
    void GameOut(PointerEventData eventData)
    {
        Managers.Sound.Play("Sounds/SFX/Main_Button_SFX");
        if (gameStartClicked == true&&canout==true) // �̰� ���� �������� ��ư��.
        {
            GuideTextBald.text = "";
            FillingBoot.fillAmount = 0;
            DoTweenScaleDown(GameStartBlank.transform);
            StopAllCoroutines();
            GuideText.text = "";
            UpText.text = "���ӽ���";
            MiddleText.text = "���Ӽ���";
            MiddleText.color = Color.black;
            DownText.text = "������";
            gameStartClicked = false;
            text = getText0;


        }
        else if(gameOptionClicked==true&&canout==true) // �̰� ���� �ɼǳ������ư��
        {
            DoTweenScaleDown(GameStartBlank.transform);
            UpText.text = "���ӽ���";
            MiddleText.text = "���Ӽ���";
            MiddleText.color = Color.black;
            DownText.text = "������";
            gameStartClicked = false;
            Option.SetActive(false);
        }
    }
}
