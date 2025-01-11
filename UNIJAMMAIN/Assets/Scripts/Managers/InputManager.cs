using System;
using UnityEngine;
using System.Collections.Generic;
public class InputManager
{
    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;
    public Action<GamePlayDefine.RangedAttackType> KeyArrowcodeAction = null;
    public Action SettingpopAction = null;
    public List<KeyCode> keysToCheck = new List<KeyCode> {
                KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
                };
    public List<KeyCode> keysToCheckArrow = new List<KeyCode> {
                 KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow
                };

    public Action<KeyCode> KeyBoardChecking = null;
    public void OnUpdate()
    {
        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    return;
        //}
        if (Input.anyKey)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SettingpopAction.Invoke();
            }   
           
                // ���� Ű�� �迭�� �ִ��� Ȯ��
                foreach (KeyCode key in keysToCheck)
                {
                    
                    if(Managers.Tracker.keyPressCounts[key.ToString()]<4&&Input.GetKeyDown(key)) // WASD
                    {
                        KeyBoardChecking.Invoke(key);
                        Debug.Log(key.ToString());
                        Managers.Game.ReceiveKey(key.ToString());
                        return;
                    }
                    /*else
                    {
                        Debug.Log("KeyBlock���� �����⿡ �����Ͽ����ϴ�.");
                        return;
                    }
                    */
                }

                foreach (KeyCode key in keysToCheckArrow)
                {
                    if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.UpArrow)) // ������
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftUp);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftUp);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.DownArrow)) // ���ʾƷ�
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftDown);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftDown);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.UpArrow)) //��������
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightUp);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightUp);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.DownArrow)) // ������ �Ʒ�
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightDown);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightDown);
                        Clear();
                        return;
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