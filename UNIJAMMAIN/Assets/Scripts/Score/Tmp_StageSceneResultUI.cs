using UnityEngine;
using UnityEngine.UI;

public class Tmp_StageSceneResultUI : MonoBehaviour
{
    [Header("B, n, ng, g, p,None 순")]
    [SerializeField] Sprite[] sprites;
    private Image sp;

    private void Start()
    {
        sp = GetComponent<Image>();
        int rank = (int)IngameData.GetRankForChapter(0);
        if (rank >= sprites.Length)
        {
            sp.sprite = null;
            sp.enabled = false;
        }
        else
        {
            sp.sprite = sprites[rank];
            sp.enabled = true;
        }        

        sp.SetNativeSize();
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
}