using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioData", menuName = "Audio/Audio Data")]
public class AudioDataSO : ScriptableObject
{
    [SerializeField] private AudioType _audioType;
    [SerializeField] private AudioFile[] _audioList;

    public AudioFile[] GetAudioList()
    {
        return _audioList;
    }

    public AudioFile GetAudioFile(string audioTag)
    {
        foreach(AudioFile audioFile in _audioList)
        {
            if (audioFile.audioName == audioTag)
            {
                return audioFile;
            }
        }
        return default;
    }
}
