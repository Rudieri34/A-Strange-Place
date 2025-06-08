using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using InnominatumDigital.Base;
using Unity.VisualScripting;

public class SoundManager : SingletonBase<SoundManager>
{
    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioDataSO _sfxAudioData;
    [HorizontalLine(color: EColor.Gray)]

    [Header("Music")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioDataSO _musicAudioData;
    private List<AudioFile> _musicPlaylist = new();

    private bool _isSequencing;

    [HorizontalLine(color: EColor.Gray)]

    private GameObject _player;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        AddAllMusicToPlaylist();
    }

    private void Update()
    {
        if (_isSequencing && _musicSource.isPlaying == false)
        {
            PlayNextSong(_musicSource.clip);
        }
    }

    private void OnDisable()
    {
        sfxSource.Stop();
    }

    private void OnApplicationQuit()
    {
        sfxSource.Stop();
    }

    public void PlaySFX(string audioName, float pitch = 1, float volume = 1)
    {
        if (!_sfxAudioData)
        {
            Debug.LogError($"The SFX audio data was not set.");
            return;
        }

        AudioFile sfx = _sfxAudioData.GetAudioFile(audioName);

        if (!sfx.audio)
        {
            Debug.LogError($"The audio {audioName} was not found on the SFX audio data (check the scriptable object).");
            return;
        }

        sfxSource.pitch = sfx.pitch * pitch;
        sfxSource.volume = sfx.volume * volume;
        sfxSource.PlayOneShot(sfx.audio);
    }

    public void Play3DAudio(AudioClip clip, AudioSource audioSource)
    {
        audioSource.PlayOneShot(clip);
    }

    public void PlayAudioLoop(string audioName, bool startSequencing = false)
    {
        AudioFile loop = _musicAudioData.GetAudioFile(audioName);

        _isSequencing = startSequencing;
        _musicSource.Stop();

        if (_isSequencing)
            _musicSource.loop = false;
        else
            _musicSource.loop = true;

        _musicSource.clip = loop.audio;
        _musicSource.Play();
    }

    public void StopAudioLoop()
    {
        _isSequencing = false;
        _musicSource.Stop();
        _musicSource.loop = false;
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void PlayNextSong(AudioClip current)
    {
        int currentIndex = _musicPlaylist.FindIndex(a => a.audio == current);

        AudioFile next = new AudioFile();
        if (currentIndex + 1 >= _musicPlaylist.Count)
            next = _musicPlaylist[0];
        else
            next = _musicPlaylist[currentIndex + 1];

        _musicSource.clip = next.audio;
        _musicSource.Play();
    }

    public void AddAllMusicToPlaylist()
    {
        _musicPlaylist.Clear();
        _musicPlaylist = _musicAudioData.GetAudioList().ToList();
    }
}

public enum AudioType
{
    Music,
    Ambience,
    SFX,
    Dialogue
}

[System.Serializable]
public struct AudioFile
{
    public string audioName;
    public AudioClip audio;
    [Range(0.1f, 3f)] public float pitch;
    [Range(0f, 1f)] public float volume;
}


