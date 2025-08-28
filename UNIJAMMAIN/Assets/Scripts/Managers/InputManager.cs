using System;
using System.Collections.Generic;
using UnityEngine;
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
        IngameData.Pause = isStoped;
        if (isStoped)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
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
        if (IngameData.Pause) return;

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
            if (Input.GetKeyDown(key.Key))
            {
                InputWasd?.Invoke(key.Value);
                Managers.Game.ReceiveKey(key.Value);
            }
        }


        
    }

    private void HandleDiagonalKeys()
    {
        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.UpArrow))
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.LeftUp);
            return;
        }

        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.LeftUp);
            return;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.DownArrow))
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.LeftDown);
            return;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.LeftDown);
            return;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.UpArrow))
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.RightUp);
            return;
        }
        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.RightArrow))
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.RightUp);
            return;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.DownArrow))
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.RightDown);
            return;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.RightArrow))
        {
            InputDiagonal?.Invoke(GamePlayDefine.DiagonalType.RightDown);
            return;
        }
    }
}