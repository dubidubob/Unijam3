using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingActivater : MonoBehaviour
{
    public Button panel;

    void Start()
    {
        PauseManager.IsPaused -= ShowPanel;
        PauseManager.IsPaused += ShowPanel;

        if (panel == null)
        {
            panel = GetComponentInChildren<Button>();
        }
    }

    void OnDestroy()
    {
        PauseManager.IsPaused -= ShowPanel;
    }

    private void ShowPanel(bool isStop)
    {
        panel.gameObject.SetActive(isStop);
    }

    public void ReloadScene()
    {
        Managers.Sound.StopBGM();
        Scene currentScene = SceneManager.GetActiveScene();

        Managers.Clear();
        SceneManager.LoadScene(currentScene.name);
    }
}