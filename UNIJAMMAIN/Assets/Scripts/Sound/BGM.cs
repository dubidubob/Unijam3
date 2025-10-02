using UnityEngine;
using UnityEngine.UI;

public class BGM : MonoBehaviour
{
    // A static variable holds the state across all scenes and instances
    private static int _currentBGMVolumeStep = 0;
    public Image image;
    // Define the volume levels. Note that Unity uses 0.0f to 1.0f for volume.
    // Sequence: 25% -> 50% -> 75% -> 100% -> 0%
    private readonly float[] _volumeLevels = {0.0f,0.25f, 0.5f, 0.75f, 1.0f};
    [SerializeField] private Sprite[] sprite;

    private void Awake()
    {
        image.sprite = sprite[_currentBGMVolumeStep];
    }

    public static float CurrentVolumeBGM
    {
        // _currentBGMVolumeStep은 private이므로 직접 접근 대신 프로퍼티를 통해
        // 계산된 볼륨 값을 외부에 알려줍니다.
        get
        {
            // 임시 배열을 만들어 현재 볼륨을 반환합니다.
            // (readonly 멤버는 static 메소드/프로퍼티에서 직접 접근이 안되기 때문)
            float[] tempLevels = { 0.25f, 0.5f, 0.75f, 1.0f, 0.0f };
            return tempLevels[_currentBGMVolumeStep];
        }
    }

    /// <summary>
    /// This method should be linked to your UI Button's OnClick event.
    /// </summary>
    public void BGMClicked()
    {
        // Get the volume for the current step
        float newVolume = _volumeLevels[_currentBGMVolumeStep];
        image.sprite = sprite[_currentBGMVolumeStep];

        Managers.Sound.Play("SFX/Setting_Volume_Button_SFX", Define.Sound.SFX);

        // Tell the SoundManager to change the BGM volume
        // Assuming you have a static Managers class that holds a reference to SoundManager
        Managers.Sound.ChangeBGMVolume(newVolume);

        Debug.Log($"BGM Volume set to: {newVolume * 100}%");

        // Increment the step for the next click, and loop back to 0 if it goes past the end
        _currentBGMVolumeStep = (_currentBGMVolumeStep + 1) % _volumeLevels.Length;
        
    }
}