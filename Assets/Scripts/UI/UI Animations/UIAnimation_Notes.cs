using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimation_Notes : UIAnimation
{
    [SerializeField] private Image _blackFog;
    [SerializeField] private Image _transparentBackground;
    [SerializeField] private TextMeshProUGUI _noteText;
    [SerializeField] private TextMeshProUGUI _noteTitle;

    public override async void PlayOpenAnimation()
    {
        _blackFog.color = new Color(_blackFog.color.r, _blackFog.color.g, _blackFog.color.b, 0);
        _transparentBackground.color = new Color(_transparentBackground.color.r, _transparentBackground.color.g, _transparentBackground.color.b, 0);
        await _transparentBackground.DOFade(0.7f, 0.1f).AsyncWaitForCompletion();
        await _blackFog.DOFade(1, 0.3f).AsyncWaitForCompletion();
        _noteTitle.DOFade(1, 0.1f);
        _noteText.DOFade(1, 0.3f);
    }

    public override async void PlayCloseAnimation()
    {
        _noteTitle.DOFade(0, 0.3f);
        _noteText.DOFade(0, 0.1f);
        await _blackFog.DOFade(0, 0.3f).AsyncWaitForCompletion();
        await _transparentBackground.DOFade(0, 0.1f).AsyncWaitForCompletion();
        gameObject.SetActive(false);
    }
}
