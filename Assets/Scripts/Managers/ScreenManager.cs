using InnominatumDigital.Base;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenManager : SingletonBase<ScreenManager>
{
    public bool isShowingMessage;

    [Header("Popups")]
    public List<PopupConfig> PopupsConfigs;
    public List<KeyValuePair<string, GameObject>> OpenedPopups = new List<KeyValuePair<string, GameObject>>();
    public string currentOpenPopup;

    [Header("Scenes")]
    public List<SceneConfig> ScenesConfigs;

    [Header("Dialogue Screen")]
    [SerializeField] private GameObject _dialogScreen;
    [SerializeField] private TMP_Text _dialogText;

    [Header("Message Screen")]
    [SerializeField] private GameObject _messageScreen;
    [SerializeField] private TMP_Text _messageText;


    [Header("Fade")]
    [SerializeField] private Image _fade;

    private string _popupToOpen;
    private bool _isDialogOngoing;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        currentOpenPopup = string.Empty;
    }

    public GameObject ShowPopup(string name)
    {
        if (OpenedPopups.FirstOrDefault(s => s.Key == name).Value != null)
            return null;

        GameObject newPopup = null;

        newPopup = Instantiate(PopupsConfigs.First(s => s.PrefabName == name).Prefab);

        OpenedPopups.Add(new KeyValuePair<string, GameObject>(name, newPopup));

        currentOpenPopup = name;
        return newPopup;
    }

    public void HidePopup(string name)
    {
        if (OpenedPopups.FirstOrDefault(s => s.Key == name).Value == null)
            return;

        Destroy(OpenedPopups.First(s => s.Key == name).Value);

        OpenedPopups.Remove(OpenedPopups.First(s => s.Key == name));
        currentOpenPopup = string.Empty;
    }

    public void ChangeScene(string name, string openOnChange = "")
    {
        ShowPopup("LoadingPopup");
        SceneManager.LoadSceneAsync(name);

        _popupToOpen = openOnChange;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HidePopup("LoadingPopup");
        if (!String.IsNullOrEmpty(_popupToOpen))
        {
            ShowPopup(_popupToOpen);
            _popupToOpen = "";
        }

    }

    #region Dialogue
    public async Task ShowDialogText(string dialog)
    {
        _dialogText.text = "";
        _dialogScreen.SetActive(true);
        await SetDialogText(dialog);
    }

    public void HideDialogText()
    {
        _dialogText.text = "";
        _dialogScreen.SetActive(false);
    }

    async Task SetDialogText(string fullText)
    {
        if (_isDialogOngoing)
        {
            _isDialogOngoing = false;
            _dialogText.text = fullText;
        }
        else
        {
            string currentText = "";
            _isDialogOngoing = true;
            for (int i = 0; i <= fullText.Length; i++)
            {
                if (!_isDialogOngoing)
                    return;
                if (Random.Range(0, 3) == 1)
                    SoundManager.Instance.PlaySFX("Type", Random.Range(0.8f, 1.3f));

                currentText = fullText.Substring(0, i);
                _dialogText.text = currentText;
                await UniTask.Delay(2);
            }
        }

        _isDialogOngoing = false;
    }
    #endregion

    #region Message
    public void ShowMessageText(string dialog)
    {
        isShowingMessage = true;
        _messageScreen.SetActive(true);
        SetMessageText(dialog);
    }

    async void SetMessageText(string fullText)
    {
        SoundManager.Instance.PlaySFX("Type", Random.Range(0.8f, 1.3f));

        string currentText = "";
        for (int i = 0; i <= fullText.Length; i++)
        {
            if (!isShowingMessage)
                return;

            currentText = fullText.Substring(0, i);
            _messageText.text = currentText;
            await UniTask.Delay(2);
        }

    }

    public void HideMessageText()
    {
        Debug.Log("Hiding message text");
        _messageScreen.SetActive(false);
        _messageText.text = "";
        isShowingMessage = false;
    }
    #endregion

}

[Serializable]
public class PopupConfig
{
    public string PrefabName;
    public GameObject Prefab;
}

[Serializable]
public class SceneConfig
{
    public string SceneName;
    public GameObject Prefab;
}
