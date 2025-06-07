using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController_MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _continueButton;
    private void Start()
    {
        GameManager.Instance.IsPauseAllowed = false;
        _versionText.text = Application.version;
        _continueButton.SetActive(SaveManager.Instance.CheckIfSaveGameExists());
    }

    [SerializeField] private TextMeshProUGUI _versionText;
    public async void NewGame()
    {
        SaveManager.Instance.ResetObjectivesAndClues();
        await ScreenManager.Instance.LoadSceneWithFade("OfficeIntro", 0.4f, 0.4f);
    }

    public async void ContinueGame()
    {
        await ScreenManager.Instance.LoadSceneWithFade("Sandbox", 0.4f, 0.4f);
    }

    public void OpenOptions()
    {
        ScreenManager.Instance.ShowPopup("SettingsPopup");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
