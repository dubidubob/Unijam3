using System;
using UnityEngine;
using System.Collections.Generic;
public class InputManager
{
    public void Clear()
    {
        KeyAction = null;
        MouseAction = null;
        KeyArrowcodeAction = null;
        SettingpopAction = null;
        KeyBoardChecking = null;
    }

    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;
    public Action<GamePlayDefine.DiagonalType> KeyArrowcodeAction = null;
    public Action SettingpopAction = null;
    public Action<KeyCode> KeyBoardChecking = null;

    public readonly List<KeyCode> keysToCheck = new List<KeyCode>
                {
                KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
                };
    public readonly List<KeyCode> keysToCheckArrow = new List<KeyCode> {
                 KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow
                };
    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SettingpopAction?.Invoke();
            return;
        }

        if (Time.timeScale == 0f) return;

        HandleMovementKeys();
        HandleDiagonalKeys();
    }

    private void HandleMovementKeys()
    {
        foreach (KeyCode key in keysToCheck) // WASD
        {
            if (Managers.Tracker.keyPressCounts[key.ToString()] < 4
                && Input.GetKeyDown(key))
            {
                KeyBoardChecking?.Invoke(key);
                Managers.Game.ReceiveKey(key.ToString());
            }
        }
    }

    private void HandleDiagonalKeys()
    {
        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.UpArrow) && Managers.Tracker.keyPressCounts["LeftUp"] < 4)
        {
            KeyArrowcodeAction?.Invoke(GamePlayDefine.DiagonalType.LeftUp);
            return;
        }

        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.LeftArrow) && Managers.Tracker.keyPressCounts["LeftUp"] < 4)
        {
            KeyArrowcodeAction?.Invoke(GamePlayDefine.DiagonalType.LeftUp);
            return;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.DownArrow) && Managers.Tracker.keyPressCounts["LeftDown"] < 4) // 왼쪽아래
        {
            KeyArrowcodeAction?.Invoke(GamePlayDefine.DiagonalType.LeftDown);
            return;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.LeftArrow) && Managers.Tracker.keyPressCounts["LeftDown"] < 4)
        {
            KeyArrowcodeAction?.Invoke(GamePlayDefine.DiagonalType.LeftDown);
            return;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.UpArrow) && Managers.Tracker.keyPressCounts["RightUp"] < 4) //오른쪽위
        {
            KeyArrowcodeAction?.Invoke(GamePlayDefine.DiagonalType.RightUp);
            return;
        }
        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.RightArrow) && Managers.Tracker.keyPressCounts["RightUp"] < 4)
        {
            KeyArrowcodeAction?.Invoke(GamePlayDefine.DiagonalType.RightUp);
            return;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.DownArrow) && Managers.Tracker.keyPressCounts["RightDown"] < 4) // 오른쪽 아래
        {
            KeyArrowcodeAction?.Invoke(GamePlayDefine.DiagonalType.RightDown);
            return;
        }

        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.RightArrow) && Managers.Tracker.keyPressCounts["RightDown"] < 4)
        {
            KeyArrowcodeAction?.Invoke(GamePlayDefine.DiagonalType.RightDown);
            return;
        }
    }
}