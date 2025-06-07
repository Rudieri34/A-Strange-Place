using Cysharp.Threading.Tasks;
using InnominatumDigital.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


//languages
[System.Serializable]
public class LanguageOptions
{
    public string ENGLISH;
    public string PORTUGUESE;
}

// CardGame - Card Descriptions
[Serializable]
public class CardDesc
{
    public List<LanguageOptions> descriptions;
}

[Serializable]
public class CardDescWrapper
{
    public List<CardDesc> cards;
}


//Bar Game Dialogs
[System.Serializable]
class BarGameDialog
{
    //general
    public List<LanguageOptions> _miscPhrases;
    public List<LanguageOptions> _introPhrases;
    public List<LanguageOptions> _endGamePhrases;


    // cardGame
    public List<LanguageOptions> _firstMatchPhrases;
    public List<LanguageOptions> _secondMatchPhrases;
    public List<LanguageOptions> _thirdMatchPhrases;
    public List<LanguageOptions> _checkHandPhrases;

    //Billiard Game
    public List<LanguageOptions> _midGameTaunts;
}

[System.Serializable]
class BarGameDialogWrapper
{
    public List<BarGameDialog> BarGameDialogues;
}

public class DialogManager : SingletonBase<DialogManager>

{
    [SerializeField] string languageSel = "ENGLISH";
    [SerializeField] private TMP_Text _gameDialog;


    public string GetFilePath( string jsonFileName)
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "StreamingAssets", jsonFileName);
#else
        return Path.Combine(Application.streamingAssetsPath, jsonFileName);
#endif
    }


    public string GetLanguageText(LanguageOptions languages)
    {
        switch (languageSel.ToUpper())
        {
            case "ENGLISH":
                return languages.ENGLISH ?? "Text not available";
            case "PORTUGUESE":
                return languages.PORTUGUESE ?? "Texto no disponible";
            default:
                return "Language not supported";
        }
    }

    public async Task SetDialog(LanguageOptions language)
    {
        string dialog = GetLanguageText(language);

        _gameDialog.text = "";
        _gameDialog.gameObject.SetActive(true);

        bool skipTyping = false;

        for (int i = 0; i < dialog.Length; i++)
        {
            if (Input.anyKeyDown && i > 3)
            {
                skipTyping = true;
            }

            if (skipTyping)
            {
                _gameDialog.text = dialog;
                break;
            }

            char c = dialog[i];

            if (!char.IsWhiteSpace(c) && char.IsLetterOrDigit(c) && UnityEngine.Random.Range(0, 2) == 1)
            {
                SoundManager.Instance.PlaySFX("Type", UnityEngine.Random.Range(0.8f, 1.3f));
            }

            _gameDialog.text += c;
            await UniTask.Delay(30);
        }

        await UniTask.Delay(500);
    }
    public void HideDialog()
    {
        _gameDialog.gameObject.SetActive(false);
    }

}
