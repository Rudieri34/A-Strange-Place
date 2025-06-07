using InnominatumDigital.Base;
using System;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    public GameObject[] BarGamesPrefabs;
    public static Action<bool> OnPauseStateChanged;
    public static Action<bool> OnPlayerInputStateChanged;
    public static Action<GameControllerTypes> OnGameControllerTypeChanged;

    public bool IsPauseAllowed
    {
        get
        {
            return _isPauseAllowed;
        }
        set
        {
            _isPauseAllowed = value;
        }
    }

    public bool IsGamePaused
    {
        get
        {
            return _isGamePaused;
        }
        set
        {
            if (!IsPauseAllowed)
                return;

            _isGamePaused = value;
            IsPlayerInputEnabled = !value;
            IsCursorVisible = value;
            OnPauseStateChanged?.Invoke(value);

            if (_isGamePaused)
            {
                ScreenManager.Instance.ShowPopup("PauseMenuPopup", hasOpeningAnimation: true);
            }
            else
            {
                ScreenManager.Instance.HidePopup("PauseMenuPopup", hasClosingAnimation: true);
            }
        }
    }

    public bool IsPlayerInputEnabled
    {
        get
        {
            return _isPlayerInputEnabled;
        }
        set
        {
            _isPlayerInputEnabled = value;
            OnPlayerInputStateChanged?.Invoke(_isPlayerInputEnabled);
        }
    }

    public bool IsCursorVisible
    {
        get
        {
            return Cursor.visible;
        }
        set
        {
            Cursor.visible = value;
        }
    }

    public GameControllerTypes CurrentGameController
    {
        get
        {
            return _currentGameController;
        }
        set
        {
            _currentGameController = value;
            OnGameControllerTypeChanged?.Invoke(value);
        }
    }

    private bool _isPauseAllowed;
    private bool _isGamePaused;
    private bool _isPlayerInputEnabled;
    private GameControllerTypes _currentGameController;

    private void Start()
    {
        IsPlayerInputEnabled = true;
        IsGamePaused = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            TogglePause();

        if (Input.GetButtonDown("Objective"))
            ObjectivesManager.Instance.ShowObjectivePopup();
    }

    public void TogglePause() => IsGamePaused = !IsGamePaused;
    public void TogglePlayerInput() => IsPlayerInputEnabled = !IsPlayerInputEnabled;
    public void ToggleIsCursorVisible() => IsCursorVisible = !IsCursorVisible;

    public void RestartBarGame(GameControllerTypes _barGame, GameObject Instance)
    {
        DialogManager.Instance.HideDialog();
        SoundManager.Instance.StopSFX();
        SoundManager.Instance.StopAudioLoop();

        Transform parent = Instance.transform.parent;

        if (Instance != null)
            Destroy(Instance);

        GameObject newInstance = Instantiate(BarGamesPrefabs[(int)_barGame], parent);
        newInstance.SetActive(true);
    }
}

public enum GameControllerTypes
{
    Cards,
    Billiard,
    Platformer,
    TopDown,
    FirstPerson,
    PointAndClick,
    Car
}