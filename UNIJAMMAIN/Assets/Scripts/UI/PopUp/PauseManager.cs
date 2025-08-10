using UnityEngine;

public class PauseManager 
{
    // TODO : PausePanel UI 관리 어떻게 할 것인가?
    [SerializeField] GameObject pausePanel;
    public void Init()
    {
        pausePanel?.SetActive(false);

        Managers.Input.SettingpopAction -= ControlTime;
        Managers.Input.SettingpopAction += ControlTime;
    }
    private void ControlTime(bool isStop)
    {
        if (!isStop)
        {
            pausePanel?.SetActive(false);
            Resume();
        }
        else
        {
            pausePanel?.SetActive(true);
            Pause();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
    }
}