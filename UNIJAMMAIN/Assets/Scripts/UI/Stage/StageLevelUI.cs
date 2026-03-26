using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening; // DOTween ���ӽ����̽� �߰�

public class StageLevelSceneUI : MonoBehaviour
{
    private CanvasGroup canvasGroup; // CanvasGroup ���� �߰�
    public TMP_Text tmpText;
    public TMP_Text extraText;
    public List<string> extraTextString;

    public bool isMoving = false;

    [SerializeField] private StageSceneUI stageSceneUI;

    // Start ��� Awake ��� ���� (GetComponent�� Awake����)
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>(); // CanvasGroup ������Ʈ ��������

        // ������Ʈ�� ���� ��� ���� ����
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup ������Ʈ�� �����ϴ�. GameObject�� �߰����ּ���.");
            canvasGroup = gameObject.AddComponent<CanvasGroup>(); // �ӽ÷� �߰�
        }

        // ���� �� �����ϰ� ����
        canvasGroup.alpha = 0f;
    }

    

    /// <summary>
    /// �������� ���� UI�� ǥ���ϴ� �ڷ�ƾ (1-based index)
    /// </summary>
    /// <param name="nowStageLevel">���� �������� ���� (1, 2, 3...)</param>
    public IEnumerator SetStageLevelSceneUI(int nowStageLevel)
    {
        
        if(stageSceneUI.isEventMap) // 이벤트 맵이라면 작동하지않는다
        {
            yield break;
        

        }


        // 해당 장에 처음 들어왔을때만 실행하기.
  
        if(nowStageLevel+1>IngameData.StageProgress) //해당스테이지에 처음 들어온 순간!
        {
            IngameData.StageProgress++;
        }
        else
        {
            // 해당스테이지에 들어온적이 있었다.
            yield break;
        }


        canvasGroup.blocksRaycasts = true;
        isMoving = true;
        stageSceneUI.isAnimating = true;
        // 0. �ؽ�Ʈ �ʱ�ȭ
        tmpText.text = "";
        tmpText.transform.localScale = Vector3.zero; // ũ�� �ʱ�ȭ

        // 1. ��Ʈ������ CanvasGroup õõ�� -> ��� ���� (Fade In)
        yield return canvasGroup.DOFade(1f, 1.2f).WaitForCompletion();


        // 2. tmpText ������Ʈ�� ����ó�� ������ �ִϸ��̼�
        // 3. tmpText.text = "��{nowStageLevel}��" ; ���� ����
        yield return new WaitForSeconds(0.5f);
        stageSceneUI.localizationController.RefreshLevelInfoUI(tmpText, stageSceneUI.currentPageLevel, stageSceneUI.isEventMap); // 로컬라이제이션 
        /*
        if (nowStageLevel == 2)
        {
            tmpText.text = $"제?장";
        }
        else
        {
            tmpText.text = $"제{nowStageLevel+1}장";
        }
        */

        tmpText.transform.localScale = new Vector2(3f, 3f);
        Managers.Sound.Play("SFX/UI/Act123_V1", Define.Sound.SFX, 1f, 2f);
        yield return new WaitForSeconds(0.7f);
        // Ease.OutBack�� ����ó�� ���� Ƣ�� ������ �ݴϴ�.
        yield return tmpText.transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack).WaitForCompletion();

        yield return new WaitForSeconds(0.4f);


        // ------------------------------------------------------------------------
        // [동적 그룹 중앙 정렬 계산] 

        // 1. 나타날 텍스트 세팅
        int textIndex = nowStageLevel;
        string nextExtraString = stageSceneUI.localizationController.levelGuide_localizedString[textIndex].GetLocalizedString();
        // .Trim()을 붙여서 번역 데이터 앞뒤의 쓸데없는 띄어쓰기(공백)를 모두 날려버립니다!
        extraText.text = nextExtraString.Trim();
        extraText.text = stageSceneUI.localizationController.levelGuide_localizedString[textIndex].GetLocalizedString();
        extraText.color = new Color(extraText.color.r, extraText.color.g, extraText.color.b, 0f);

        // 2. TMP 메시 즉시 업데이트 강제
        tmpText.ForceMeshUpdate(true);
        extraText.ForceMeshUpdate(true);

        // 3. ✨핵심✨ rect.width가 아닌 preferredWidth 사용! (딜레이 없이 즉시 실제 텍스트 길이 가져옴)
        float tmpWidth = tmpText.preferredWidth * tmpText.transform.localScale.x;
        float extraWidth = extraText.preferredWidth * extraText.transform.localScale.x;
        float spacing = 60f; // 두 텍스트 사이를 벌려줄 간격 (원하는 만큼 늘리세요!)

        // 4. 두 텍스트 + 간격을 합친 '가상 그룹'의 전체 길이 계산
        float totalGroupWidth = tmpWidth + spacing + extraWidth;

        // 5. 정중앙(X=0)을 기준으로 양옆으로 쫙 벌어질 타겟 좌표 계산
        float tmpTextTargetX = (-totalGroupWidth / 2f) + (tmpWidth / 2f);
        float extraTextTargetX = (totalGroupWidth / 2f) - (extraWidth / 2f);

        // 6. 오른쪽 텍스트(ExtraText)를 벌어진 위치(extraTextTargetX)에 세팅
        extraText.rectTransform.anchoredPosition = new Vector2(extraTextTargetX, extraText.rectTransform.anchoredPosition.y);

        // 7. 왼쪽 텍스트(Text)는 벌어질 위치(tmpTextTargetX)로 스르륵 이동!
        tmpText.rectTransform.DOAnchorPosX(tmpTextTargetX, 0.7f).SetEase(Ease.OutCubic);
        // ------------------------------------------------------------------------

        // 4. 텍스트가 이동할 때까지 잠깐 기다림
        yield return new WaitForSeconds(1.3f);

        // 5. 드디어 ExtraText를 스르륵 나타나게 합니다. (Fade In)
        extraText.DOFade(1f, 0.5f);

        // 6. 연출 감상 시간
        yield return new WaitForSeconds(1.5f);

        // 7. 종료 코루틴 실행
        StartCoroutine(SetOffStageLevelSceneUI());
    }

    IEnumerator SetOffStageLevelSceneUI()
    {
        // 1. CanvasGroup -> ������� (Fade Out)
        yield return canvasGroup.DOFade(0f, 1.8f).WaitForCompletion();

        // 2. ������ ��ο����� �ؽ�Ʈ clear
        tmpText.text = "";
        extraText.text = "";
        canvasGroup.blocksRaycasts = false;

        tmpText.rectTransform.anchoredPosition = new Vector2(0, 0);
        tmpText.transform.localScale = Vector3.zero; // ������ ���� ũ�⵵ ����
        isMoving = false;
        stageSceneUI.isAnimating = false;
    }
}