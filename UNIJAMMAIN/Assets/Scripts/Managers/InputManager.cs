using System;
using UnityEngine;

public class InputManager
{
    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;
    public Action<GamePlayDefine.RangedAttackType> KeyArrowcodeAction = null;
    public Action SettingpopAction = null;
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
            {
                // 체크할 키 배열
                KeyCode[] keysToCheck = {
                KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
                };
                KeyCode[] keysToCheckArrow = {
                KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow
            };

                // 눌린 키가 배열에 있는지 확인
                foreach (KeyCode key in keysToCheck)
                {
                    
                    if(Managers.Tracker.keyPressCounts[key.ToString()]<4&&Input.GetKeyDown(key))
                    {
                        Debug.Log(key.ToString());
                        Managers.Game.ReceiveKey(key.ToString());
                        return;
                    }
                    /*else
                    {
                        Debug.Log("KeyBlock으로 누르기에 실패하였습니다.");
                        return;
                    }
                    */
                }

                foreach (KeyCode key in keysToCheckArrow)
                {
                    if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.UpArrow)) // 왼쪽위
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
                    else if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.DownArrow)) // 왼쪽아래
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
                    else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.UpArrow)) //오른쪽위
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
                    else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.DownArrow)) // 오른쪽 아래
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

    }

    public void Clear()
    {
        MouseAction = null;
        KeyAction = null;
    }

}