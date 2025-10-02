using UnityEngine;
using UnityEngine.UI;

public class SFX : MonoBehaviour
{
    // A static variable holds the state across all scenes and instances
    private static int _currentSFXVolumeStep = 0;
    public Image image;
    // Define the volume levels. Note that Unity uses 0.0f to 1.0f for volume.
    // Sequence: 25% -> 50% -> 75% -> 100% -> 0%
    private readonly float[] _volumeLevels = { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f };
    [SerializeField] private Sprite[] sprite;


    /// <summary>
    /// This method should be linked to your UI Button's OnClick event.
    /// </summary>
    public void SFXClicked()
    {
        // Get the volume for the current step
        float newVolume = _volumeLevels[_currentSFXVolumeStep];
        image.sprite = sprite[_currentSFXVolumeStep];
        Managers.Sound.Play("Sounds/SFX/Setting_Volume_Button_SFX", Define.Sound.SFX);

        // Tell the SoundManager to change the BGM volume
        // Assuming you have a static Managers class that holds a reference to SoundManager
        Managers.Sound.ChangeSFXVolume(newVolume);

        Debug.Log($"BGM Volume set to: {newVolume * 100}%");

        // Increment the step for the next click, and loop back to 0 if it goes past the end
        _currentSFXVolumeStep = (_currentSFXVolumeStep + 1) % _volumeLevels.Length;
    }
}