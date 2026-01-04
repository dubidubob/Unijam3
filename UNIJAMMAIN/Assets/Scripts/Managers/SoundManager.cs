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
    public AudioSource SubBGM;

    private string _currentSceneName = "";


    //--- BGM Fade
    private float _originalBGMVolume = 1.0f;
    private Coroutine _fadeCoroutine;
    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");

        // root가 없으면 새로 만듦
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);
        }

        // [수정됨] root가 있든 없든, 리소스 로드와 AudioSource 배열 연결은 
        // _audioSources가 비어있다면 무조건 실행해야 함
        if (_audioSources == null || _audioSources.Length == 0)
        {
            audioMixer = Resources.Load<AudioMixer>("Sounds/SoundSetting"); // 경로 확인 필요

            _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
            _audioClips = new Dictionary<string, AudioClip>();

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                // 기존에 만들어진 자식 오브젝트가 있는지 확인
                Transform child = root.transform.Find(soundNames[i]);
                if (child == null)
                {
                    GameObject go = new GameObject { name = soundNames[i] };
                    _audioSources[i] = go.AddComponent<AudioSource>();
                    go.transform.parent = root.transform;
                }
                else
                {
                    _audioSources[i] = child.GetComponent<AudioSource>();
                }

                // 믹서 그룹 연결 (믹서가 로드 성공했을 때만)
                if (audioMixer != null)
                {
                    var groups = audioMixer.FindMatchingGroups($"{soundNames[i]}");
                    if (groups.Length > 0)
                        _audioSources[i].outputAudioMixerGroup = groups[0];
                }
            }

            SFX = _audioSources[(int)Define.Sound.SFX];
            // BGM과 SubBGM 변수도 여기서 미리 할당해두는 것이 안전함
            BGM = _audioSources[(int)Define.Sound.BGM];
            SubBGM = _audioSources[(int)Define.Sound.SubBGM];

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
            if (_audioSources == null)
                Init();
            AudioSource audioSource = _audioSources[(int)Define.Sound.BGM];

            string newSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"--- BGM Play 요청 ---");
            Debug.Log($"이전 씬: '{_currentSceneName}', 현재 씬: '{newSceneName}'");

            // Yejun - Skips if the requested BGM is already playing.
            if (audioSource.isPlaying && audioSource.clip == audioClip)
            {
                bool isSharedBGMPair =
                (_currentSceneName == "MainTitle" && newSceneName == "StageScene") ||
                (_currentSceneName == "StageScene" && newSceneName == "MainTitle");

                Debug.Log($"BGM 공유 조건 검사 결과: {isSharedBGMPair}");

                if (isSharedBGMPair)
                {

                    Debug.Log("===> BGM을 유지합니다.");

                    _currentSceneName = newSceneName; // 씬 이름만 현재 씬으로 갱신
                    return; // BGM을 끄거나 켜지 않고 그대로 둠
                }
            }

            Debug.LogWarning("===> BGM을 처음부터 다시 재생합니다!");

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
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "GamePlayScene")
            {
                audioSource.loop = false;
            }
            else
            {
                audioSource.loop = true;
            }

            BGM = audioSource;
            audioSource.Play();
        }
        else if(type==Define.Sound.SubBGM)
        {
            if (_audioSources == null)
                Init();
            AudioSource audioSource = _audioSources[(int)Define.Sound.SubBGM];

            string newSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"--- BGM Play 요청 ---");
            Debug.Log($"이전 씬: '{_currentSceneName}', 현재 씬: '{newSceneName}'");

            // Yejun - Skips if the requested BGM is already playing.
            if (audioSource.isPlaying && audioSource.clip == audioClip)
            {
                bool isSharedBGMPair =
                (_currentSceneName == "MainTitle" && newSceneName == "StageScene") ||
                (_currentSceneName == "StageScene" && newSceneName == "MainTitle");

                Debug.Log($"BGM 공유 조건 검사 결과: {isSharedBGMPair}");

                if (isSharedBGMPair)
                {

                    Debug.Log("===> Option BGM 유지합니다.");

                    _currentSceneName = newSceneName; // 씬 이름만 현재 씬으로 갱신
                    return; // BGM을 끄거나 켜지 않고 그대로 둠
                }
            }

            Debug.LogWarning("===> Option BGM을 처음부터 다시 재생합니다!");

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
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "GamePlayScene")
            {
                audioSource.loop = false;
            }
            else
            {
                audioSource.loop = true;
            }

            SubBGM = audioSource;
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
        // Steam 업적 : 처음으로 SFX나 BGM을 변경 
        Managers.Steam.UnlockAchievement("ACH_SETTING_CALIBRATION");
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
        // Steam 업적 : 처음으로 SFX나 BGM을 변경 
        Managers.Steam.UnlockAchievement("ACH_SETTING_CALIBRATION");
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

    public void PlayInOptionSoundMusic(bool isPlay)
    {
        if(SubBGM==null) { Debug.Log("SubBGM 없음");  }
        if(isPlay)
        {
            Managers.Sound.Play("BGM/ESCPressed_V1", Define.Sound.SubBGM,1,2);
        }
        else
        {
            SubBGM.Stop();
        }

    }



    public void StopBGM()
    {
        if (BGM != null)
        {
            BGM.Stop();
        }
    }

    public void Stop(AudioSource audioSource)
    {
        audioSource.Stop();
        Debug.Log("Stop 실행");
    }
    public void Clear()
    {
        if (_audioSources != null)
            foreach (AudioSource audioSource in _audioSources)
            {
                if (audioSource == _audioSources[(int)Define.Sound.BGM])
                    continue;

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
        if (BGM != null&&SFX!=null)
        {
            BGM.volume = Mathf.Clamp01(BGMController.CurrentVolumeBGM);
            SFX.volume = Mathf.Clamp01(SFXController.CurrentVolumeSFX);
        }
    }

    // 1. 경로(string)로 호출하는 버전
    public void PlayScheduled(string path, double dspTime, Define.Sound type = Define.Sound.BGM, float pitch = 1.0f, float volume = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        PlayScheduled(audioClip, dspTime, type, pitch, volume);
    }

    // 2. AudioClip으로 호출하는 실제 구현부
    public void PlayScheduled(AudioClip audioClip, double dspTime, Define.Sound type = Define.Sound.BGM, float pitch = 1.0f, float volume = 1.0f)
    {
        if (audioClip == null) return;

        if (type == Define.Sound.BGM)
        {
            if (_audioSources == null) Init();
            AudioSource audioSource = _audioSources[(int)Define.Sound.BGM];

            // BGM PlayScheduled는 보통 리듬게임 스테이지 시작 시 강제로 재생하는 경우가 많으므로
            // 기존 씬 유지 로직(MainTitle <-> StageScene 공유 등)은 무시하고
            // 요청받은 곡을 해당 시간에 정확히 재생하는 것에 집중합니다.

            _currentSceneName = SceneManager.GetActiveScene().name;

            // 기존 재생 중인 곡 정지
            audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;

            // 볼륨 설정 (기존 로직 유지)
            audioSource.volume = BGMController.CurrentVolumeBGM * volume;
            _originalBGMVolume = audioSource.volume;

            // Fade 코루틴이 돌고 있었다면 정지
            if (_fadeCoroutine != null)
            {
                StaticCoroutine.StopStaticCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            // 루프 설정 (기존 로직 유지)
            if (_currentSceneName == "GamePlayScene")
            {
                audioSource.loop = false;
            }
            else
            {
                audioSource.loop = true;
            }

            BGM = audioSource;

            audioSource.PlayScheduled(dspTime);
        }
        else if (type == Define.Sound.SubBGM)
        {
            // SubBGM에 대한 PlayScheduled 로직 (필요시 구현, BGM과 유사)
            if (_audioSources == null) Init();
            AudioSource audioSource = _audioSources[(int)Define.Sound.SubBGM];

            audioSource.Stop();
            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.volume = BGMController.CurrentVolumeBGM * volume;

            SubBGM = audioSource;
            audioSource.PlayScheduled(dspTime);
        }
        else // SFX
        {
            // 주의: SFX는 보통 PlayOneShot을 쓰지만, Scheduled는 AudioSource가 필요하므로
            // PlayOneShot을 쓸 수 없습니다. 기존 SFX가 끊길 수 있습니다.
            if (SFX == null) Init();

            SFX.Stop(); // 예약 재생을 위해 기존 소리 중단
            SFX.clip = audioClip;
            SFX.pitch = pitch;
            SFX.volume = volume * SFXController.CurrentVolumeSFX;
            SFX.PlayScheduled(dspTime);
        }
    }

}
