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
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (Input.anyKey)
        {
            {
                // üũ�� Ű �迭
                KeyCode[] keysToCheck = {
                KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
                };
                KeyCode[] keysToCheckArrow = {
                KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow
            };

                // ���� Ű�� �迭�� �ִ��� Ȯ��
                foreach (KeyCode key in keysToCheck)
                {
                    if (Input.GetKeyDown(key))
                    {
                        if (KeycodeAction != null) KeycodeAction.Invoke(key.ToString());
                        Managers.Game.ReceiveKey(key.ToString());
                        return;
                    }
                }

                foreach (KeyCode key in keysToCheckArrow)
                {
                    if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.UpArrow)) // ������
                    {
                        Managers.Game.ReceiveKey("LeftUp");
                        return;
                    }
                    else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        Managers.Game.ReceiveKey("LeftUp");
                        return;
                    }


                    if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.DownArrow)) // ���ʾƷ�
                    {
                        Managers.Game.ReceiveKey("LeftDown");
                        return;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        Managers.Game.ReceiveKey("LeftDown");
                        return;
                    }

                    if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.UpArrow)) //��������
                    {
                        Managers.Game.ReceiveKey("RightUp");
                        return;
                    }
                    else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        Managers.Game.ReceiveKey("RightUp");
                        return;
                    }

                    if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.DownArrow)) // ������ �Ʒ�
                    {
                        Managers.Game.ReceiveKey("RightDown");
                        return;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        Managers.Game.ReceiveKey("RightDown");
                        return;
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

    }

    public void Clear()
    {
        MouseAction = null;
        KeyAction = null;
    }

}