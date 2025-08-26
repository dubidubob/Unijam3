using System;
using UnityEngine;
using System.Collections.Generic;
using static GamePlayDefine;
public class InputManager
{
    public void Clear()
    {
        InputMouse = null;
        InputDiagonal = null;
        InputEsc = null;
        InputWasd = null;
    }
    public Action<WASDType> InputWasd = null;
    public Action<DiagonalType> InputDiagonal = null;
    public Action<MouseType> InputMouse = null;
    public Action<bool> InputEsc = null;

    private readonly Dictionary<KeyCode, WASDType> keysToCheck = new()
    {
        {KeyCode.W, WASDType.W},
        {KeyCode.S, WASDType.S},
        {KeyCode.A, WASDType.A},
        {KeyCode.D, WASDType.D},
    };
    public readonly List<KeyCode> keysToCheckArrow = new List<KeyCode>
    {
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow
    };


    bool isStoped = false;
    public void HandleStop()
    {
        isStoped = !isStoped;
        Managers.Sound.PauseBGM(isStoped);
        InputEsc?.Invoke(isStoped);
    }

    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleStop();
            return;
        }

        if (Time.timeScale == 0f) return;

        HandleMovementKeys();
        HandleDiagonalKeys();
        HandleMouseKeys();
    }

    private void HandleMouseKeys()
    {
        if (Input.GetMouseButtonDown(0))
        {
            InputMouse?.Invoke(MouseType.Left);
        }
        if (Input.GetMouseButtonDown(1))
        {
            InputMouse?.Invoke(MouseType.Right);
        }
    }

    private void HandleMovementKeys()
    {
        foreach (var key in keysToCheck) // WASD
        {
            if (Managers.Tracker.keyPressCounts[key.Key.ToString()] < 4
                && Input.GetKeyDown(key.Key))
            {
                InputWasd?.Invoke(key.Value);
                Managers.Game.ReceiveKey(key.Value);
            }
        }


        
    }

    private void HandleDiagonalKeys()
    {
        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.UpArrow) && Managers.Tracker.keyPressCounts["LeftUp"] < 4)
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.LeftUp);
            return;
        }

        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.LeftArrow) && Managers.Tracker.keyPressCounts["LeftUp"] < 4)
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.LeftUp);
            return;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.DownArrow) && Managers.Tracker.keyPressCounts["LeftDown"] < 4) // 왼쪽아래
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.LeftDown);
            return;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.LeftArrow) && Managers.Tracker.keyPressCounts["LeftDown"] < 4)
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.LeftDown);
            return;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.UpArrow) && Managers.Tracker.keyPressCounts["RightUp"] < 4) //오른쪽위
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.RightUp);
            return;
        }
        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.RightArrow) && Managers.Tracker.keyPressCounts["RightUp"] < 4)
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.RightUp);
            return;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.DownArrow) && Managers.Tracker.keyPressCounts["RightDown"] < 4) // 오른쪽 아래
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.RightDown);
            return;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.RightArrow) && Managers.Tracker.keyPressCounts["RightDown"] < 4)
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.RightDown);
            return;
        }
    }
}