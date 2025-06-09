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
    public Vector3 PlayerPosition
    {
        get { LoadGame(); return _gameData.playerPosition; }
        set { _gameData.playerPosition = value; SaveGame(); }
    }
    public List<InventoryItem> InventoryItems
    {
        get { LoadGame(); return _gameData.inventoryItems; }
        set { _gameData.inventoryItems = value; SaveGame(); }
    }

    public void SaveCollectables(List<CollectableItemData> collectables)
    {
        if (_gameData == null) LoadGame();

        _gameData.collectables.Clear();
        foreach (var item in collectables)
        {
            _gameData.collectables.Add(item);
        }

        SaveGame();
    }

    public void LoadCollectables(List<CollectableItemController> collectables)
    {
        if (_gameData == null || _gameData.collectables == null) return;

        foreach (var item in collectables)
        {
            var saved = _gameData.collectables.Find(c => c.UniqueId == item.UniqueId);
            if (saved != null)
            {
                item.LoadFromSaveData(saved);
            }
        }
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
        return FileExists();
    }

    private bool FileExists() => File.Exists(_filePath);

    public void ClearSaveData()
    {
        if (FileExists())
        {
            File.Delete(_filePath);
            _gameData = new GameData();
            SaveUpdated?.Invoke();
            Debug.LogWarning("Save Data Cleared");

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
    public Vector3 playerPosition;
    public List<InventoryItem> inventoryItems;
    public List<CollectableItemData> collectables;

    public GameData()
    {
        languageOption = 0;
        playerPosition = Vector3.zero;
        inventoryItems = new List<InventoryItem>();
        collectables = new List<CollectableItemData>();
    }
}
