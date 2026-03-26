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
        tmpText.rectTransform.DOAnchorPosX(-200f, 0.7f); // (O) �ν������� Pos X ����
        // 4. ��� ��ٸ�
        yield return new WaitForSeconds(1.3f);

        // 5. tmpText.text �� ���� ���ڿ� extraText[nowStageLevel] �� �����ִ� ���ڸ� ���ʷ� ��

        // nowStageLevel�� 1���� �����ϴ� ���� ���̹Ƿ�, ����Ʈ �ε����δ� -1�� ���ݴϴ�.
        int textIndex = nowStageLevel;

        // ����Ʈ ���� üũ


        extraText.text = stageSceneUI.localizationController.levelGuide_localizedString[textIndex].GetLocalizedString(); // 로컬라이제이션 적용
        // extraText.text = extraTextString[textIndex];
       

        // 6. ��ô���ϰ�
        yield return new WaitForSeconds(1.5f);

        // 7. SetOffStageLevelSceneUI ����
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