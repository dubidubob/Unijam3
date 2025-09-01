using UnityEngine;

public class TutorialDiagonalKeyUI : MonoBehaviour
{
    private void OnEnable()
    {
        if(IngameData.ChapterIdx != 0)
            Destroy(gameObject);
    }

    public void OnDisableCalledByParent()
    {
        Destroy(gameObject);
    }
}