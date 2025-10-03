using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager
{
    public AudioSource[] _audioSources;
    Dictionary<string, AudioClip> _audioClips;
    [SerializeField]
    public AudioMixer audioMixer;
    public AudioMixerGroup[] audioMixerGroups;
    public AudioSource BGM;
    public AudioSource SFX;

    private string _currentSceneName = "";


    //--- BGM Fade
    private float _originalBGMVolume = 1.0f;
    private Coroutine _fadeCoroutine;
    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {

            audioMixer = Resources.Load<AudioMixer>("Sounds/SoundSetting");
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

         

            _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
            _audioClips = new Dictionary<string, AudioClip>();
            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                _audioSources[i].outputAudioMixerGroup = audioMixer.FindMatchingGroups($"{soundNames[i]}")[0];
                go.transform.parent = root.transform;
            }

            SFX = _audioSources[(int)Define.Sound.SFX];
            SettingNewSceneVolume();
        }
      
    }
    public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.SFX, float pitch = 1.0f,float volume = 1.0f)
    {
        if (audioClip == null)
        {
            return;
        }
        if (type == Define.Sound.BGM)
        {
            if (_audioSources!=null)
                Init();
            AudioSource audioSource = _audioSources[(int)Define.Sound.BGM];

            string newSceneName = SceneManager.GetActiveScene().name;

            // Yejun - Skips if the requested BGM is already playing.
            if (audioSource.isPlaying && audioSource.clip == audioClip)
            {
                bool isSharedBGMPair =
                (_currentSceneName == "MainTitle" && newSceneName == "StageScene") ||
                (_currentSceneName == "StageScene" && newSceneName == "MainTitle");

                if (isSharedBGMPair)
                {
                    _currentSceneName = newSceneName; // �� �̸��� ���� ������ ����
                    return; // BGM�� ���ų� ���� �ʰ� �״�� ��
                }
            }

            _currentSceneName = newSceneName;

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.pitch = pitch;
            audioSource.clip = audioClip;

            //audioSource.volume = volume;
            audioSource.volume = BGMController.CurrentVolumeBGM * volume;
            _originalBGMVolume = audioSource.volume;

            if (_fadeCoroutine != null)
            {
                StaticCoroutine.StopStaticCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _originalBGMVolume = volume;

            audioSource.loop = true;
            BGM = audioSource;
            audioSource.Play();
        }
        else
        {
            if (SFX == null) Init();


            SFX.pitch = pitch;
            SFX.PlayOneShot(audioClip, volume * SFXController.CurrentVolumeSFX);
        }
    }
    /// <summary>
    /// Changes the volume of the currently playing BGM.
    /// </summary>
    /// <param name="volume">The new volume, from 0.0f to 1.0f.</param>
    public void ChangeBGMVolume(float volume)
    {
        if (BGM != null)
        {
            // Clamp the value to ensure it's between 0 and 1.
            BGM.volume = Mathf.Clamp01(volume);
        }
    }

    // --- BGM FadeOut & Restore --- //
    public void BGMFadeOut(float duration = 1.0f)
    {
        // --- ����� �κ� ---
        Debug.Log("����");
        if (_fadeCoroutine != null)
        {
            StaticCoroutine.StopStaticCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StaticCoroutine.StartStaticCoroutine(FadeOutCoroutine(duration));
        // ------------------
    }

    /// <summary>
    /// BGM ������ ���̵� �ƿ� ������ ���� ������ ��� �����մϴ�.
    /// </summary>
    public void BGMRestoreVolume()
    {
        // --- ����� �κ� ---
        if (_fadeCoroutine != null)
        {
            StaticCoroutine.StopStaticCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
        // ------------------

        if (BGM != null)
        {
            BGM.volume = _originalBGMVolume;
        }
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        if (BGM == null) yield break;

        float startVolume = BGM.volume;

        // --- �ð� ��� ��� ���� ---
        // Time.time ��� ���� �ð��� Time.realtimeSinceStartup ���
        float startTime = Time.realtimeSinceStartup;
        float elapsedTime = 0f;
        // -------------------------

        // --- ���� ���� �� ���� ���� ���� ---
        while (elapsedTime < duration)
        {
            // Time.deltaTime ��� startTime�� �������� ���� ��� �ð��� ���
            elapsedTime = Time.realtimeSinceStartup - startTime;

            // Lerp(���۰�, ��ǥ��, ���൵)�� ����� ������ �ε巴�� ����
            BGM.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);

            // WaitForSeconds ��� null�� ��ȯ�Ͽ� ���� �����ӱ��� ���
            // timeScale�� 0�� �� WaitForSeconds�� ������ ��ٸ��� �˴ϴ�.
            yield return null;
        }
        // ---------------------------------

        BGM.volume = 0f; // ���̵� �ƿ� �Ϸ� �� ������ Ȯ���� 0���� ����
        _fadeCoroutine = null; // �ڷ�ƾ �Ϸ� �� ���� ����
    }

    //-----//

    /// <summary>
    /// Changes the base volume for all Sound Effects.
    /// </summary>
    /// <param name="volume">The new volume, from 0.0f to 1.0f.</param>
    public void ChangeSFXVolume(float volume)
    {
        // SFX AudioSource�� �ʱ�ȭ�Ǿ����� Ȯ��
        if (SFX != null)
        {
            SFX.volume = Mathf.Clamp01(volume);
        }
    }
    public void PlayDelayed(AudioClip audioClip, float delay, Define.Sound type = Define.Sound.BGM, float pitch = 1.0f)
    {
        //audioMixer = Managers.Resource.Load<AudioMixer>("AudioMixer/SoundSetting");
        if (audioClip == null)
        {
            return;
        }
        if (type == Define.Sound.BGM)
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.BGM];
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            //audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];
            audioSource.PlayDelayed(3.0f);
        }
        else
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.SFX];
            audioSource.pitch = pitch;
            //audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
            audioSource.PlayOneShot(audioClip);
        }
    }
    public void Play(string path, Define.Sound type = Define.Sound.SFX, float pitch = 1.0f,float volume = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch,volume);
    }
    public void PlayDelayed(string path, float delay, Define.Sound type = Define.Sound.BGM, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        PlayDelayed(audioClip, delay, type, pitch);
    }
    
    public void ChangeBGMPitch(float newPitch)
    {
        if (BGM != null)
        {
            BGM.pitch = newPitch;
        }
    }

    AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.SFX)
    {
        if (path.Contains("Sounds/") == false)
        {
            path = $"Sounds/{path}";
        }
        AudioClip audioClip = null;
        if (type == Define.Sound.BGM)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
        }
        else
        {
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
        }
        if (audioClip == null)
        {
            Debug.Log($"AudioClip Missing : {path}");
        }
        return audioClip;
    }

    public void PauseBGM(bool isStop)
    {
        if (BGM == null) return;

        if (isStop) BGM.Pause();
        else BGM.UnPause();
    }

    public void StopBGM()
    { 
        BGM.Stop();
    }

    public void Stop(AudioSource audioSource)
    {
        audioSource.Stop();
    }
    public void Clear()
    {
        if (_audioSources != null)
            foreach (AudioSource audioSource in _audioSources)
            {
                audioSource.clip = null;
                audioSource.Stop();
            }
        if(_audioClips!=null)
            _audioClips.Clear();
    }
    public void Audiorate(int volume)
    {
        AudioSource audioSource1 = _audioSources[(int)Define.Sound.BGM];
        audioSource1.pitch = volume;
        AudioSource audioSource2 = _audioSources[(int)Define.Sound.SFX];
        audioSource2.pitch = volume;

    }
    public void AudiorateBGM(int volume)
    {
       
    }
    public void AudiorateSFX(int volume)
    {
        AudioSource audioSource2 = _audioSources[(int)Define.Sound.SFX];
        audioSource2.pitch = volume;

    }

    public void SettingNewSceneVolume()
    {
        Managers.Sound.ChangeBGMVolume(BGMController.CurrentVolumeBGM);
        Managers.Sound.ChangeSFXVolume(SFXController.CurrentVolumeSFX);
        
        Debug.Log($"SettingNewSceneVolme : {BGMController.CurrentVolumeBGM},{SFXController.CurrentVolumeSFX}");
    }
}
