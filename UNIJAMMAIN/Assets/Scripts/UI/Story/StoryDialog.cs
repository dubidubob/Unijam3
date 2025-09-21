using DG.Tweening;
using KoreanTyper;                                                  // Add KoreanTyper namespace | 네임 스페이스 추가
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryDialog : UI_Popup
{
    public string musicPath;
    public Sprite backGroundImage;
    public Image backGround;

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

        Managers.Sound.Play(musicPath, Define.Sound.BGM);
        StartCoroutine(FirstInAnimation());
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        backGround.sprite = backGroundImage;
        StartCoroutine(TypingCoroutine());
    }

    public IEnumerator TypingCoroutine()
    {
        panelRect.anchoredPosition = originalPanelPos;

        for (int idx = 0; idx < scenes.Count; idx++)
        {
            Managers.Sound.Play("SFX/UI/Dialogue/Dialogue_V1");

            DialogueScene scene = scenes[idx];

            yield return new WaitForSecondsRealtime(scene.preDelay);

            

            // ======================
            // 1. 캐릭터 None 처리
            // ======================
            if (scene.speakingCharacterData == null)
            {
                // 캐릭터 이미지 모두 OFF
                StandingImage[0].gameObject.SetActive(false);
                StandingImage[1].gameObject.SetActive(false);
                // 패널 원래 위치 (panelPositionOffset 무시)
                if (panelRect != null)
                    panelRect.anchoredPosition = originalPanelPos;
            }
            else
            {
                // ======================
                // 2. 일반 캐릭터 처리
                // ======================
                // 캐릭터 인덱스 None 제외
              
                // 왼쪽 캐릭터 처리
                if (scene.showLeftCharacter)
                {
                    TextPanel.GetComponentInChildren<TMP_Text>().text = scene.speakingCharacterData.name;
                    StandingImage[0].sprite = scene.overrideSprite != null ? scene.overrideSprite : scene.speakingCharacterData.CharacterImage;
                    StandingImage[0].gameObject.SetActive(true);
                    //StandingImage[0].SetNativeSize(); // 원본크기로 하지 않고 원래 크기로 설정. 만약 바꾼다면 이부분 바꾸도록.

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
                        Debug.Log("왼쪽사람화내기!");
                        LeftCharacter.instance.frowningAnim.DORestartById("1");
                    }
                    if (scene.isSurprized)
                    {
                        Debug.Log("왼쪽사람놀라기!");
                        LeftCharacter.instance.frowningAnim.DORestartById("2");
                    }
                }

                else
                {
                    StandingImage[0].gameObject.SetActive(false);
                }

                // 오른쪽 캐릭터 처리
                if (scene.showRightCharacter)
                {
                    TextPanel.GetComponentInChildren<TMP_Text>().text = scene.speakingCharacterData.name;
                    StandingImage[1].sprite = scene.overrideSprite != null ? scene.overrideSprite : scene.speakingCharacterData.CharacterImage;
                    StandingImage[1].gameObject.SetActive(true);
                    //StandingImage[1].SetNativeSize();
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

                // 패널 위치 (원래 위치 + 오프셋)
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
                            Debug.Log("오른쪽사람화내기!");
                            RightCharacter.instance.frowningAnim.DORestartById("1");
                        }
                        if (scene.isSurprized)
                        {
                            Debug.Log("오른쪽사람놀라기!");
                            RightCharacter.instance.surprisedAnim.DORestartById("2");
                        }
                    }
                }
            }

            // 텍스트 표시 (타이핑 효과)
            TestTexts[idx].gameObject.SetActive(true);
            string full = scene.text;
            int len = full.GetTypingLength();

            for (int i = 0; i <= len; i++)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Space & skip");
                    TestTexts[idx].text = full; // 텍스트를 즉시 전체 문장으로 설정
                    break;                      // 타이핑 루프 탈출
                }

                TestTexts[idx].text = full.Typing(i);
                yield return new WaitForSecondsRealtime(0.02f);
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

                // 입력받으면 TextPanel 끄기
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
                
                    // 마우스 아무 버튼 클릭 시
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

                // 바로 다음 대사로!
                continue;
            }


            if (scene.requiredKey == KeyCode.None)
            {
                while ((!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return)))
                {
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        goto LoopEnd;
                    }


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

        LoopEnd:
        // DiaLogue끝
        StartCoroutine(LastOutAnimation());


    }


    private void SceneMoving()
    {
        Managers.Scene.LoadScene(Define.Scene.GamePlayScene);
    }

    #region Enimation

    IEnumerator FirstInAnimation()
    {
        CanvasGroup canvasGroup;
        canvasGroup = contents.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        yield return new WaitForSecondsRealtime(0.4f);
        // 1초에 걸쳐 alpha 1로 변경
        canvasGroup.DOFade(1f, 0.6f).SetUpdate(true);
    }

    IEnumerator LastOutAnimation()
    {
        CanvasGroup canvasGroup;
        canvasGroup = contents.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        yield return new WaitForSecondsRealtime(0.4f);
        // 1초에 걸쳐 alpha 1로 변경
        canvasGroup.DOFade(0f, 0.6f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.6f);
        SceneMoving();
        
    }
    #endregion

    #region Tool

    #endregion
}