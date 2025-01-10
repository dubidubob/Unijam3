using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;
    public Action<GamePlayDefine.RangedAttackType> KeyArrowcodeAction = null;
    bool _pressed = false;
    public void OnUpdate()
    {
        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    return;
        //}
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
                        Managers.Game.ReceiveKey(key.ToString());
                        return;
                    }
                }

                foreach (KeyCode key in keysToCheckArrow)
                {
                    if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.UpArrow)) // ������
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftUp);
                        return;
                    }
                    else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftUp);  
                        return;
                    }

                    if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.DownArrow)) // ���ʾƷ�
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftDown);
                        return;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftDown);
                        return;
                    }

                    if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.UpArrow)) //��������
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightUp);
                        return;
                    }
                    else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightUp);
                        return;
                    }

                    if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.DownArrow)) // ������ �Ʒ�
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightDown);
                        return;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightDown);
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