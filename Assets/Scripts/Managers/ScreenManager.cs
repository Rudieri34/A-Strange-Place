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

    [Header("Notes Screen")]
    [SerializeField] private GameObject _noteParentObject;
    [SerializeField] private TextMeshProUGUI _noteTitleTMP;
    [SerializeField] private TextMeshProUGUI _noteContextTMP;
    [SerializeField] private TextMeshProUGUI _notePagesTMP;
    [SerializeField] private int _currentNotePage;
    [SerializeField] private int _maxNotePage;

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

    public GameObject ShowPopup(string name, bool openMultiple = false, bool hasOpeningAnimation = false, Action callingMethod = null, GameObject popupGameObject = null)
    {
        if (OpenedPopups.FirstOrDefault(s => s.Key == name).Value != null && !openMultiple)
            return null;

        GameObject newPopup = null;

        if (!popupGameObject)
        {
            newPopup = Instantiate(PopupsConfigs.First(s => s.PrefabName == name).Prefab);
        }
        else
        {
            newPopup = popupGameObject;
            newPopup.SetActive(true);
        }

        OpenedPopups.Add(new KeyValuePair<string, GameObject>(name, newPopup));

        if (hasOpeningAnimation)
            if (newPopup.TryGetComponent(out UIAnimation uiAnimation))
                uiAnimation.PlayOpenAnimation();

        callingMethod?.Invoke();
        currentOpenPopup = name;
        return newPopup;
    }

    public void HidePopup(string name, bool hasClosingAnimation = false, Action callingMethod = null, bool shouldDestroyGameObject = true)
    {
        if (OpenedPopups.FirstOrDefault(s => s.Key == name).Value == null)
            return;

        if (hasClosingAnimation)
        {
            if (OpenedPopups.FirstOrDefault(s => s.Key == name).Value.GetComponent<UIAnimation>())
                OpenedPopups.FirstOrDefault(s => s.Key == name).Value.GetComponent<UIAnimation>().PlayCloseAnimation();
        }
        else
        {
            if(shouldDestroyGameObject)
                Destroy(OpenedPopups.First(s => s.Key == name).Value);
        }

        callingMethod?.Invoke();

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

    #region Notes
    public void ShowNewNote(string noteTitle, string noteContent)
    {
        GameManager.Instance.IsCursorVisible = true;
        GameManager.Instance.IsPlayerInputEnabled = false;
        GameManager.Instance.IsPauseAllowed = false;
        ShowPopup("Notes", hasOpeningAnimation: true, popupGameObject: _noteParentObject);
        _noteTitleTMP.text = noteTitle;
        _noteContextTMP.text = noteContent;
        _maxNotePage = Mathf.Max(1, _noteContextTMP.textInfo.pageCount);
        _currentNotePage = 1;
        _noteContextTMP.pageToDisplay = _currentNotePage;
        _notePagesTMP.text = $"{_currentNotePage}/{_maxNotePage}";
        _notePagesTMP.ForceMeshUpdate();
        _noteContextTMP.ForceMeshUpdate();
    }

    public void NextNotePage()
    {
        if (_currentNotePage == _maxNotePage)
            return;
        _currentNotePage++;
        _noteContextTMP.pageToDisplay = _currentNotePage;
        _notePagesTMP.text = $"{_currentNotePage}/{_maxNotePage}";
    }

    public void PreviousNotePage()
    {
        if (_currentNotePage == 1)
            return;
        _currentNotePage--;
        _noteContextTMP.pageToDisplay = _currentNotePage;
        _notePagesTMP.text = $"{_currentNotePage}/{_maxNotePage}";
    }

    public void CloseNotePopup()
    {
        GameManager.Instance.IsPauseAllowed = true;
        GameManager.Instance.IsPlayerInputEnabled = true;
        GameManager.Instance.IsCursorVisible = false;
        HidePopup("Notes", hasClosingAnimation: true, shouldDestroyGameObject: false);
    }
    #endregion

    public async Task LoadSceneWithFade(string sceneName, float fadeInTime, float fadeOutTime)
    {
        await DoFade(0, 1, fadeInTime);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        var taskCompletionSource = new TaskCompletionSource<bool>();
        asyncLoad.completed += _ => taskCompletionSource.SetResult(true);
        await taskCompletionSource.Task;
        await DoFade(1, 0, fadeOutTime);
        await Task.Delay(100);
    }

    public async Task DoFade(float startValue, float endValue, float duration)
    {
        _fade.color = new Color(_fade.color.r, _fade.color.g, _fade.color.b, startValue);
        await _fade.DOFade(endValue, duration).AsyncWaitForCompletion();
    }
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
