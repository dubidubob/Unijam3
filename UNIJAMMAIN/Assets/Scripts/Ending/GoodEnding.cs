using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GoodEnding : MonoBehaviour
{
    public GameObject defaultMonk;
    public GameObject meditate;
    public GameObject human;
    public GameObject hat;
    public GameObject body;
    public GameObject egg;
    public GameObject ByeUI;
    public GameObject bg;
    public Sprite fry;
    public Image erun;

    private void Start()
    {
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        defaultMonk.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        defaultMonk.SetActive(false);
        meditate.SetActive(true);
        ByeUI.SetActive(false);
        bg.SetActive(true);
        Camera.main.DOOrthoSize(3.5f, 4f);
        Managers.Sound.Play("SFX/Game_End_Holy_BGM");
        yield return new WaitForSeconds(4f);
        Camera.main.transform.DOMoveY(1f, 2f);
        yield return new WaitForSeconds(2f);
        human.transform.DOMove(new Vector3(0, 1f, 0), 1.5f);
        yield return new WaitForSeconds(1.5f);
        hat.transform.DOMove(new Vector3(0, -1f, 0), 1.2f)
            .SetEase(Ease.InSine);
        yield return new WaitForSeconds(1.2f);
        //Ÿ��~~!!
        egg.transform.DOMove(new Vector3(0, 0.5f, 0), 1.5f);
        yield return new WaitForSeconds(2f);
        egg.transform.DOMove(new Vector3(0, -2f, 0), 1f);
        yield return new WaitForSeconds(1.5f);
        Managers.Sound.Play("SFX/Egg_Crack_SFX_Loud");
        egg.GetComponent<SpriteRenderer>().sprite = fry;

        egg.transform.localScale = Vector3.one * 0.1f;
        yield return new WaitForSeconds(2f);
        erun.DOFade(1f, 1.5f);
    }

    public void OnClickGohome()
    {
        Managers.Clear();
        Managers.Scene.LoadScene("MainTitle");
    }
}
