using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioService : MonoBehaviour
{
    public static AudioService Instance { get; private set; }

    [SerializeField] private List<AudioData> audioList;

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource.loop = true;
        sfxSource.loop = false;
    }

    // -------------------------------------------------------
    // BGM
    // -------------------------------------------------------

    public void PlayBGM(AudioType audioType)
    {
        AudioData audioData = audioList.Find(x => x.audioType == audioType);

        if (audioData == null)
        {
            Debug.LogError($"BGM Audio data not found for type: {audioType}");
            return;
        }

        if (bgmSource.clip == audioData.audioClip && bgmSource.isPlaying)
            return; // Already playing same BGM

        bgmSource.clip = audioData.audioClip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // -------------------------------------------------------
    // SFX
    // -------------------------------------------------------

    public void PlaySFX(AudioType audioType)
    {
        AudioData audioData = audioList.Find(x => x.audioType == audioType);

        if (audioData == null)
        {
            Debug.LogError($"SFX Audio data not found for type: {audioType}");
            return;
        }

        sfxSource.PlayOneShot(audioData.audioClip);
    }

    // -------------------------------------------------------
    // Volume
    // -------------------------------------------------------

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void SetVolume(float bgmVolume, float sfxVolume)
    {
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }

    // -------------------------------------------------------
    // Mute
    // -------------------------------------------------------

    public void MuteBGM(bool mute)  => bgmSource.mute = mute;
    public void MuteSFX(bool mute)  => sfxSource.mute = mute;
    public void MuteAll(bool mute)
    {
        MuteBGM(mute);
        MuteSFX(mute);
    }
}

[Serializable]
public class AudioData
{
    public AudioType audioType;
    public AudioClip audioClip;
}

public enum AudioType
{
    BGM_1,
    ButtonClick,
    GameOver,
    BombMerge,
    TileMerge,
    TilePlace,
    TileUpgrade,
}