using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightColorController : MonoBehaviour
{

    [SerializeField] private Color[] _colors;
    [SerializeField] private Light _lightSource;

    // Start is called before the first frame update
    void Start()
    {
        ChangeColor();
    }

    void ChangeColor()
    {
        _lightSource.DOColor(_colors[Random.Range(0, _colors.Length)], 1f).OnComplete(() =>
        {
            ChangeColor();
        });
    }
}
