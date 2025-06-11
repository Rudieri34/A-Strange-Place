using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    public void PlaySfx(string s)
    {
        SoundManager.Instance.PlaySFX(s, Random.Range(0.9f, 1.1f));
    }


}
