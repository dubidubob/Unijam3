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

        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.deltaTime;  // ��� �ð� ����
            FillingBoot.fillAmount = Mathf.Lerp(startValue, 1f, elapsedTime / 0.2f);  // fillAmount ��ȭ
            yield return null;  // ���� �����ӱ��� ���
        }

        // fillAmount�� 1�� ��Ȯ�� ������
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
            MiddleText.text = "���ӽ���";
            MiddleText.color = Color.red;
            DownText.text = "��������";

            DoTweenScaleUp(GameStartBlank.transform);

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
        if(gameStartClicked == true) // �̰� ���� �������� ��ư��.
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
        else if(gameOptionClicked==true) // �̰� ���� �ɼǳ������ư��
        {
            DoTweenScaleDown(GameStartBlank.transform);
            UpText.text = "���ӽ���";
            MiddleText.text = "���Ӽ���";
            MiddleText.color = Color.black;
            DownText.text = "������";
            gameStartClicked = false;
        }
    }
}
