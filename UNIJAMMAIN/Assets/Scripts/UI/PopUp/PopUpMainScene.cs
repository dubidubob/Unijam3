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
    public TMP_Text UpText;
    public TMP_Text MiddleText;
    public TMP_Text DownText;
    private string text;

    private bool gameStartClicked = false;
    private bool gameOptionClicked = false;
    public GameObject GameStartBlank;
    public GameObject GameStart;

    public string getText0, getText1;


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
    }
    void GameStartClicked(PointerEventData eventData)
    {
        UpText.text = "";
        MiddleText.text = "���ӽ���";
        MiddleText.color=Color.red;
        DownText.text = "��������";

        DoTweenScaleUp(GameStartBlank.transform);
        gameStartClicked = true;
       
    }

    IEnumerator textPrint(float delay=0.15f)
    {
        yield return new WaitForSeconds(1f);
       
        for (int i = 0; i < 3; i++)
        {
            int count = 0; // �ʱ�ȭ
            while (count != text.Length) // �������Ҷ����� �ٸ����� ����.
            {
                if (count < text.Length)
                {
                    GuideText.text += text[count].ToString(); // �ؽ�Ʈ õõ�� �����
                    count++;
                }

                yield return new WaitForSeconds(delay); // ��¼ӵ� ����
            }
            for (int j = 0; j < 4; j++) // ��� ��µ� ��� ������
            {
                yield return new WaitForSeconds(delay * 2);
            }

            GuideText.text += "\n"; // �ʱ�ȭ
            if (i == 0) text = getText0; // �Է°� �ޱ�
            else if (i == 1) text = getText1; // �Է°� �ޱ�
        }


        yield return null;


    }




    public void DoTweenScaleUp(Transform transform, float upScaleAmount = 10f)
    {
        // transform.DOScale(Vector3.one * upScaleAmount, 0.2f);
        //   .OnComplete(() => transform.DOScale(Vector3.one, 0.2f));

        transform.DOScaleY(upScaleAmount, 0.5f);
        transform.DOLocalMoveY(60, 0.5f);
        GameStart.transform.DOLocalMoveY(360, 0.5f);
        if (gameStartClicked == true)
        {
            StartCoroutine(textPrint(delay));
        }
    }

    public void DoTweenScaleDown(Transform transform, float downScaleAmount = 1f)
    {
        transform.DOScaleY(downScaleAmount, 0.5f);
        transform.DOLocalMoveY(-190, 0.5f);
        GameStart.transform.DOLocalMoveY(-190, 0.5f);
    }


    void GameOptionClicked(PointerEventData eventData)
    {
        if (gameStartClicked == true) // Nothing
        {

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
            DoTweenScaleDown(GameStartBlank.transform);
            StopAllCoroutines();
            GuideText.text = "";
            UpText.text = "���ӽ���";
            MiddleText.text = "���Ӽ���";
            MiddleText.color = Color.black;
            DownText.text = "������";
            gameStartClicked = false;
            
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
