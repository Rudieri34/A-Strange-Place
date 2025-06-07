using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController_PauseMenu : MonoBehaviour
{
    public void ResumeGame()
    {
        GameManager.Instance.IsGamePaused = false;
    }

    public void OpenOptionsMenu()
    {
        ScreenManager.Instance.ShowPopup("SettingsPopup");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
