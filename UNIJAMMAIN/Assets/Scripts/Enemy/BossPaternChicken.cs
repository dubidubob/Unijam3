using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossPaternChicken : UI_Popup
{
    [Header("Arrow Prefab & Parent")]
    public RectTransform patternPanel; // PaternPanel�� RectTransform
    public GameObject arrowPrefab;
    public int arrowSize = 100; // ȭ��ǥ ũ��(100x100)
    private int arrowCount;

    [Header("Settings")]
    public Sprite[] arrowSprites;           // �� �� �� �� ������ Sprite (0=Up,1=Down,2=Left,3=Right)
    public float timeLimit = 3f;             // ���� �ð� (��)

    private float timer;
    private bool isPlaying;
    private MiddleBoss middleBoss;

    private List<int> currentSequence = new List<int>(); // ȭ��ǥ ���� ������
    private int currentIndex = 0;                        // ���� ����� �� �ε���

    
  
    private void Start()
    {
        StartPattern();
    }
    void Update()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            EndPattern(false); // �ð� �ʰ� �� ����
            return;
        }

        // Ű���� �Է� üũ
        if (Input.GetKeyDown(KeyCode.UpArrow)) CheckInput(0);
        if (Input.GetKeyDown(KeyCode.DownArrow)) CheckInput(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) CheckInput(2);
        if (Input.GetKeyDown(KeyCode.RightArrow)) CheckInput(3);
    }

    /// <summary>
    /// ���� ���� ���� (���� ���� ȭ��ǥ ����)
    /// </summary>
    public IEnumerator StartPattern()
    {
        ClearArrows();
        currentSequence.Clear();
        currentIndex = 0;
        patternPanel.sizeDelta = new Vector2(arrowCount * arrowSize, patternPanel.sizeDelta.y);
        for (int i = 0; i < arrowCount; i++)
        {
            int dir = Random.Range(0, 4); // 0=Up, 1=Down, 2=Left, 3=Right
            currentSequence.Add(dir);

            GameObject arrowObj = Instantiate(arrowPrefab, patternPanel);
            arrowObj.transform.SetParent(patternPanel.transform, false); // false�� ���� ��ǥ/ũ�� ����

            arrowObj.GetComponent<Image>().sprite = arrowSprites[dir];
        }

        // ����׿�: ���� �ѱ۷� ǥ��
        string[] dirNames = { "��", "�Ʒ�", "����", "������" };
        List<string> patternNames = new List<string>();
        foreach (int dir in currentSequence)
        {
            patternNames.Add(dirNames[dir]);
        }
        Debug.Log("���� ����: " + string.Join(" -> ", patternNames));

        timer = timeLimit;
        isPlaying = true;

        yield return null;
    }


    /// <summary>
    /// ����Ű �Է� üũ
    /// </summary>
    void CheckInput(int dir)
    {
        if (!isPlaying) return;

        // ���� �Է��ؾ� �� ȭ��ǥ�� ����� ��ġ�ϴ��� Ȯ��
        if (dir == currentSequence[currentIndex])
        {
            // ù ��° ȭ��ǥ UI ����
            Destroy(patternPanel.GetChild(0).gameObject);
            currentIndex++;

            if (currentIndex >= currentSequence.Count)
            {
                EndPattern(true); // ����
            }
        }
        else
        {
            InCorrectPattern(1);
            EndPattern(false); // ���� Ʋ��
        }
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    void EndPattern(bool success)
    {
        isPlaying = false;
        Debug.Log(success ? "���� Ŭ����!" : "���� ����!");

        ClearArrows();

        middleBoss.EndChickenPattern();
        if (this.gameObject != null)
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// ȭ��ǥ UI ����
    /// </summary>
    void ClearArrows()
    {
        foreach (Transform child in patternPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetTimer(float time)
    {
        timeLimit = time;
    }

    public void SetArrowCount(int count)
    {
        arrowCount = count;
    }

    public void SetMiddleBoss(MiddleBoss boss)
    {
        middleBoss = boss;
    }

    private void InCorrectPattern(int number) // number�� Ʋ������
    {
        middleBoss.InCorrectChickenPattern(number);
    }
}
