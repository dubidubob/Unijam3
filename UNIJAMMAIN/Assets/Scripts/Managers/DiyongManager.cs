using UnityEngine;
using DG.Tweening;
using System.Collections;

public class DiyongManager : MonoBehaviour
{
    [System.Serializable]
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

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite nonActionSprite;

    private GameObject ActionGo;
    private Sprite actionSprite;
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
            actionSprite = W.sp;
        }
        else if (key == "A")
        {
            ActionGo = A.go;
            actionSprite = A.sp;
        }
        else if (key == "S")
        {
            ActionGo = S.go;
            actionSprite = S.sp;
        }
        else if (key == "D")
        {
            ActionGo = D.go;
            actionSprite = D.sp;
        }
        else Debug.Log("어떤것도 행해지지않은 오류");

        if (ActionGo != null)
        {
            ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.1f)
      .OnComplete(() => ActionGo.transform.DOScale(Vector3.one * originalScale, 0.1f));
        }
        
        if(spriteRenderer!=null)
            spriteRenderer.sprite = actionSprite;
        StartCoroutine(SetOriginal());
    }

    private IEnumerator SetOriginal()
    {
        yield return new WaitForSeconds(0.5f);
        if(nonActionSprite != null)
            spriteRenderer.sprite = nonActionSprite;
    }
    void PressButtonKeyBoard(GamePlayDefine.RangedAttackType attackType)
    {
        string key = attackType.ToString();
        if (key == "LeftUp")
        {
            ActionGo = LeftUp.go;
            actionSprite = LeftUp.sp;
        }
        else if (key == "LeftDown")
        {
            ActionGo = LeftDown.go;
            actionSprite = LeftDown.sp;
        }
        else if (key == "RightUp")
        {
            ActionGo = RightUp.go;
            actionSprite = RightUp.sp;
        }
        else if (key == "RightDown")
        {
            ActionGo = RightDown.go;
            actionSprite = RightDown.sp;
        }
        else Debug.Log("어떤것도 행해지지않은 오류");

        ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.1f)
           .OnComplete(() => ActionGo.transform.DOScale(Vector3.one * originalScale, 0.1f));

        spriteRenderer.sprite = actionSprite;
    }
}


