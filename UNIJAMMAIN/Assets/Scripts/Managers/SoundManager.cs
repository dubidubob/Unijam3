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
                    _currentSceneName = newSceneName; // 씬 이름만 현재 씬으로 갱신
                    return; // BGM을 끄거나 켜지 않고 그대로 둠
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
        // --- 변경된 부분 ---
        Debug.Log("진입");
        if (_fadeCoroutine != null)
        {
            StaticCoroutine.StopStaticCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StaticCoroutine.StartStaticCoroutine(FadeOutCoroutine(duration));
        // ------------------
    }

    /// <summary>
    /// BGM 볼륨을 페이드 아웃 이전의 원래 값으로 즉시 복원합니다.
    /// </summary>
    public void BGMRestoreVolume()
    {
        // --- 변경된 부분 ---
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

        // --- 시간 계산 방식 변경 ---
        // Time.time 대신 실제 시간인 Time.realtimeSinceStartup 사용
        float startTime = Time.realtimeSinceStartup;
        float elapsedTime = 0f;
        // -------------------------

        // --- 루프 조건 및 내부 로직 변경 ---
        while (elapsedTime < duration)
        {
            // Time.deltaTime 대신 startTime을 기준으로 실제 경과 시간을 계산
            elapsedTime = Time.realtimeSinceStartup - startTime;

            // Lerp(시작값, 목표값, 진행도)를 사용해 볼륨을 부드럽게 조절
            BGM.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);

            // WaitForSeconds 대신 null을 반환하여 다음 프레임까지 대기
            // timeScale이 0일 때 WaitForSeconds는 영원히 기다리게 됩니다.
            yield return null;
        }
        // ---------------------------------

        BGM.volume = 0f; // 페이드 아웃 완료 후 볼륨을 확실히 0으로 설정
        _fadeCoroutine = null; // 코루틴 완료 후 참조 정리
    }

    //-----//

    /// <summary>
    /// Changes the base volume for all Sound Effects.
    /// </summary>
    /// <param name="volume">The new volume, from 0.0f to 1.0f.</param>
    public void ChangeSFXVolume(float volume)
    {
        // SFX AudioSource가 초기화되었는지 확인
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
