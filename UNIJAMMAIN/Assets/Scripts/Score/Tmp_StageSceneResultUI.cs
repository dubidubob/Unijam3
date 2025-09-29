using UnityEngine;
using UnityEngine.UI;

public class Tmp_StageSceneResultUI : MonoBehaviour
{
    [Header("B, n, ng, g, p,None ��")]
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
    /// �������� ��ư�� Ŭ������ �� ȣ��Ǵ� �Լ��Դϴ�.
    /// ���õ� ���������� ��ũ�� �´� �̹����� ǥ���մϴ�.
    /// </summary>
    /// <param name="stageIndex">���õ� ���������� �ε���</param>
    public void LoadClickedStageData(int stageIndex)
    {
        // 1. ���޹��� stageIndex�� ����� �ش� é���� ��ũ�� �����ɴϴ�.
        // IngameData�� �����ν� GetRankForChapter �Լ��� ����մϴ�.
        Define.Rank rank = IngameData.GetRankForChapter(stageIndex);

        // 2. ��ũ ���¿� ���� ��������Ʈ�� �����մϴ�.

        // ���� ��ũ�� 'Unknown' �̶�� (���� �÷������� �ʾ� ��ũ�� ���� ����)
        // 'None'�� �ش��ϴ� ��������Ʈ�� ǥ���մϴ�.
        // 'None' ��������Ʈ�� �迭�� ������(�ε��� 5)�� �ִٰ� �����մϴ�.
        if (rank == Define.Rank.Unknown)
        {
            // sprites.Length - 1 �� �迭�� ������ �ε����� ����ŵ�ϴ�.
            sp.sprite = sprites[sprites.Length - 1];
        }
        else // ��ũ�� �ִٸ� (Bad, Good, Perfect ��)
        {
            // ��ũ enum�� ����(int)�� ��ȯ�Ͽ� �迭�� �ε����� ����մϴ�.
            // (��: Bad = 0, Normal = 1, ...)
            int rankIndex = (int)rank;

            // Ȥ�� �� ������ �����ϱ� ���� �ε����� �迭 ���� ���� �ִ��� Ȯ��
            if (rankIndex >= 0 && rankIndex < sprites.Length)
            {
                sp.sprite = sprites[rankIndex];
            }
        }

        // 3. �̹��� ������Ʈ�� Ȱ��ȭ�ϰ�, ũ�⸦ ���� ��������Ʈ�� ����ϴ�.
        sp.enabled = true;
        sp.SetNativeSize();
    }
}