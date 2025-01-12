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
           
                // 눌린 키가 배열에 있는지 확인
                foreach (KeyCode key in keysToCheck)
                {
                    
                    if(Managers.Tracker.keyPressCounts[key.ToString()]<4&&Input.GetKeyDown(key)) // WASD
                    {
                     Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
                    KeyBoardChecking.Invoke(key);
                        Debug.Log(key.ToString());
                        Managers.Game.ReceiveKey(key.ToString());
                         Debug.Log($"{key.ToString()}의 KeyPressCounts : {Managers.Tracker.keyPressCounts[key.ToString()]}");

                     }
                 }

                foreach (KeyCode key in keysToCheckArrow)
                {
                    if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.UpArrow)&& Managers.Tracker.keyPressCounts["LeftUp"] < 4) // 왼쪽위
                    {
                    Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
                    KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftUp);
                        Clear();
                        
                        return;
                    }
                    
                    else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.LeftArrow)&&Managers.Tracker.keyPressCounts["LeftUp"] < 4)
                    {
                    Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
                    KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftUp);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.DownArrow) && Managers.Tracker.keyPressCounts["LeftDown"] < 4) // 왼쪽아래
                    {
                    Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
                    KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftDown);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.LeftArrow) && Managers.Tracker.keyPressCounts["LeftDown"] < 4)
                    {
                    Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
                    KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.LeftDown);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.UpArrow) && Managers.Tracker.keyPressCounts["RightUp"] < 4) //오른쪽위
                    {
                    Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
                    KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightUp);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.RightArrow) && Managers.Tracker.keyPressCounts["RightUp"] < 4)
                    {
                    Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
                    KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightUp);
                        Clear();
                        return;
                    }
                    else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKeyDown(KeyCode.DownArrow) && Managers.Tracker.keyPressCounts["RightDown"] < 4) // 오른쪽 아래
                    {
                    Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
                    KeyArrowcodeAction.Invoke(GamePlayDefine.RangedAttackType.RightDown);
                        Clear();
                        return;
                    }
              
                    else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.RightArrow) && Managers.Tracker.keyPressCounts["RightDown"] < 4)
                    {
                    Managers.Sound.Play("Sounds/SFX/Key_Input_SFX");
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