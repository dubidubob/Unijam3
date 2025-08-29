using System;
using UnityEngine;

public class PauseManager 
{
    public static Action<bool> IsPaused;
    public static void ControlTime(bool isStop)
    {
        Managers.Sound.PauseBGM(isStop);
        IngameData.Pause = isStop;
        IsPaused?.Invoke(isStop);
        if (!isStop)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }
    }
}