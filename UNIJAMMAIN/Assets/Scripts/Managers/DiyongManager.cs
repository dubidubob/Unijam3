using UnityEngine;
using DG.Tweening;

public class DiyongManager : MonoBehaviour
{
    private struct KeyData
    {
        public GameObject go;
        public Sprite sp;
    }

    [SerializeField] private KeyData W;
    [SerializeField] private KeyData A;
    [SerializeField] private KeyData S;
    [SerializeField] private KeyData D;
    [SerializeField] private KeyData LeftUp;
    [SerializeField] private KeyData LeftDown;
    [SerializeField] private KeyData RightUp;
    [SerializeField] private KeyData RightDown;

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
            ActionGo = W.go;
        }
        else if (key == "A")
        {
            ActionGo = A.go;
        }
        else if (key == "S")
        {
            ActionGo = S.go;
        }
        else if (key == "D")
        {
            ActionGo = D.go;
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
            ActionGo = LeftUp.go;
        }
        else if (key == "LeftDown")
        {
            ActionGo = LeftDown.go;
        }
        else if (key == "RightUp")
        {
            ActionGo = RightUp.go;
        }
        else if (key == "RightDown")
        {
            ActionGo = RightDown.go;
        }
        else Debug.Log("어떤것도 행해지지않은 오류");

        ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
           .OnComplete(() => ActionGo.transform.DOScale(Vector3.one * originalScale, 0.2f));
    }
}


