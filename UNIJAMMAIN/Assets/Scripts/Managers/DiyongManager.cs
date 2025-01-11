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

        Managers.Input.KeyBoardChecking -= PressButtonArrow;
        Managers.Input.KeyBoardChecking += PressButtonArrow;

        Managers.Input.KeyArrowcodeAction -= PressButtonKeyBoard;
        Managers.Input.KeyArrowcodeAction += PressButtonKeyBoard;
    }
    void PressButtonArrow(KeyCode attackType) //WASD
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

        ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
          .OnComplete(() => ActionGo.transform.DOScale(Vector3.one * originalScale, 0.2f));

    }
    void PressButtonKeyBoard(GamePlayDefine.RangedAttackType attackType)
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

        ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
           .OnComplete(() => ActionGo.transform.DOScale(Vector3.one* originalScale, 0.2f));
    }
}
