using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimation_CurrentObjectivePopup : UIAnimation
{
    //[SerializeField] private Image _blackFog;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    public override async void PlayOpenAnimation()
    {
        //_blackFog.color = new Color(_blackFog.color.r, _blackFog.color.g, _blackFog.color.b, 0);
        _descriptionText.color = new Color(_descriptionText.color.r, _descriptionText.color.g, _descriptionText.color.b, 0);
        _titleText.color = new Color(_descriptionText.color.r, _descriptionText.color.g, _descriptionText.color.b, 0);
        //await _blackFog.DOFade(1, 0.3f).AsyncWaitForCompletion();
        await _titleText.DOFade(1, 0.3f).AsyncWaitForCompletion();
        await _descriptionText.DOFade(1, 0.3f).AsyncWaitForCompletion();
    }

    public override async void PlayCloseAnimation()
    {
        await _descriptionText.DOFade(0, 0.3f).AsyncWaitForCompletion();
        await _titleText.DOFade(0, 0.3f).AsyncWaitForCompletion();
        //await _blackFog.DOFade(0, 0.3f).AsyncWaitForCompletion();
    }
}
