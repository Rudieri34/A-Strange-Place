using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    Transform _player;
    bool _isShowingMessage;

    void Start()
    {
        _player = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {

        ToggleMessage();

        if (Input.GetButtonDown("Interact"))
            Finish();
    }



    void ToggleMessage()
    {
        if (!IsObjectNearby())
        {
            if (!_isShowingMessage)
                return;

            ScreenManager.Instance.HideMessageText();
            _isShowingMessage = false;
        }
        else if (!ScreenManager.Instance.isShowingMessage)
        {
            ScreenManager.Instance.ShowMessageText("'E'<br>Go back to normal?");
            _isShowingMessage = true;
        }
    }
    private bool IsObjectNearby()
    {
        if (Vector3.Distance(_player.transform.position, transform.position) < 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    private async void Finish()
    {
        if (!IsObjectNearby())
            return;
        await ScreenManager.Instance.ShowDialogText("Try touching some grass.");
        await UniTask.WaitUntil(() => Input.anyKeyDown);

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
