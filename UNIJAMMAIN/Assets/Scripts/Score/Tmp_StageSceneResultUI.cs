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
        // IngameData에 만들어두신 GetRankForChapter 함수를 사용합니다.
        Define.Rank rank = IngameData.GetRankForChapter(stageIndex);

        // 2. 랭크 상태에 따라 스프라이트를 결정합니다.

        // 만약 랭크가 'Unknown' 이라면 (아직 플레이하지 않아 랭크가 없는 상태)
        // 'None'에 해당하는 스프라이트를 표시합니다.
        // 'None' 스프라이트는 배열의 마지막(인덱스 5)에 있다고 가정합니다.
        if (rank == Define.Rank.Unknown)
        {
            // sprites.Length - 1 은 배열의 마지막 인덱스를 가리킵니다.
            sp.sprite = sprites[sprites.Length - 1];
        }
        else // 랭크가 있다면 (Bad, Good, Perfect 등)
        {
            // 랭크 enum을 정수(int)로 변환하여 배열의 인덱스로 사용합니다.
            // (예: Bad = 0, Normal = 1, ...)
            int rankIndex = (int)rank;

            // 혹시 모를 오류를 방지하기 위해 인덱스가 배열 범위 내에 있는지 확인
            if (rankIndex >= 0 && rankIndex < sprites.Length)
            {
                sp.sprite = sprites[rankIndex];
            }
        }

        // 3. 이미지 컴포넌트를 활성화하고, 크기를 원본 스프라이트에 맞춥니다.
        sp.enabled = true;
        sp.SetNativeSize();
    }
}