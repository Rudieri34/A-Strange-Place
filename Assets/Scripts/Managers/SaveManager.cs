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
        return true;
    }

    private bool FileExists() => File.Exists(_filePath);

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
}

[Serializable]
public class GameData
{
    public int languageOption;
   

    public GameData()
    {
        languageOption = 0;
       
    }
}
