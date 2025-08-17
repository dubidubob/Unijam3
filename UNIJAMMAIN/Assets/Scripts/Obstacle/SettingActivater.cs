using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingActivater : MonoBehaviour
{
    public Button panel;

    void Start()
    {
        Managers.Input.SettingpopAction -= ShowPanel;
        Managers.Input.SettingpopAction += ShowPanel;

        if (panel == null)
        {
            panel = GetComponentInChildren<Button>();
        }
    }

    void OnDestroy()
    {
        Managers.Input.SettingpopAction -= ShowPanel;
    }

    private void ShowPanel(bool isStop)
    {
        panel.gameObject.SetActive(isStop);
    }

    public void ReloadScene()
    {
        Managers.Sound.StopBGM();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}