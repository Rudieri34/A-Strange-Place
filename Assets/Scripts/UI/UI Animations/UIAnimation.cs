using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class UIAnimation : MonoBehaviour
{
    public virtual async void PlayOpenAnimation()
    {
        await Task.Delay(500);
    }
    public virtual async void PlayCloseAnimation()
    {
        await Task.Delay(500);
    }
}
    