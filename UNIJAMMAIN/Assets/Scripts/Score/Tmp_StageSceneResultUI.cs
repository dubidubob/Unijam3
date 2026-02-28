using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 마우스 이벤트 감지를 위해 필요
using TMPro; // TextMeshPro 사용
using DG.Tweening;

public class Tmp_StageSceneResultUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("B, n, ng, g, p,None 순")]
    [SerializeField] Sprite[] sprites;
    private Image sp;

    [SerializeField] private CanvasGroup scoreCanvasGroup; // 페이드 효과를 위해 필요
    [SerializeField] private TextMeshProUGUI scoreText;   // 점수 텍스트
    [SerializeField] private RectTransform scoreRect;      // 위치 이동을 위해 필요

    private Vector2 originPos; // 원래 위치 저장용

    private void Start()
    {
        sp = GetComponent<Image>();
        sp.enabled = false;
        // 초기 세팅: 투명도를 0으로 만들고 원래 위치 저장
        if (scoreCanvasGroup != null) scoreCanvasGroup.alpha = 0;
        originPos = scoreRect.anchoredPosition;
    }

    /// <summary>
    /// 스테이지 버튼을 클릭했을 때 호출되는 함수입니다.
    /// 선택된 스테이지의 랭크에 맞는 이미지를 표시합니다.
    /// </summary>
    /// <param name="stageIndex">선택된 스테이지의 인덱스</param>
    public void LoadClickedStageData(int stageIndex)
    {
        // 1. 전달받은 stageIndex를 사용해 해당 챕터의 랭크를 가져옵니다.
        Define.Rank rank = IngameData.GetBestRankForChapter(stageIndex);

        // 2. 랭크 상태에 따라 스프라이트를 결정합니다.
        int rankIndex = (int)rank;

        if (rankIndex >= 0 && rankIndex < sprites.Length)
        {
            sp.sprite = sprites[rankIndex];
        }

        // 3. 이미지 컴포넌트를 활성화하고, 크기를 원본 스프라이트에 맞춥니다.
        sp.enabled = true;
        sp.SetNativeSize();
    }


    // 1. 마우스를 올렸을 때 (Hover Enter)
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 기존에 실행 중인 애니메이션이 있다면 중지 (꼬임 방지)
        scoreRect.DOKill();
        scoreCanvasGroup.DOKill();

        // [애니메이션 설정]
        // 시작 위치를 약간 아래로 설정하고 원래 위치로 복귀하며 Fade In
        scoreRect.anchoredPosition = originPos + new Vector2(0, -20f);

        // 위로 슥 올라오는 연출 (0.4초 동안)
        scoreRect.DOAnchorPos(originPos, 0.4f).SetEase(Ease.OutBack);

        scoreText.text = "Score : ";
        scoreText.text += IngameData.GetBestRankScoreForChapter(IngameData.ChapterIdx).ToString();
        // 나타나는 연출 (0.3초 동안)
        scoreCanvasGroup.DOFade(1f, 0.3f);
    }

    // 2. 마우스가 나갔을 때 (Hover Exit)
    public void OnPointerExit(PointerEventData eventData)
    {
        scoreRect.DOKill();
        scoreCanvasGroup.DOKill();

        // 다시 사라지는 연출
        scoreCanvasGroup.DOFade(0f, 0.2f);
    }

}