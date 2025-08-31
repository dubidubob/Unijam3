using DG.Tweening;
using KoreanTyper;                                                  // Add KoreanTyper namespace | ���� �����̽� �߰�
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryDialog : UI_Popup
{
    public Text[] TestTexts;
    public Image[] StandingImage;
    public GameObject TextPanel;
    public DOTweenAnimation[] StandingAnimations;
    public GameObject dimmedPanel;
    public GameObject TutorialDialog;
    public CharacterData[] cd;

    public Vector2 panelOffset;
    public GameObject contents;

    private RectTransform panelRect;
    private Vector2 originalPanelPos;
    public List<DialogueScene> scenes;

    public bool canGoNextStep = true;
    public GameObject shop;

    public GameObject[] leftSDAnimSet;
    public GameObject leftSDCharacter;
    public GameObject KongCanvas;

    public GameObject[] rightSDAnimSet;
    public GameObject rightSDCharacter;
    public GameObject GretCanvas;
    private void Awake()
    {
        panelRect = TextPanel.GetComponent<RectTransform>();
        originalPanelPos = panelRect.anchoredPosition;
        contents.SetActive(true);
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        StartCoroutine(TypingCoroutine());
    }

    public IEnumerator TypingCoroutine()
    {
        panelRect.anchoredPosition = originalPanelPos;

        for (int idx = 0; idx < scenes.Count; idx++)
        {

            DialogueScene scene = scenes[idx];

            yield return new WaitForSecondsRealtime(scene.preDelay);

            // ======================
            // 0. Hide Dialog ó��
            // ======================
            if (scene.speakingCharacter == DialogueCharacter.HideDialog)
            {
                TextPanel.SetActive(false);
                if (StandingImage != null)
                {
                    foreach (var standing in StandingImage)
                        standing.gameObject.SetActive(false);
                }
            }

            // ======================
            // 1. ĳ���� None ó��
            // ======================
            if (scene.speakingCharacter == DialogueCharacter.None)
            {
                // ĳ���� �̹��� ��� OFF
                StandingImage[0].gameObject.SetActive(false);
                StandingImage[1].gameObject.SetActive(false);
                // �г� ���� ��ġ (panelPositionOffset ����)
                if (panelRect != null)
                    panelRect.anchoredPosition = originalPanelPos;
            }
            else
            {
                // ======================
                // 2. �Ϲ� ĳ���� ó��
                // ======================
                // ĳ���� �ε��� None ����
                int charIdx = (int)scene.speakingCharacter - 2;

                // ���� ĳ���� ó��
                if (scene.showLeftCharacter)
                {

                    StandingImage[0].sprite = scene.overrideSprite != null ? scene.overrideSprite : cd[charIdx].CharacterImage;
                    StandingImage[0].gameObject.SetActive(true);
                    StandingImage[0].SetNativeSize();

                    if (scene.XFlip)
                    {
                        LeftCharacter.instance.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 180, 0);
                    }
                    else
                    {
                        LeftCharacter.instance.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                    }

                    if (scene.isFirstAppearance)
                    {
                        StandingImage[0].rectTransform.localScale = Vector3.one;

                        yield return new WaitForSecondsRealtime(2.2f);
                        TextPanel.SetActive(true);
                    }
                    else
                    {

                    }
                    if (scene.isAnger)
                    {
                        Debug.Log("���ʻ��ȭ����!");
                        LeftCharacter.instance.frowningAnim.DORestartById("1");
                    }
                    if (scene.isSurprized)
                    {
                        Debug.Log("���ʻ������!");
                        LeftCharacter.instance.frowningAnim.DORestartById("2");
                    }
                }

                else
                {
                    StandingImage[0].gameObject.SetActive(false);
                }

                // ������ ĳ���� ó��
                if (scene.showRightCharacter)
                {

                    StandingImage[1].sprite = scene.overrideSprite != null ? scene.overrideSprite : cd[charIdx].CharacterImage;
                    StandingImage[1].gameObject.SetActive(true);
                    StandingImage[1].SetNativeSize();
                    if (scene.XFlip)
                    {
                        RightCharacter.instance.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 180, 0);
                    }
                    else
                    {
                        RightCharacter.instance.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                    }

                }
                else
                {
                    StandingImage[1].gameObject.SetActive(false);
                }

                // �г� ��ġ (���� ��ġ + ������)
                if (panelRect != null)
                    panelRect.anchoredPosition = originalPanelPos + scene.panelPositionOffset;

                if (scene.showRightCharacter)
                {
                    if (scene.isFirstAppearance)
                    {

                        if (TextPanel != null)
                            TextPanel.SetActive(false);


                        if (TextPanel != null)
                            TextPanel.SetActive(true);

                    }
                    else
                    {
                        if (scene.isAnger)
                        {
                            Debug.Log("�����ʻ��ȭ����!");
                            RightCharacter.instance.frowningAnim.DORestartById("1");
                        }
                        if (scene.isSurprized)
                        {
                            Debug.Log("�����ʻ������!");
                            RightCharacter.instance.surprisedAnim.DORestartById("2");
                        }
                    }
                }
            }

            // �ؽ�Ʈ ǥ�� (Ÿ���� ȿ��)
            TestTexts[idx].gameObject.SetActive(true);
            string full = scene.text;
            int len = full.GetTypingLength();

            for (int i = 0; i <= len; i++)
            {
                TestTexts[idx].text = full.Typing(i);
                yield return new WaitForSecondsRealtime(0.025f);
            }

            if (scene.leftSDAnim || scene.rightSDAnim)
            {
                if (scene.requiredKey == KeyCode.None)
                {
                    while ((!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return)))
                    {
                        TestTexts[idx].text = full;
                        yield return null;
                    }

                }
                else
                {
                    while (!Input.GetKeyDown(scene.requiredKey))
                    {
                        TestTexts[idx].text = full;
                        yield return null;
                    }
                }

                // �Է¹����� TextPanel ����
                if (TextPanel != null)
                {
                    TextPanel.SetActive(false);
                    StandingImage[0].gameObject.SetActive(false);
                    StandingImage[1].gameObject.SetActive(false);
                }

                if (scene.leftSDAnim)
                {
                    var leftAnim = leftSDCharacter.GetComponent<DOTweenAnimation>();
                    var leftSR = leftSDCharacter.GetComponent<SpriteRenderer>();
                    leftSR.sortingOrder = 3;
                    if (leftAnim != null)
                        leftAnim.DORestart();


                    foreach (var obj in leftSDAnimSet)
                        if (obj != null) obj.SetActive(true);


                }
                if (scene.rightSDAnim)
                {
                    var rightAnim = rightSDCharacter.GetComponent<DOTweenAnimation>();
                    var rightSR = rightSDCharacter.GetComponent<SpriteRenderer>();
                    rightSR.sortingOrder = 3;
                    if (rightAnim != null)
                        rightAnim.DORestart();


                    foreach (var obj in rightSDAnimSet)
                        if (obj != null) obj.SetActive(true);


                }

                yield return new WaitForSecondsRealtime(1.5f);
                if (scene.leftSDAnim) KongCanvas.SetActive(true);
                if (scene.rightSDAnim) GretCanvas.SetActive(true);
                bool panelTurnedOn = false;
                while (!panelTurnedOn)
                {
                    // ���콺 �ƹ� ��ư Ŭ�� ��
                    if (Input.GetMouseButtonDown(0))
                    {


                        if (KongCanvas != null) KongCanvas.SetActive(false);
                        if (GretCanvas != null) GretCanvas.SetActive(false);
                        if (scene.leftSDAnim)
                        {
                            foreach (var obj in leftSDAnimSet)
                                if (obj != null) obj.SetActive(false);
                            var leftAnim = leftSDCharacter.GetComponent<DOTweenAnimation>();
                            var leftSR = leftSDCharacter.GetComponent<SpriteRenderer>();
                            leftAnim.DOPlayBackwards();
                            leftSR.sortingOrder = -3;
                        }
                        if (scene.rightSDAnim)
                        {
                            foreach (var obj in rightSDAnimSet)
                                if (obj != null) obj.SetActive(false);
                            var rightAnim = rightSDCharacter.GetComponent<DOTweenAnimation>();
                            var rightSR = rightSDCharacter.GetComponent<SpriteRenderer>();
                            rightAnim.DOPlayBackwards();
                            rightSR.sortingOrder = -3;
                        }


                        if (TextPanel != null)
                            TextPanel.SetActive(true);

                        panelTurnedOn = true;
                    }
                    yield return null;
                }

                // �ٷ� ���� ����!
                continue;
            }


            if (scene.requiredKey == KeyCode.None)
            {
                while ((!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return)))
                {
                    TestTexts[idx].text = full;
                    yield return null;
                }

            }
            else
            {
                while (!Input.GetKeyDown(scene.requiredKey))
                {
                    TestTexts[idx].text = full;
                    yield return null;
                }
            }
            if (TextPanel != null)
            {
                TextPanel.SetActive(false);
                StandingImage[0].gameObject.SetActive(false);
                StandingImage[1].gameObject.SetActive(false);
            }

            Time.timeScale = 1f;
            float startTime = Time.unscaledTime;
            float targetDuration = scene.goingTimeAmount;
            float elapsed = 0f;
            while (elapsed < targetDuration)
            {
                elapsed = Time.unscaledTime - startTime;
                yield return null;
            }
            Time.timeScale = 0f;
            TextPanel.SetActive(true);
            continue;
        }



    }
}