using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

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

    public Sprite[] StackSprite;
    private void Start()
    {

        Managers.Input.KeyBoardChecking -= PressButtonArrow;
        Managers.Input.KeyBoardChecking += PressButtonArrow;

        Managers.Input.KeyArrowcodeAction -= PressButtonKeyBoard;
        Managers.Input.KeyArrowcodeAction += PressButtonKeyBoard;

        Managers.Game.MissedKeyUpdate -= MissedKeyPressChecking;
        Managers.Game.MissedKeyUpdate += MissedKeyPressChecking;

        Managers.Tracker.KeyArrowMissed -= MissedKeyPressedArrow;
        Managers.Tracker.KeyArrowMissed += MissedKeyPressedArrow;
   
    }
    void MissedKeyPressChecking(string key) //AWSD
    {
        GameObject gogo;
        if (key == "W")
        {
            gogo = W.go;
        }
        else if (key == "A")
        {
            gogo = A.go;

        }
        else if (key == "S")
        {
            gogo = S.go;
        }
        else if (key == "D")
        {
            gogo = D.go;
        }
        else gogo=null;
        if (gogo != null)
        {
            int count = Managers.Tracker.keyPressCounts[key];
            if (count <= 3)
            {
                gogo.GetComponentsInChildren<SpriteRenderer>()[1].sprite = StackSprite[Managers.Tracker.keyPressCounts[key]];
                gogo.GetComponentsInChildren<SpriteRenderer>()[1].transform.localScale = new Vector3(5f, 5f);
            }
            else if(count==4)
            {
                gogo.GetComponentsInChildren<SpriteRenderer>()[1].sprite = StackSprite[Managers.Tracker.keyPressCounts[key]];
                gogo.GetComponentsInChildren<SpriteRenderer>()[1].transform.localScale = new Vector3(1f, 1f);
            }
        }
    }

    public void MissedKeyPressedArrow(string key)
    {
        GameObject gogo;
        if (key == "LeftUp")
        {
            gogo = LeftUp.go;
        }
        else if (key == "LeftDown")
        {
            gogo = LeftDown.go;
        }
        else if (key == "RightUp")
        {
            gogo = RightUp.go;
        }
        else if (key == "RightDown")
        {
            gogo = RightDown.go;
        }
        else gogo = null;
        
        if(gogo!=null)
        {
            int count = Managers.Tracker.keyPressCounts[key];
            if (count <= 3)
            {
                gogo.GetComponentsInChildren<SpriteRenderer>()[1].sprite = StackSprite[Managers.Tracker.keyPressCounts[key]];
                gogo.GetComponentsInChildren<SpriteRenderer>()[1].transform.localScale = new Vector3(5f, 5f);
            }
            else if (count == 4)
            {
                gogo.GetComponentsInChildren<SpriteRenderer>()[1].sprite = StackSprite[Managers.Tracker.keyPressCounts[key]];
                gogo.GetComponentsInChildren<SpriteRenderer>()[1].transform.localScale = new Vector3(1f, 1f);
            }
        }


    }
    private void Update()
    {
        /*
        W.go.GetComponentInChildren<SpriteRenderer>(true).sprite = StackSprite[Managers.Tracker.keyPressCounts["W"]];
        A.go.GetComponentInChildren<SpriteRenderer>(true).sprite = StackSprite[Managers.Tracker.keyPressCounts["A"]];
        S.go.GetComponentInChildren<SpriteRenderer>(true).sprite = StackSprite[Managers.Tracker.keyPressCounts["S"]];
        D.go.GetComponentInChildren<SpriteRenderer>(true).sprite = StackSprite[Managers.Tracker.keyPressCounts["D"]];
        LeftDown.go.GetComponentInChildren<SpriteRenderer>(true).sprite = StackSprite[Managers.Tracker.keyPressCounts["LeftDown"]];
        LeftUp.go.GetComponentInChildren<SpriteRenderer>(true).sprite = StackSprite[Managers.Tracker.keyPressCounts["LeftUp"]];
        RightDown.go.GetComponentInChildren<SpriteRenderer>(true).sprite = StackSprite[Managers.Tracker.keyPressCounts["RightDown"]];
        RightUp.go.GetComponentInChildren<SpriteRenderer>(true).sprite = StackSprite[Managers.Tracker.keyPressCounts["RightUp"]];
        */

    }
    public void text()
    {
        Debug.Log("테스스스스스");
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
            ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
      .OnComplete(() => ActionGo.transform.DOScale(Vector3.one * originalScale, 0.2f));
        }
        
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

        ActionGo.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
           .OnComplete(() => ActionGo.transform.DOScale(Vector3.one * originalScale, 0.2f));

        spriteRenderer.sprite = actionSprite;
    }
}


