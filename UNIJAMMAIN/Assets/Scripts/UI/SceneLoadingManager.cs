using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : UI_Base
{
    // �̱��� �ν��Ͻ�
    public static SceneLoadingManager Instance { get; private set; }

    // �ε� �߿� ESC ������ ���� ��Ű�� ���� �ذ��ϱ� ���� ����
    public static bool IsLoading { get; private set; } = false;

    // 1. �� �غ� �Ϸ� ��ȣ�� ��ٸ��� ���� ���� �߰� ����
    private bool isSceneReadyToDisplay = false;

    [SerializeField] private Image leftPanel;
    [SerializeField] private Image rightPanel;

    [Header("Animation Settings")]
    [Tooltip("���� �������� ���� X ��ǥ (��: 1490)")]
    [SerializeField] private float openXPosition = 1490f;

    [Tooltip("���� ������ ���� X ��ǥ (��: 480)")]
    [SerializeField] private float closedXPosition = 480f;

    [Tooltip("���� �����̴� �� �ɸ��� �ð�")]
    [SerializeField] private float animationDuration = 0.8f;

    [Tooltip("���� ���� ���¿��� ����ϴ� �ð�")]
    [SerializeField] private float waitDuration = 0.5f;

    private AnimationCurve closingCurve;
    private AnimationCurve openingCurve;

    private RectTransform leftRect;
    private RectTransform rightRect;



    private float panelWidth;

    public override void Init()
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        // �̱��� ���� ����
      
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� ��ȯ�Ǿ �� ������Ʈ�� �ı����� ����
            InitializePanels();
        }
        else
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� �ִٸ� �� ������Ʈ�� �ı�
        }
   
    }

    public void LoadScene(string sceneName)
    {
        // ���� �̹� �ε� ���̶��, �� LoadScene�� �������� �ʰ� ��� �����մϴ�.
        if (IsLoading) return;

        // �ڷ�ƾ�� �����Ͽ� �񵿱� �� �ε��� ����
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    // �� �ε� �������� �����ϴ� ���� �ڷ�ƾ
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // �ε� ���� �� ��� true�� ����
        IsLoading = true;


        // ���� 2. �ڷ�ƾ ���� �� �غ� ���¸� false�� �ʱ�ȭ ����
        isSceneReadyToDisplay = false;


        // 1. �� �ݱ� �ִϸ��̼�
        Managers.Sound.Play("SFX/UI/StorySelect_V1", Define.Sound.SFX);
        leftPanel.gameObject.SetActive(true);
        rightPanel.gameObject.SetActive(true);
        yield return AnimatePanels(true); // true = �ݱ�


        Managers.Clear();
        // 2. ���� ���¿��� ��� ���
        yield return new WaitForSecondsRealtime(waitDuration);

        // 3. �� �񵿱� �ε� ���� (���� ���� ���¿��� ����)
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        // �� �ε��� 90% �Ϸ�� ������ ���
        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }

        // 4. �� Ȱ��ȭ (���� ���� ��������)
        // �� �������� ���� ���� Awake(), OnEnable() ���� ȣ���
        asyncOperation.allowSceneActivation = true;

        // ���� ������ Ȱ��ȭ�ǰ� ù �������� �׸� �ð��� �ֱ� ���� �� ������ ���
        yield return null;

        // ���� 3. ���⼭ �ٷ� ���� ���� �ʰ�, ��ȣ�� ���� ������ ���� ��� ����
        while (!isSceneReadyToDisplay)
        {
            yield return null;
        }

        Managers.Sound.Play("SFX/UI/Dialogue/Dialogue_V1", Define.Sound.SFX);

        // 5. ���ο� ���� �غ�Ǹ� �� ���� �ִϸ��̼� ����
        yield return AnimatePanels(false); // false = ����

        // ��� �� �г� ��Ȱ��ȭ (���� ����)
        yield return new WaitForSecondsRealtime(0.1f);
        leftPanel.gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);
        Managers.Sound.SettingNewSceneVolume();

        // ��� �ε� ������ ������ ������ false�� ����
        IsLoading = false;
        StopAllCoroutines();

    }

    public void NotifySceneReady()
    {
        isSceneReadyToDisplay = true;
    }

    // ���� �г��� �����̴� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator AnimatePanels(bool isClosing)
    {
        float timer = 0f;

        // ���� ��ġ�� ��ǥ ��ġ ���� (isClosing ���ο� ����)
        float startX = isClosing ? openXPosition : closedXPosition;
        float endX = isClosing ? closedXPosition : openXPosition;

        AnimationCurve curve = isClosing ? closingCurve : openingCurve;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / animationDuration);

            // AnimationCurve�� �����Ͽ� �ð� ������� ���������� ���� (ƨ��� ȿ�� ����)
            float curveProgress = curve.Evaluate(progress);

            // LerpUnclamped�� ����ؾ� Ŀ�� ���� 1�� �Ѿ ���������� ����
            float currentX = Mathf.LerpUnclamped(startX, endX, curveProgress);

            // ���� �г��� ����, ������ �г��� ��� ��ǥ�� ��Ī �̵�
            leftRect.anchoredPosition = new Vector2(-currentX, leftRect.anchoredPosition.y);
            rightRect.anchoredPosition = new Vector2(currentX, rightRect.anchoredPosition.y);

            yield return null;
        }

        // �ִϸ��̼��� ���� �� ��Ȯ�� ���� ��ġ�� ����
        leftRect.anchoredPosition = new Vector2(-endX, leftRect.anchoredPosition.y);
        rightRect.anchoredPosition = new Vector2(endX, rightRect.anchoredPosition.y);
    }


    // �г� �ʱ�ȭ �� ���� ��ġ ����
    private void InitializePanels()
    {
        leftRect = leftPanel.GetComponent<RectTransform>();
        rightRect = rightPanel.GetComponent<RectTransform>();

        // ���� �� ���� ���� ���·� ���� ����
        closingCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.0f),      // 0% �������� ���� (��: 0)
            new Keyframe(0.7f, 0.9f),     // 
            new Keyframe(0.8f, 0.95f),     //
            new Keyframe(0.9f, 0.975f),     //
            new Keyframe(1f, 1f)    //
           
        );
        openingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        leftRect.anchoredPosition = new Vector2(-openXPosition, leftRect.anchoredPosition.y);
        rightRect.anchoredPosition = new Vector2(openXPosition, rightRect.anchoredPosition.y);
    }

}