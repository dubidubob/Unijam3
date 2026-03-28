using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class InitSceneManager : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_EDITOR
Debug.unityLogger.logEnabled = false;
#endif
    }
    private async void Start()
    {
        Managers.UI.ShowPopUpUI<Init_PopUp>();
        // 로컬라이제이션 시스템이 완전히 준비될 때까지 안전하게 대기
        await LocalizationSettings.InitializationOperation;

        // 준비가 끝났으니 진짜 타이틀 씬으로 이동!
        // 타이틀 씬이 열릴 때는 시스템이 이미 켜져 있으므로 에러가 절대 발생하지 않음.
        SceneManager.LoadScene("MainTitle");
    }
}