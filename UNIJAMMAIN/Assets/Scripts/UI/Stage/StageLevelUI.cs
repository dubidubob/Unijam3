using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class StageLevelSceneUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public TMP_Text tmpText;
    public TMP_Text extraText;
    public List<string> extraTextString;
    public bool isMoving = false;
    [SerializeField] private StageSceneUI stageSceneUI;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup이 없어서 자동 추가합니다.");
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
        isMoving = false;
    }

    public IEnumerator SetStageLevelSceneUI(int nowStageLevel)
    {
        if (stageSceneUI.isEventMap) { yield break; }

        if (nowStageLevel + 1 > IngameData.StageProgress)
        {
            IngameData.StageProgress++;
        }
        else
        {
            yield break;

        }

        stageSceneUI.isAnimating = true;
        // 🌟 [핵심 방어 코드] 진행 중인 DOTween 애니메이션이 있다면 강제 종료 (꼬임 및 영구 투명화 방지)
        canvasGroup.DOKill();
        tmpText.transform.DOKill();
        tmpText.rectTransform.DOKill();
        extraText.DOKill();

        canvasGroup.blocksRaycasts = true;
        isMoving = true;
        

        tmpText.text = "";
        tmpText.transform.localScale = Vector3.zero;

        yield return canvasGroup.DOFade(1f, 1.2f).WaitForCompletion();

        yield return new WaitForSeconds(0.5f);
        stageSceneUI.localizationController.RefreshLevelInfoUI(tmpText, stageSceneUI.currentPageLevel, stageSceneUI.isEventMap);

        tmpText.transform.localScale = new Vector2(3f, 3f);
        Managers.Sound.Play("SFX/UI/Act123_V1", Define.Sound.SFX, 1f, 2f);

        yield return new WaitForSeconds(0.7f);
        yield return tmpText.transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack).WaitForCompletion();
        yield return new WaitForSeconds(0.4f);

        int textIndex = nowStageLevel;
        string nextExtraString = stageSceneUI.localizationController.levelGuide_localizedString[textIndex].GetLocalizedString();
        extraText.text = nextExtraString.Trim();
        extraText.color = new Color(extraText.color.r, extraText.color.g, extraText.color.b, 0f);

        tmpText.ForceMeshUpdate(true);
        extraText.ForceMeshUpdate(true);

        float tmpWidth = tmpText.preferredWidth * tmpText.transform.localScale.x;
        float extraWidth = extraText.preferredWidth * extraText.transform.localScale.x;
        float spacing = 60f;

        float totalGroupWidth = tmpWidth + spacing + extraWidth;
        float tmpTextTargetX = (-totalGroupWidth / 2f) + (tmpWidth / 2f);
        float extraTextTargetX = (totalGroupWidth / 2f) - (extraWidth / 2f);

        extraText.rectTransform.anchoredPosition = new Vector2(extraTextTargetX, extraText.rectTransform.anchoredPosition.y);
        tmpText.rectTransform.DOAnchorPosX(tmpTextTargetX, 0.7f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(1.3f);
        extraText.DOFade(1f, 0.5f);
        yield return new WaitForSeconds(1.5f);

        StartCoroutine(SetOffStageLevelSceneUI());
    }

    IEnumerator SetOffStageLevelSceneUI()
    {
        yield return canvasGroup.DOFade(0f, 1.8f).WaitForCompletion();
        

        tmpText.text = "";
        extraText.text = "";
        canvasGroup.blocksRaycasts = false;
        tmpText.rectTransform.anchoredPosition = new Vector2(0, 0);
        tmpText.transform.localScale = Vector3.zero;
        isMoving = false;
        stageSceneUI.isAnimating = false;

    }
}