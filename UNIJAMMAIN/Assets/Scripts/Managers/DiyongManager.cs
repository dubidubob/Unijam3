using UnityEngine;
using DG.Tweening;

public class DiyongManager : MonoBehaviour
{
    public GameObject W, A, S, D, LeftUp, LeftDown, RightUp, RightDown;

    private GameObject ActionGo;
    private float originalScale = 0.25f;
    private float upScaleAmount = 0.2f;
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
        else Debug.Log("��͵� ������������ ����");

        if (ActionGo != null)
        {
            ActionGo.transform.DOScale(Vector3.one * upScaleAmount, 0.2f)
              .OnComplete(() => { ActionGo.transform.DOScale(Vector3.one, 0.2f);
                  ActionGoNull();
              });
        }


        ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
          .OnComplete(() => ActionGo.transform.DOScale(Vector3.one * originalScale, 0.2f));

    }
    void ActionGoNull()
    {
        Debug.Log("ActionGoNull ����");
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
        else Debug.Log("��͵� ������������ ����");

        ActionGo.transform.DOScale(Vector3.one * upScaleAmount, 0.2f)
           .OnComplete(() =>
           {
               ActionGo.transform.DOScale(Vector3.one, 0.2f);
               ActionGoNull();
           });
           }
 } 

        ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
           .OnComplete(() => ActionGo.transform.DOScale(Vector3.one* originalScale, 0.2f));
    }
}

