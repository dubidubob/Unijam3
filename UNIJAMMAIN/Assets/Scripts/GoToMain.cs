using UnityEngine;
using UnityEngine.UI;

public class GoToTitle : MonoBehaviour
{
    private void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }
    public void OnButtonClick()
    {
        Managers.Scene.LoadScene(Define.Scene.TitleScene);
    }
}
