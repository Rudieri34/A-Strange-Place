using InnominatumDigital.Base;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class SaveManager : SingletonBase<SaveManager>
{
    private string _filePath;
    private GameData _gameData;
    [HideInInspector] public UnityEvent SaveUpdated;

    public int LanguageOption
    {
        get { LoadGame(); return _gameData.languageOption; }
        set { _gameData.languageOption = value; SaveGame(); }
    }
    public int QualityOption
    {
        get { LoadGame(); return _gameData.qualityOption; }
        set { _gameData.qualityOption = value; SaveGame(); }
    }
    public bool Fullscreen
    {
        get { LoadGame(); return _gameData.fullscreen; }
        set { _gameData.fullscreen = value; SaveGame(); }
    }
    public float BGMVolume
    {
        get { LoadGame(); return _gameData.bgmVolume; }
        set { _gameData.bgmVolume = value; SaveGame(); }
    }
    public float SFXVolume
    {
        get { LoadGame(); return _gameData.sfxVolume; }
        set { _gameData.sfxVolume = value; SaveGame(); }
    }

    public List<string> UnlockedClueIds
    {
        get { LoadGame(); return _gameData.unlockedClueIds; }
        set { _gameData.unlockedClueIds = value; SaveGame(); }
    }

    public List<string> PendingClueIds
    {
        get { LoadGame(); return _gameData.pendingClueIds; }
        set { _gameData.pendingClueIds = value; SaveGame(); }
    }

    public List<ObjectiveTag> CompletedObjectives
    {
        get { LoadGame(); return _gameData.completedObjectives; }
        set { _gameData.completedObjectives = value; SaveGame(); }
    }

    public ObjectiveTag CurrentActiveObjective
    {
        get { LoadGame(); return _gameData.currentActiveObjective; }
        set { _gameData.currentActiveObjective = value; SaveGame(); }
    }

    protected override void Awake()
    {
        base.Awake();
        _filePath = Application.persistentDataPath + "/save.json";
        LoadGame();
    }

    public void SaveGame()
    {
        try
        {
            string json = JsonUtility.ToJson(_gameData, true);
            File.WriteAllText(_filePath, json);
            SaveUpdated?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save game: " + e.Message);
        }
    }

    public void LoadGame()
    {
        if (FileExists())
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                _gameData = JsonUtility.FromJson<GameData>(json) ?? new GameData();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load game: " + e.Message);
                _gameData = new GameData();
            }
        }
        else
        {
            _gameData = new GameData();
        }
    }

    public bool CheckIfSaveGameExists()
    {
        return CompletedObjectives.Count != 0;
    }

    private bool FileExists() => File.Exists(_filePath);

    public void ResetObjectivesAndClues()
    {
        CompletedObjectives = new List<ObjectiveTag>();
        CurrentActiveObjective = ObjectiveTag.No_Objective;
        ObjectivesManager.Instance.SetNewActiveObjective(ObjectiveTag.No_Objective, false);
    }

    public void ClearSaveData()
    {
        if (FileExists())
        {
            File.Delete(_filePath);
            _gameData = new GameData();
            SaveUpdated?.Invoke();
        }
    }

    public void OpenSaveFolder()
    {
#if UNITY_EDITOR
        EditorUtility.RevealInFinder(Application.persistentDataPath);
#endif
    }

    public void AddUnlockedClueId(string clueId)
    {
        if (!_gameData.unlockedClueIds.Contains(clueId))
        {
            _gameData.unlockedClueIds.Add(clueId);
            SaveGame();
        }
    }

    public void AddPendingClueId(string clueId)
    {
        if (!_gameData.pendingClueIds.Contains(clueId))
        {
            _gameData.pendingClueIds.Add(clueId);
            SaveGame();
        }
    }

    public void RemovePendingClueId(string clueId)
    {
        if (_gameData.pendingClueIds.Contains(clueId))
        {
            _gameData.pendingClueIds.Remove(clueId);
            SaveGame();
        }
    }

    public void AddNewCompletedObjective(ObjectiveTag value)
    {
        if (!_gameData.completedObjectives.Contains(value))
        {
            _gameData.completedObjectives.Add(value);
            SaveGame();
        }
    }
}

[Serializable]
public class GameData
{
    public int languageOption;
    public int qualityOption;
    public bool fullscreen;
    public float bgmVolume;
    public float sfxVolume;
    public List<string> unlockedClueIds = new List<string>();
    public List<string> pendingClueIds = new List<string>();
    public List<ObjectiveTag> completedObjectives;
    public ObjectiveTag currentActiveObjective;

    public GameData()
    {
        languageOption = 0;
        qualityOption = 0;
        fullscreen = true;
        bgmVolume = 100;
        sfxVolume = 100;
        unlockedClueIds = new List<string>();
        pendingClueIds = new List<string>();
        completedObjectives = new List<ObjectiveTag>();
        currentActiveObjective = ObjectiveTag.No_Objective;
    }
}
