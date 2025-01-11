using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DiyongManager : MonoBehaviour
{
    public GameObject W, A, S, D, LeftUp, LeftDown, RightUp, RightDown;

    private GameObject ActionGo;
    private float upScaleAmount = 1.2f;
    private void Start()
    {

        Managers.Input.KeyBoardChecking -= PressButtonKeyBoard;
        Managers.Input.KeyBoardChecking += PressButtonKeyBoard;

        Managers.Input.KeyArrowcodeAction -= PressButtonArrow;
        Managers.Input.KeyArrowcodeAction += PressButtonArrow;
    }
    void PressButtonKeyBoard(KeyCode attackType) //WASD
    {
        string key = attackType.ToString();
        if (key == "W")
        {
            ActionGo = W;
        }
        else if (key == "A")
        {
            ActionGo = A;
        }
        else if (key == "S")
        {
            ActionGo = S;
        }
        else if (key == "D")
        {
            ActionGo = D;
        }
        else Debug.Log("어떤것도 행해지지않은 오류");
        if (ActionGo != null)
        {
            ActionGo.transform.DOScale(Vector3.one * upScaleAmount, 0.2f)
              .OnComplete(() => { ActionGo.transform.DOScale(Vector3.one, 0.2f);
                  ActionGoNull();
              });
        }

    }
    void ActionGoNull()
    {
        Debug.Log("ActionGoNull 실행");
        ActionGo = null;
    }
    void PressButtonArrow(GamePlayDefine.RangedAttackType attackType)
    {
        string key = attackType.ToString();
        if (key == "LeftUp")
        {
            ActionGo = LeftUp;
        }
        else if (key == "LeftDown")
        {
            ActionGo = LeftDown;
        }
        else if (key == "RightUp")
        {
            ActionGo = RightUp;
        }
        else if (key == "RightDown")
        {
            ActionGo = RightDown;
        }
        else Debug.Log("어떤것도 행해지지않은 오류");

        ActionGo.transform.DOScale(Vector3.one * upScaleAmount, 0.2f)
           .OnComplete(() =>
           {
               ActionGo.transform.DOScale(Vector3.one, 0.2f);
               ActionGoNull();
           });
           }
 } 

