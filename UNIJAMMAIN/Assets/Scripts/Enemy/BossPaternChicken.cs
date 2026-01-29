using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossPaternChicken : UI_Popup
{
    /* 기획변경
    [Header("Arrow Prefab & Parent")]
    public RectTransform patternPanel; // PaternPanel의 RectTransform
    public GameObject arrowPrefab;
    public int arrowSize = 100; // 화살표 크기(100x100)
    private int arrowCount;

    [Header("Settings")]
    public Sprite[] arrowSprites;           // ↑ ↓ ← → 순서의 Sprite (0=Up,1=Down,2=Left,3=Right)
    public float timeLimit = 3f;             // 제한 시간 (초)

    private float timer;
    private bool isPlaying;
    private MiddleBoss middleBoss;

    private List<int> currentSequence = new List<int>(); // 화살표 방향 시퀀스
    private int currentIndex = 0;                        // 현재 맞춰야 할 인덱스

    
  
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
            EndPattern(false); // 시간 초과 → 실패
            return;
        }

        // 키보드 입력 체크
        if (Input.GetKeyDown(KeyCode.UpArrow)) CheckInput(0);
        if (Input.GetKeyDown(KeyCode.DownArrow)) CheckInput(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) CheckInput(2);
        if (Input.GetKeyDown(KeyCode.RightArrow)) CheckInput(3);
    }

    /// <summary>
    /// 보스 패턴 시작 (랜덤 개수 화살표 생성)
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
            arrowObj.transform.SetParent(patternPanel.transform, false); // false면 로컬 좌표/크기 유지

            arrowObj.GetComponent<Image>().sprite = arrowSprites[dir];
        }

        // 디버그용: 패턴 한글로 표시
        string[] dirNames = { "위", "아래", "왼쪽", "오른쪽" };
        List<string> patternNames = new List<string>();
        foreach (int dir in currentSequence)
        {
            patternNames.Add(dirNames[dir]);
        }
        Debug.Log("현재 패턴: " + string.Join(" -> ", patternNames));

        timer = timeLimit;
        isPlaying = true;

        yield return null;
    }


    /// <summary>
    /// 방향키 입력 체크
    /// </summary>
    void CheckInput(int dir)
    {
        if (!isPlaying) return;

        // 현재 입력해야 할 화살표의 방향과 일치하는지 확인
        if (dir == currentSequence[currentIndex])
        {
            // 첫 번째 화살표 UI 제거
            Destroy(patternPanel.GetChild(0).gameObject);
            currentIndex++;

            if (currentIndex >= currentSequence.Count)
            {
                EndPattern(true); // 성공
            }
        }
        else
        {
            InCorrectPattern(1);
            EndPattern(false); // 방향 틀림
        }
    }

    /// <summary>
    /// 패턴 종료
    /// </summary>
    void EndPattern(bool success)
    {
        isPlaying = false;
        Debug.Log(success ? "패턴 클리어!" : "패턴 실패!");

        ClearArrows();

        middleBoss.EndChickenPattern();
        if (this.gameObject != null)
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 화살표 UI 제거
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

    private void InCorrectPattern(int number) // number은 틀린개수
    {
        middleBoss.InCorrectChickenPattern(number);
    }
    */
}
