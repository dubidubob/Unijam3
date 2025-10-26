using UnityEngine;
using UnityEngine.SceneManagement;

public class AspectRatioEnforcer : MonoBehaviour
{
    public static AspectRatioEnforcer Instance;

    [Header("Aspect Ratio")]
    public float targetAspectWidth = 16.0f;
    public float targetAspectHeight = 9.0f;
    [Header("Size Limits")]
    public int minWindowWidth = 960;

    private float targetRatio;
    private int minWindowHeight;

    // --- [�ٽ� ����] ---
    // �� �������� "����ڰ� ���������� ����� *â���* ũ��"�� ����մϴ�.
    private int _lastWindowWidth;
    private int _lastWindowHeight;

    // �� ������ "����ڰ� *����* ���ϴ� ����(��üȭ�� or â���)"�� ����մϴ�.
    private bool _userWantsFullscreen = false;
    // --- [�ٽ� ���� ��] ---


    void Awake()
    {
        // 1. �̱��� (������ B�� ����) - �ذ�å 2 ����
        if (Instance != null)
        {
            this.enabled = false;
            Destroy(this); // ��ũ��Ʈ ������Ʈ�� �ı�
            return;
        }

        // 2. ����(A)�� ����
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        // 3. �ʱ� ���
        targetRatio = targetAspectWidth / targetAspectHeight;
        minWindowHeight = (int)Mathf.Round((float)minWindowWidth / targetRatio);

        // 4. [����] ���� ���� �� ���� ���¸� �����մϴ�.
        _userWantsFullscreen = Screen.fullScreen;
        if (_userWantsFullscreen)
        {
            // ��üȭ������ �����ߴٸ�, â��� ũ��� �ּҰ����� ���
            _lastWindowWidth = minWindowWidth;
            _lastWindowHeight = minWindowHeight;
        }
        else
        {
            // â���� �����ߴٸ�, ���� ũ�⸦ ����ϰ� ������ 1ȸ ����
            _lastWindowWidth = Screen.width;
            _lastWindowHeight = Screen.height;
            EnforceWindowedResolution();
        }
    }

    void Update()
    {
        if (Application.isEditor) return;

        // 1. [�ٽ�] ����ڰ� Alt+Enter ������ ���¸� "����"�ߴ��� ����
        if (Screen.fullScreen != _userWantsFullscreen)
        {
            _userWantsFullscreen = Screen.fullScreen; // �� ���¸� ��� ����

            if (!_userWantsFullscreen)
            {
                // ����ڰ� "��üȭ�� -> â���"�� *���* ��ȯ�߽��ϴ�.
                // �츮�� ����ص� ������ â ũ��� �������ݴϴ�.
                Screen.SetResolution(_lastWindowWidth, _lastWindowHeight, false);
            }
            // (����ڰ� "â��� -> ��üȭ��"���� ��ȯ�� ���, 
            // �츰 �ƹ��͵� �� �ʿ� ���� �׳� �θ� �˴ϴ�.)

            return; // ���� ������ ������ �����ӿ��� �ٸ� �۾��� ���� ����
        }

        // 2. ����ڰ� "��üȭ��" ���¸� ���Ѵٸ�, �ƹ��͵� ���� �ʰ� ���Ӵϴ�.
        if (_userWantsFullscreen)
        {
            return;
        }

        // 3. (���� �����ߴٸ� _userWantsFullscreen == false)
        // ����ڰ� "â���" ������ ��, â ũ�Ⱑ (���� ���� ������) ����Ǿ����� ����
        if (Screen.width != _lastWindowWidth || Screen.height != _lastWindowHeight)
        {
            // â ũ�Ⱑ �ٲ�����Ƿ�, ������ �ٽ� �����ϰ� �� ũ�⸦ �����մϴ�.
            EnforceWindowedResolution();
        }
    }

    /// <summary>
    /// [����] �� �Լ��� "���� â �ʺ�"�� �������� ������ �����ϰ�,
    /// �� ����� _lastWindow ������ �����մϴ�.
    /// </summary>
    private void EnforceWindowedResolution()
    {
        if (Screen.fullScreen) return; // â����� ���� ����

        int newWidth = Screen.width;

        // �ּ� ũ�� ����
        if (newWidth < minWindowWidth)
        {
            newWidth = minWindowWidth;
        }

        // �� �ʺ� �´� ���� ���
        int newHeight = (int)Mathf.Round((float)newWidth / targetRatio);

        // �ػ� ����
        Screen.SetResolution(newWidth, newHeight, false);

        // [�ٽ�] ��� ������ "���ο�" â ũ�⸦ ������ �����մϴ�.
        _lastWindowWidth = newWidth;
        _lastWindowHeight = newHeight;
    }


    /// <summary>
    /// [�ſ� �߿�] �� �ε� ���� ȣ��� "�ػ� ����" �Լ��Դϴ�.
    /// �� �ε� ���װ� �ػ󵵸� �������� ����, 
    /// �� �Լ��� �츮�� ����� ����(_userWantsFullscreen, _lastWindowWidth)�� ���� ������ŵ�ϴ�.
    /// </summary>
    public void RestoreDisplayState()
    {
        if (_userWantsFullscreen)
        {
            // ����ڴ� ��üȭ�� ���¿����Ƿ�, ��üȭ���� ������ �ٽ� ����
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        else
        {
            // ����ڴ� â��� ���¿����Ƿ�, ������ â ũ�⸦ ������ �ٽ� ����
            Screen.SetResolution(_lastWindowWidth, _lastWindowHeight, false);
        }
    }

    // OnDestroy�� ����Ӵϴ� (�̱��� ������ �ı����� ����)
    void OnDestroy() { }
}