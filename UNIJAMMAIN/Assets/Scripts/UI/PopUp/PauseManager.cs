using System;
using UnityEngine;

public class PauseManager 
{
    public static Action<bool> IsPaused;
    /// <summary>
    /// true ¸é ¸ØÃã, false¸é Àç»ý
    /// </summary>
    /// <param name="isStop"></param>
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