using UnityEngine;

public class SettingActivater : MonoBehaviour
{
    public GameObject panel;

    void Start()
    {
        Managers.Input.SettingpopAction += ShowPanel;
    }

    void OnDestroy()
    {
        Managers.Input.SettingpopAction -= ShowPanel;
    }

    private void ShowPanel()
    {
        if(panel.activeSelf)
            panel.SetActive(false);
        else panel.SetActive(true);
    }
}
