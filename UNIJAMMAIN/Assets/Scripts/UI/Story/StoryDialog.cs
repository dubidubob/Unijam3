using DG.Tweening;
using KoreanTyper;                                                  // Add KoreanTyper namespace | ���� �����̽� �߰�
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryDialog : UI_Popup
{
    private bool inputRequested = false;
    private bool skipAllRequested = false;
    public string musicPath;
    public Sprite backGroundImage;
    public Image backGround;

    private static Sprite currentStoryBackground = null;

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
    private Vector2 originalLeftImagePos;
    private Vector2 originalRightImagePos;

    public List<DialogueScene> scenes;

    [Header("챕터 연출용")]
    public Image saturatedBackground; // [유지] 이 변수는 B층(Layer)으로 계속 사용
    public Ease backgroundEaseType = Ease.InCubic;

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
        LocalizationManager.Load();

        panelRect = TextPanel.GetComponent<RectTransform>();
        originalPanelPos = panelRect.anchoredPosition;
        contents.SetActive(true);

        originalLeftImagePos = StandingImage[0].GetComponent<RectTransform>().anchoredPosition;
        originalRightImagePos = StandingImage[1].GetComponent<RectTransform>().anchoredPosition;

        // --- ▼ 배경 설정 로직을 TypingCoroutine에서 여기로 이동 ▼ ---

        // 1. Awake 시점(OnEnable/첫 프레임 렌더링 전)에 static 배경 확인
        // (MainTitle에서 ResetStoryBackground()가 호출되었다고 가정)
        if (currentStoryBackground != null)
        {
            backGround.sprite = currentStoryBackground;
        }
        else
        {
            // 2. static이 null이면(첫 시작) 인스펙터의 기본 이미지 사용
            backGround.sprite = backGroundImage;
            currentStoryBackground = backGroundImage; // static에 저장
        }

        // 3. (중요) 첫 씬이 '즉시 페이드' 트리거를 가졌는지 확인
        // (scenes 리스트가 Awake 시점에 Inspector에서 할당되었다고 가정)
        if (scenes != null && scenes.Count > 0 && scenes[0].triggerBackgroundFade && scenes[0].preDelay == 0)
        {
            // 첫 씬이 즉시(preDelay=0) 페이드아웃/인 하는 씬이라면,
            // A층(backGround)을 완전 투명하게 시작해서 깜박임을 원천 차단합니다.
            backGround.color = new Color(1, 1, 1, 0);
        }
        else
        {
            // 일반적인 씬은 A층을 불투명하게 시작
            backGround.color = new Color(1, 1, 1, 1);
        }
        // --- ▲ 이동 끝 ▲ ---

        Managers.Sound.Play(musicPath, Define.Sound.BGM);
        StartCoroutine(FirstInAnimation());
    }

    private void Update()
    {
        // 스페이스바나 엔터키가 눌리면 플래그를 true로 설정
        // GetKeyDown은 한 프레임만 true이므로 Update에서 확인하는게 가장 정확합니다.
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            inputRequested = true;
        }

        // X키가 눌리면 '전체 스킵' 신호등을 켭니다.
        if (Input.GetKeyDown(KeyCode.X)) // <<< 이 if문을 추가하세요
        {
            skipAllRequested = true;
        }

    }

    private void OnEnable()
    {
        Time.timeScale = 0f;

        StartCoroutine(TypingCoroutine());
    }

    public IEnumerator TypingCoroutine()
    {
        panelRect.anchoredPosition = originalPanelPos;
        skipAllRequested = false; // 코루틴 시작 시 초기화

        for (int idx = 0; idx < scenes.Count; idx++)
        {
            // Update가 '전체 스킵' 신호를 켰는지 매 대사 시작 전에 확인합니다.
            if (skipAllRequested)
            {
                goto LoopEnd; // 즉시 대화 루프 탈출
            }

            if (idx > 0)
            {
                Managers.Sound.Play("SFX/UI/Dialogue/Dialogue_V3", Define.Sound.SFX, 1f, 2f);
            }

            DialogueScene scene = scenes[idx];

            yield return new WaitForSecondsRealtime(scene.preDelay);



            // ======================
            // 1. ĳ���� None ó��
            // ======================
            if (scene.speakingCharacterData == null)
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

                // ���� ĳ���� ó��
                if (scene.showLeftCharacter)
                {
                    TextPanel.GetComponentInChildren<TMP_Text>().text = scene.speakingCharacterData.CharacterName;
                    StandingImage[0].sprite = scene.overrideSprite != null ? scene.overrideSprite : scene.speakingCharacterData.CharacterImage;
                    StandingImage[0].gameObject.SetActive(true);

                    RectTransform imageRect = StandingImage[0].GetComponent<RectTransform>();
                    imageRect.anchoredPosition = new Vector2(originalLeftImagePos.x, originalLeftImagePos.y + scene.spriteYOffset);
                    //StandingImage[0].SetNativeSize(); // ����ũ��� ���� �ʰ� ���� ũ��� ����. ���� �ٲ۴ٸ� �̺κ� �ٲٵ���.

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
                    TextPanel.GetComponentInChildren<TMP_Text>().text = scene.speakingCharacterData.CharacterName;
                    StandingImage[1].sprite = scene.overrideSprite != null ? scene.overrideSprite : scene.speakingCharacterData.CharacterImage;
                    StandingImage[1].gameObject.SetActive(true);
                    RectTransform imageRect = StandingImage[1].GetComponent<RectTransform>();
                    imageRect.anchoredPosition = new Vector2(originalRightImagePos.x, originalRightImagePos.y + scene.spriteYOffset);
                    // --- [추가 끝] ---
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
            string full = LocalizationManager.Get(scene.localizationKey, scene.text);
            int len = full.GetTypingLength();

            inputRequested = false;

            for (int i = 0; i <= len; i++)
            {
                // Update가 켜놓은 신호등을 발견하면 즉시 스킵
                if (inputRequested || skipAllRequested)
                {
                    break;
                }
                TestTexts[idx].text = full.Typing(i);
                yield return new WaitForSecondsRealtime(0.02f);

            }

            // 타이핑이 끝나거나 스킵되면, 항상 전체 텍스트를 확실히 표시
            TestTexts[idx].text = full;

            // 2. 다음으로 넘어가기 위한 대기
            inputRequested = false; // 방금 사용한 스킵 입력을 초기화
            yield return null;      // 입력이 중복 처리되는 것을 막기 위해 한 프레임 대기

            // 새로운 입력이 들어올 때까지 계속 대기
            while (!inputRequested && !skipAllRequested)
            {
                yield return null;
            }

            // 만약 X키 때문에 루프를 탈출했다면, 즉시 전체 대화 루프를 끝냅니다.
            if (skipAllRequested)
            {
                goto LoopEnd;

            }

            // [배경 전환 연출]
            if (scene.triggerBackgroundFade && scene.newBackgroundSprite != null)
            {
                if (saturatedBackground != null)
                {
                    Debug.Log($"[{idx}] 배경 전환 트리거 확인됨. 새 배경: {scene.newBackgroundSprite.name}");

                    // --- [!!! 추가 확인 코드 !!!] ---
                    Debug.Log($"Fade 시작 전: A층(backGround) 알파 = {backGround.color.a}, B층(saturated) 알파 = {saturatedBackground.color.a}");
                    Debug.Log($"Fade 시작 전: B층 활성 상태 = {saturatedBackground.gameObject.activeInHierarchy}");
                    // --- [!!! 추가 끝 !!!] ---

                    if (currentStoryBackground != null && backGround.sprite != currentStoryBackground)
                    {
                        Debug.LogWarning($"Pre-Fade Check: Background sprite mismatch! " +
                                         $"Expected '{currentStoryBackground.name}', found '{backGround.sprite.name}'. Restoring.");
                        backGround.sprite = currentStoryBackground;
                        // 알파값도 페이드아웃 시작 전 상태(불투명=1)로 강제 복구
                        backGround.color = new Color(1, 1, 1, 1);
                    }
                    else if (currentStoryBackground == null) // 비상 상황 체크
                    {
                        Debug.LogError("Pre-Fade Check: currentStoryBackground is null!");
                        // static 변수가 null이면 어쩔 수 없이 기본 이미지라도 설정
                        backGround.sprite = backGroundImage;
                        backGround.color = new Color(1, 1, 1, 1);
                    }

                    // 1. B층 설정
                    saturatedBackground.sprite = scene.newBackgroundSprite;
                    saturatedBackground.color = new Color(1, 1, 1, 0); // 시작은 확실히 투명하게

                    // 2. B층 Fade In (트윈을 'fadeB' 변수에 저장)
                    Tween fadeB = saturatedBackground.DOFade(1f, 2.0f).SetUpdate(true).SetEase(backgroundEaseType).OnComplete(() => {
                        Debug.Log("B층 Fade In 완료! (알파값: " + saturatedBackground.color.a + ")");
                    });

                    //// 3. A층 Fade Out (완료 로그 추가)
                    //backGround.DOFade(0f, 2.0f).SetUpdate(true).SetEase(backgroundEaseType).OnComplete(() => {
                    //    Debug.Log("A층 Fade Out 완료! (알파값: " + backGround.color.a + ")");
                    //});

                    // 4. 대기 (2초 대기 대신, 'fadeB' 트윈이 끝날 때까지 대기)
                    yield return fadeB.WaitForCompletion();
                    Debug.Log("fadeB 트윈 완료. 레이어 교체 시도...");

                    // 5. 레이어 교체
                    backGround.sprite = scene.newBackgroundSprite;
                    currentStoryBackground = scene.newBackgroundSprite;
                    backGround.color = new Color(1, 1, 1, 1);
                    saturatedBackground.color = new Color(1, 1, 1, 0);

                    Debug.Log("배경 전환 완료!");
                }
                else
                {
                    Debug.LogError("SaturatedBackground 변수가 Inspector에 연결되지 않았습니다!");
                }
            }
            // --- [연출 끝] ---



            if (scene.leftSDAnim || scene.rightSDAnim)
            {
                //if (scene.requiredKey == KeyCode.None)
                //{
                //    while ((!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return)))
                //    {
                //        TestTexts[idx].text = full;
                //        yield return null;
                //    }

                //}
                //else
                //{
                //    while (!Input.GetKeyDown(scene.requiredKey))
                //    {
                //        TestTexts[idx].text = full;
                //        yield return null;
                //    }
                //}

                // �Է¹����� TextPanel ����
                if (idx < scenes.Count - 1)
                {
                    if (TextPanel != null)
                    {
                        TextPanel.SetActive(false);
                        StandingImage[0].gameObject.SetActive(false);
                        StandingImage[1].gameObject.SetActive(false);

                        // --- [!!! 이 줄을 추가하세요 !!!] ---
                        if (TestTexts[idx] != null) // 널 체크 추가
                            TestTexts[idx].gameObject.SetActive(false);
                        // --- [!!! 추가 끝 !!!] ---
                    }
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
                //bool panelTurnedOn = false;
                //while (!panelTurnedOn)
                //{

                //    // ���콺 �ƹ� ��ư Ŭ�� ��
                //    if (Input.GetMouseButtonDown(0))
                //    {


                //        if (KongCanvas != null) KongCanvas.SetActive(false);
                //        if (GretCanvas != null) GretCanvas.SetActive(false);
                //        if (scene.leftSDAnim)
                //        {
                //            foreach (var obj in leftSDAnimSet)
                //                if (obj != null) obj.SetActive(false);
                //            var leftAnim = leftSDCharacter.GetComponent<DOTweenAnimation>();
                //            var leftSR = leftSDCharacter.GetComponent<SpriteRenderer>();
                //            leftAnim.DOPlayBackwards();
                //            leftSR.sortingOrder = -3;
                //        }
                //        if (scene.rightSDAnim)
                //        {
                //            foreach (var obj in rightSDAnimSet)
                //                if (obj != null) obj.SetActive(false);
                //            var rightAnim = rightSDCharacter.GetComponent<DOTweenAnimation>();
                //            var rightSR = rightSDCharacter.GetComponent<SpriteRenderer>();
                //            rightAnim.DOPlayBackwards();
                //            rightSR.sortingOrder = -3;
                //        }


                //        if (TextPanel != null)
                //            TextPanel.SetActive(true);

                //        panelTurnedOn = true;
                //    }
                //    yield return null;
                //}

                // �ٷ� ���� ����!
                continue;
            }


            //if (scene.requiredKey == KeyCode.None)
            //{
            //    while ((!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return)))
            //    {
            //        if (Input.GetKeyDown(KeyCode.X))
            //        {
            //            goto LoopEnd;
            //        }


            //        TestTexts[idx].text = full;
            //        yield return null;
            //    }

            //}
            //else
            //{
            //    while (!Input.GetKeyDown(scene.requiredKey))
            //    {
            //        TestTexts[idx].text = full;
            //        yield return null;
            //    }
            //}
            if (idx < scenes.Count - 1)
            {
                if (TextPanel != null)
                {
                    TextPanel.SetActive(false);
                    StandingImage[0].gameObject.SetActive(false);
                    StandingImage[1].gameObject.SetActive(false);
                }

                // 시간 조절 관련 코드도 이 안으로 함께 옮기는 것이 안전합니다.
                Time.timeScale = 1f;
                float startTime = Time.unscaledTime;
                float targetDuration = scene.goingTimeAmount;
                float elapsed = 0f;
                while (elapsed < targetDuration)
                {
                    // --- [!!! 스킵 체크 추가 !!!] ---
                    // 딜레이 중에도 X 누르면 즉시 LoopEnd로 점프
                    if (skipAllRequested)
                    {
                        goto LoopEnd;
                    }
                    // --- [!!! 추가 끝 !!!] ---

                    elapsed = Time.unscaledTime - startTime;
                    yield return null;
                }
                Time.timeScale = 0f;
                TextPanel.SetActive(true);
            }

            continue;
        }


        LoopEnd:
        // --- ▼▼▼ 수정된 LoopEnd 로직 ▼▼▼ ---
        Sprite finalBackground = null;         // 최종 배경 저장용
        Coroutine backgroundFadeCoroutine = null; // 배경 페이드 코루틴 저장용

        // 1. 최종 배경 결정 (스킵 or 정상 종료)
        if (skipAllRequested) // X키 스킵
        {
            // 역순으로 마지막 배경 찾기 (기존과 동일)
            for (int i = scenes.Count - 1; i >= 0; i--)
            {
                if (scenes[i].triggerBackgroundFade && scenes[i].newBackgroundSprite != null)
                {
                    finalBackground = scenes[i].newBackgroundSprite;
                    break;
                }
            }
        }
        else // 스페이스바로 정상 종료
        {
            // 맨 마지막 씬에 배경 전환 트리거가 있었는지 확인
            if (scenes.Count > 0)
            {
                DialogueScene lastScene = scenes[scenes.Count - 1];
                if (lastScene.triggerBackgroundFade && lastScene.newBackgroundSprite != null)
                {
                    finalBackground = lastScene.newBackgroundSprite;
                }
            }
            CheckFirstClearSteamAchievement();
        }




        if (currentStoryBackground != null && backGround.sprite != currentStoryBackground)
        {
            Debug.LogWarning($"LoopEnd: Background sprite mismatch! " +
                             $"Expected '{currentStoryBackground.name}', found '{backGround.sprite.name}'. Restoring.");
            backGround.sprite = currentStoryBackground;
            // 알파값도 페이드 아웃 시작 전 상태(불투명)로 강제 설정
            backGround.color = new Color(1, 1, 1, 1);
        }

        // 2. UI 페이드 아웃 **시작** (아직 기다리지 않음)
        CanvasGroup canvasGroup = contents.GetComponent<CanvasGroup>();
        yield return new WaitForSecondsRealtime(0.4f); // 기존 딜레이
        Tween uiFadeTween = canvasGroup.DOFade(0f, 0.6f).SetUpdate(true);
        Debug.Log("UI Fade Out Started.");

        // 3. 배경 페이드 코루틴 **시작** (필요하다면) (아직 기다리지 않음)
        if (finalBackground != null && backGround.sprite != finalBackground)
        {
            Debug.Log("Starting Background Fade Coroutine: " + finalBackground.name);
            backgroundFadeCoroutine = StartCoroutine(FadeBackgroundAfterSkip(finalBackground));
        }
        else
        {
            Debug.Log("No background fade needed or already on correct background.");
        }

        // 4. UI 페이드 아웃이 **끝날 때까지 대기**
        yield return uiFadeTween.WaitForCompletion();
        Debug.Log("UI Fade Out Completed.");

        // 5. 배경 페이드 코루틴이 **끝날 때까지 대기** (시작되었다면)
        if (backgroundFadeCoroutine != null)
        {
            Debug.Log("Waiting for Background Fade Coroutine to complete...");
            yield return backgroundFadeCoroutine;
            Debug.Log("Background Fade Coroutine Completed.");
        }

        // 6. 모든 것이 끝나면 씬 이동
        Debug.Log("All fades completed. Moving scene.");
        SceneMoving();
        // --- ▲▲▲ 수정 끝 ▲▲▲ ---

        //StartCoroutine(LastOutAnimation());

    }


    private void SceneMoving()
    {
        SceneLoadingManager.Instance.LoadScene("GamePlayScene");
        Managers.Sound.BGMFadeOut();
    }

    #region Enimation

    IEnumerator FirstInAnimation()
    {
        CanvasGroup canvasGroup;
        canvasGroup = contents.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        yield return new WaitForSecondsRealtime(0.4f);
        // 1�ʿ� ���� alpha 1�� ����
        canvasGroup.DOFade(1f, 0.6f).SetUpdate(true);
    }

    IEnumerator LastOutAnimation()
    {
        CanvasGroup canvasGroup;
        canvasGroup = contents.GetComponent<CanvasGroup>();
        //canvasGroup.alpha = 1;
        yield return new WaitForSecondsRealtime(0.4f);
        // 1�ʿ� ���� alpha 1�� ����
        canvasGroup.DOFade(0f, 0.6f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.6f);
        //currentStoryBackground = null;
        SceneMoving();
        
    }
    #endregion

    IEnumerator FadeBackgroundAfterSkip(Sprite finalBackground)
    {
        // B층(saturatedBackground)이 연결되어 있고 현재 배경과 다를 때만 실행
        if (saturatedBackground != null && backGround.sprite != finalBackground)
        {
            Debug.Log("스킵 페이드 코루틴 시작: " + finalBackground.name);

            // 1. B층에 최종 배경 설정하고 투명하게
            saturatedBackground.sprite = finalBackground;
            saturatedBackground.color = new Color(1, 1, 1, 0);

            if (currentStoryBackground != null && backGround.sprite != currentStoryBackground)
            {
                Debug.LogWarning($"FadeBackgroundAfterSkip: Background sprite mismatch just before fade! " +
                                 $"Expected '{currentStoryBackground.name}', found '{backGround.sprite.name}'. Restoring.");
                backGround.sprite = currentStoryBackground;
                // 알파값도 혹시 모르니 불투명(1)으로 재설정
                backGround.color = new Color(1, 1, 1, 1);
            }
            else if (currentStoryBackground == null)
            {
                Debug.LogError("FadeBackgroundAfterSkip: currentStoryBackground is null!");
                // 비상: currentStoryBackground가 null이면 기본 이미지라도 설정 (오류 상황)
                backGround.sprite = backGroundImage;
                backGround.color = new Color(1, 1, 1, 1);
            }

            // 2. B층 Fade In & A층 Fade Out (동시 시작)
            //    DOTween 트윈 자체를 변수에 저장
            Tween fadeB = saturatedBackground.DOFade(1f, 2.5f).SetUpdate(true).SetEase(backgroundEaseType);
            //Tween fadeA = backGround.DOFade(0f, 2.5f).SetUpdate(true).SetEase(backgroundEaseType);

            // 3. 두 페이드가 모두 완료될 때까지 기다림 (WaitForSecondsRealtime 대신 사용)
            yield return fadeB.WaitForCompletion();
            // yield return fadeA.WaitForCompletion(); // 둘 중 하나만 기다려도 충분

            Debug.Log("스킵 페이드 코루틴: 페이드 완료.");

            // 4. 레이어 교체
            backGround.sprite = finalBackground;
            currentStoryBackground = finalBackground;
            backGround.color = new Color(1, 1, 1, 1);
            saturatedBackground.color = new Color(1, 1, 1, 0); // B층 다시 숨김
            Debug.Log("스킵 페이드 코루틴: 레이어 교체 완료.");
        }
        else // B층이 없거나 배경이 같으면 즉시 변경 (기존 로직 유지)
        {
            if (finalBackground != null && backGround.sprite != finalBackground)
            {
                backGround.sprite = finalBackground;
                backGround.color = new Color(1, 1, 1, 1);
            }
            yield return null; // 코루틴은 최소 한 번의 yield가 필요
        }
    }

    public static void ResetStoryBackground()
    {
        currentStoryBackground = null;
    }
    // --- [!!! 새 코루틴 함수 끝 !!!] ---


    // Steam 업적 : 챕터 3의 스토리를 스페이스바를 통해서 읽었을때
    private void CheckFirstClearSteamAchievement()
    {
        if(IngameData.ChapterIdx==3||IngameData.ChapterIdx==4)
        {
            Managers.Steam.UnlockAchievement($"ACH_STORY_{IngameData.ChapterIdx}_CLEAR");
        }


    }
    #region Tool

    #endregion
}