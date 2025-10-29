using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening; // DOTween 네임스페이스 추가

public class StageLevelSceneUI : MonoBehaviour
{
    private Image backGroundImage;
    private CanvasGroup canvasGroup; // CanvasGroup 참조 추가
    public TMP_Text tmpText;
    public TMP_Text extraText;
    public List<string> extraTextString;

    public bool isMoving = false;

    // Start 대신 Awake 사용 권장 (GetComponent는 Awake에서)
    private void Awake()
    {
        backGroundImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>(); // CanvasGroup 컴포넌트 가져오기

        // 컴포넌트가 없는 경우 에러 방지
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup 컴포넌트가 없습니다. GameObject에 추가해주세요.");
            canvasGroup = gameObject.AddComponent<CanvasGroup>(); // 임시로 추가
        }

        // 시작 시 투명하게 설정
        canvasGroup.alpha = 0f;
    }

    

    /// <summary>
    /// 스테이지 레벨 UI를 표시하는 코루틴 (1-based index)
    /// </summary>
    /// <param name="nowStageLevel">현재 스테이지 레벨 (1, 2, 3...)</param>
    public IEnumerator SetStageLevelSceneUI(int nowStageLevel)
    {
        canvasGroup.blocksRaycasts = true;
        isMoving = true;
        // 0. 텍스트 초기화
        tmpText.text = "";
        tmpText.transform.localScale = Vector3.zero; // 크기 초기화

        // 1. 도트윈으로 CanvasGroup 천천히 -> 밝기 조정 (Fade In)
        yield return canvasGroup.DOFade(1f, 0.7f).WaitForCompletion();


        // 2. tmpText 오브젝트가 도장처럼 박히는 애니메이션
        // 3. tmpText.text = "제{nowStageLevel}장" ; 으로 설정
        if (nowStageLevel == 2)
        {
            tmpText.text = $"제?장";
        }
        else
        {
            tmpText.text = $"제{nowStageLevel+1}장";
        }

        tmpText.transform.localScale = new Vector2(3f, 3f);
        yield return new WaitForSeconds(0.8f);
        // Ease.OutBack이 도장처럼 통통 튀는 느낌을 줍니다.
        yield return tmpText.transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack).WaitForCompletion();

        yield return new WaitForSeconds(0.4f);
        tmpText.rectTransform.DOAnchorPosX(-240f, 0.7f); // (O) 인스펙터의 Pos X 기준
        // 4. 잠시 기다림
        yield return new WaitForSeconds(1.3f);

        // 5. tmpText.text 가 기존 글자에 extraText[nowStageLevel] 에 써져있는 글자를 차례로 씀

        // nowStageLevel은 1부터 시작하는 레벨 값이므로, 리스트 인덱스로는 -1을 해줍니다.
        int textIndex = nowStageLevel;

        // 리스트 범위 체크


        extraText.text = extraTextString[textIndex];
       

        // 6. 잠시대기하고
        yield return new WaitForSeconds(1.5f);

        // 7. SetOffStageLevelSceneUI 실행
        StartCoroutine(SetOffStageLevelSceneUI());
    }

    IEnumerator SetOffStageLevelSceneUI()
    {
        // 1. CanvasGroup -> 밝기조정 (Fade Out)
        yield return canvasGroup.DOFade(0f, 0.5f).WaitForCompletion();

        // 2. 완전히 어두워진뒤 텍스트 clear
        tmpText.text = "";
        extraText.text = "";
        canvasGroup.blocksRaycasts = false;

        tmpText.rectTransform.anchoredPosition = new Vector2(0, 0);
        tmpText.transform.localScale = Vector3.zero; // 다음을 위해 크기도 리셋
        isMoving = false;
    }
}