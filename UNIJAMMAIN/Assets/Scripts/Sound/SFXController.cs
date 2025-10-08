using UnityEngine;
using UnityEngine.UI;

public class SFXController : MonoBehaviour
{
    // A static variable holds the state across all scenes and instances
    public static int _currentSFXVolumeStep = 1;
    public Image image;
    // Define the volume levels. Note that Unity uses 0.0f to 1.0f for volume.
    // Sequence: 25% -> 50% -> 75% -> 100% -> 0%
    private readonly float[] _volumeLevels = { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f };
    [SerializeField] private Sprite[] sprite;

    private void Awake()
    {
        image.sprite = sprite[_currentSFXVolumeStep];
    }

    public static float CurrentVolumeSFX
    {
        // _currentSFXVolumeStep은 private이므로 직접 접근 대신 프로퍼티를 통해
        // 계산된 볼륨 값을 외부에 알려줍니다.
        get
        {
            // 임시 배열을 만들어 현재 볼륨을 반환합니다.
            // (readonly 멤버는 static 메소드/프로퍼티에서 직접 접근이 안되기 때문)
            float[] tempLevels = { 0.0f,0.25f, 0.5f, 0.75f, 1.0f };
            return tempLevels[_currentSFXVolumeStep];
        }
    }
    /// <summary>
    /// This method should be linked to your UI Button's OnClick event.
    /// </summary>
    public void SFXClicked()
    {
        // 1. 다음 단계를 위해 먼저 _currentBGMVolumeStep 값을 1 증가시킵니다.
        _currentSFXVolumeStep = (_currentSFXVolumeStep + 1) % _volumeLevels.Length;

        // 2. 새롭게 변경된 _currentBGMVolumeStep 값으로 볼륨과 스프라이트를 가져옵니다.
        float newVolume = _volumeLevels[_currentSFXVolumeStep];
        image.sprite = sprite[_currentSFXVolumeStep];

        // 3. 효과음을 재생합니다.
        Managers.Sound.Play("SFX/Setting_Volume_Button_SFX", Define.Sound.SFX);

        // 4. SoundManager에 새로운 볼륨 값을 적용합니다.
        Managers.Sound.ChangeSFXVolume(newVolume);

        Debug.Log($"SFX Volume set to: {newVolume * 100}%");

    }
}