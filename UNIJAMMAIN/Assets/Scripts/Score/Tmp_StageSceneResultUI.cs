using UnityEngine;
using UnityEngine.UI;

public class Tmp_StageSceneResultUI : MonoBehaviour
{
    [Header("B, n, ng, g, p ¼ø")]
    [SerializeField] Sprite[] sprites;
    private Image sp;

    private void Start()
    {
        sp = GetComponent<Image>();
        int rank = (int)IngameData.ChapterRank;
        if (rank >= sprites.Length)
            sp.sprite = null;
        else
            sp.sprite = sprites[rank];

        sp.SetNativeSize();
    }
}