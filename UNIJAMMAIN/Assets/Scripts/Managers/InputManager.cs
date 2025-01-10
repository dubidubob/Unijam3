using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;
    public Action<String> KeycodeAction = null;
    bool _pressed = false;
    public void OnUpdate()
    {
        Debug.Log("key invoked");
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        } 
        if (Input.anyKey)
        {
            {
                // 체크할 키 배열
                KeyCode[] keysToCheck = {
                KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
                KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow
            };

                // 눌린 키가 배열에 있는지 확인
                foreach (KeyCode key in keysToCheck)
                {
                    if (Input.GetKeyDown(key))
                    {
                        if(KeycodeAction != null) KeycodeAction.Invoke(key.ToString());
                        Managers.Game.ReceiveKey(key.ToString());
                        return;
                    }
                }
            }
        }
        if (MouseAction != null)
        {
            if (Input.GetMouseButton(0))
            {
                MouseAction.Invoke(Define.MouseEvent.Press);
                _pressed = true;
            }
            else
            {
                if (_pressed)
                {
                    //MouseAction.Invoke(Define.MouseEvent.Click);
                    MouseAction.Invoke(Define.MouseEvent.End);
                }
                _pressed = false;
            }
        }
    }
    public void Clear()
    {
        MouseAction = null;
        KeyAction = null;
    }
}
