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
        // _currentSFXVolumeStep�� private�̹Ƿ� ���� ���� ��� ������Ƽ�� ����
        // ���� ���� ���� �ܺο� �˷��ݴϴ�.
        get
        {
            // �ӽ� �迭�� ����� ���� ������ ��ȯ�մϴ�.
            // (readonly ����� static �޼ҵ�/������Ƽ���� ���� ������ �ȵǱ� ����)
            float[] tempLevels = { 0.0f,0.25f, 0.5f, 0.75f, 1.0f };
            return tempLevels[_currentSFXVolumeStep];
        }
    }
    /// <summary>
    /// This method should be linked to your UI Button's OnClick event.
    /// </summary>
    public void SFXClicked()
    {
        // 1. ���� �ܰ踦 ���� ���� _currentBGMVolumeStep ���� 1 ������ŵ�ϴ�.
        _currentSFXVolumeStep = (_currentSFXVolumeStep + 1) % _volumeLevels.Length;

        // 2. ���Ӱ� ����� _currentBGMVolumeStep ������ ������ ��������Ʈ�� �����ɴϴ�.
        float newVolume = _volumeLevels[_currentSFXVolumeStep];
        image.sprite = sprite[_currentSFXVolumeStep];

        // 3. ȿ������ ����մϴ�.
        Managers.Sound.Play("SFX/Setting_Volume_Button_SFX", Define.Sound.SFX);

        // 4. SoundManager�� ���ο� ���� ���� �����մϴ�.
        Managers.Sound.ChangeSFXVolume(newVolume);

        Debug.Log($"SFX Volume set to: {newVolume * 100}%");

    }
}