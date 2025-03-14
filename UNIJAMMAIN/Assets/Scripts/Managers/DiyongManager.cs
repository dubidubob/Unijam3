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

    private Sprite actionSprite;
    private float originalScale = 0.25f;
    private float upScaleAmount = 0.2f;

    public Sprite[] StackSprite;
    private void Awake()
    {

        Managers.Input.KeyBoardChecking -= PressButtonArrow;
        Managers.Input.KeyBoardChecking += PressButtonArrow;

        Managers.Input.KeyArrowcodeAction -= PressButtonKeyBoard;
        Managers.Input.KeyArrowcodeAction += PressButtonKeyBoard;

        Managers.Game.MissedKeyUpdate -= MissedKeyPressChecking;
        Managers.Game.MissedKeyUpdate += MissedKeyPressChecking;

        Managers.Tracker.KeyArrowMissed -= MissedKeyPressedArrow;
        Managers.Tracker.KeyArrowMissed += MissedKeyPressedArrow;

        Managers.Tracker.KeyFree -= KeyFreeKeyBoard;
        Managers.Tracker.KeyFree += KeyFreeKeyBoard;
    }
    void KeyFreeKeyBoard(string key)
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
        else if (key == "LeftUp")
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
        if (gogo != null)
        {
            gogo.GetComponentsInChildren<SpriteRenderer>()[1].sprite = null;
        }
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
            Debug.Log(count);
            if (count == 0) { gogo.GetComponentsInChildren<SpriteRenderer>()[1].sprite = null;  }
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

    private void OnDestroy()
    {
        spriteRenderer = null;
    }
    public void text()
    {
        Debug.Log("�׽���������");
    }
    void PressButtonArrow(KeyCode attackType) // WASD
    {
        GameObject gogo;
        string key = attackType.ToString();
        if (key == "W")
        {
            gogo = W.go;
            actionSprite = W.sp;
        }
        else if (key == "A")
        {
            gogo = A.go;
            actionSprite = A.sp;
        }
        else if (key == "S")
        {
            gogo = S.go;
            actionSprite = S.sp;
        }
        else if (key == "D")
        {
            gogo = D.go;
            actionSprite = D.sp;
        }
        else
        {
            gogo = null;
        }

        if (gogo != null)
        {
            // ���� ������ gogo ������ ����
            GameObject target = gogo;

            target.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
                .OnComplete(() => target.transform.DOScale(Vector3.one * originalScale, 0.2f));
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = actionSprite;
            StartCoroutine(SetOriginal());
        }
    }

    private IEnumerator SetOriginal()
    {
        yield return new WaitForSeconds(0.5f);
        if(nonActionSprite != null)
            spriteRenderer.sprite = nonActionSprite;
    }
    void PressButtonKeyBoard(GamePlayDefine.RangedAttackType attackType)
    {
        GameObject gogo;
        string key = attackType.ToString();
        if (key == "LeftUp")
        {
            gogo = LeftUp.go;
            actionSprite = LeftUp.sp;
        }
        else if (key == "LeftDown")
        {
            gogo = LeftDown.go;
            actionSprite = LeftDown.sp;
        }
        else if (key == "RightUp")
        {
            gogo = RightUp.go;
            actionSprite = RightUp.sp;
        }
        else if (key == "RightDown")
        {
            gogo = RightDown.go;
            actionSprite = RightDown.sp;
        }
        else gogo = null;
        if (gogo != null)
        {
            GameObject target = gogo;
            target.transform.DOScale(Vector3.one * (originalScale + upScaleAmount), 0.2f)
               .OnComplete(() => target.transform.DOScale(Vector3.one * originalScale, 0.2f));

            Animator animator = spriteRenderer.transform.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
                spriteRenderer.sprite = actionSprite;
            }
        }
    }
}


