using InnominatumDigital.Base;
using System;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    public static Action<bool> OnPauseStateChanged;
    public static Action<bool> OnPlayerInputStateChanged;

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
            IsCursorVisible = IsCursorVisible;
            OnPauseStateChanged?.Invoke(value);

            if (_isGamePaused)
            {
                SoundManager.Instance.PauseAudioLoop();
                Time.timeScale = 0;
                ScreenManager.Instance.ShowPopup("PauseMenuPopup");
            }
            else
            {
                SoundManager.Instance.ResumeAudioLoop();
                Time.timeScale = 1;
                ScreenManager.Instance.HidePopup("PauseMenuPopup");
            }
        }
    }


    public bool IsInventoryOpen
    {
        get
        {
            return _isInventoryOpen;
        }
        set
        {
            _isInventoryOpen = value;
            IsCursorVisible = IsCursorVisible;
            IsPlayerInputEnabled = !value;

            if (_isInventoryOpen)
            {
                ScreenManager.Instance.ShowPopup("InventoryPopup");
            }
            else
            {
                ScreenManager.Instance.HidePopup("InventoryPopup");
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
            _isPlayerInputEnabled = (IsInventoryOpen || IsGamePaused) ? false : value;
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
            Cursor.visible = IsInventoryOpen || IsGamePaused;
        }
    }

    private bool _isPauseAllowed;
    private bool _isGamePaused;
    private bool _isPlayerInputEnabled;
    private bool _isInventoryOpen;

    private void Start()
    {
        IsPlayerInputEnabled = true;
        IsGamePaused = false;
        _isPauseAllowed = true;
        IsCursorVisible = IsCursorVisible;


        SoundManager.Instance.PlayAudioLoop("MainTheme");
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            TogglePause();

        if (Input.GetButtonDown("Inventory"))
            ToggleInventory();

        if (Input.GetKeyDown(KeyCode.P))
            SaveManager.Instance.ClearSaveData();
    }
    public void ToggleInventory() => IsInventoryOpen = !IsInventoryOpen;
    public void TogglePause() => IsGamePaused = !IsGamePaused;
    public void ToggleIsCursorVisible() => IsCursorVisible = !IsCursorVisible;

}
