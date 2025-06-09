using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC_DialogController : MonoBehaviour
{
    [SerializeField] private string[] dialogues;
    [SerializeField] private int a = 0;

    public bool IsShowingDialog;
    public bool IsShowingMessage;

    private Transform _player;

    void Awake()
    {
    }
    void OnEnable()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            SetDialog();
        }

        if (!IsObjectNearby())
        {
            if (IsShowingMessage)
            {
                ScreenManager.Instance.HideMessageText();
                IsShowingMessage = false;
            }
            if (IsShowingDialog)
            {
                ScreenManager.Instance.HideDialogText();
                a = 0;
                IsShowingDialog = false;
            }
        }
        else if (!ScreenManager.Instance.isShowingMessage)
        {
            ScreenManager.Instance.ShowMessageText("'E'<br>Talk");
            IsShowingMessage = true;
        }

    }

    private bool IsObjectNearby()
    {

        if (Vector3.Distance(_player.transform.position, transform.position) < 3)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    void SetDialog()
    {
        transform.LookAt(_player);
        if (IsObjectNearby())
        {
            if (a >= dialogues.Length)
            {
                ScreenManager.Instance.HideDialogText();
                a = -1;
                IsShowingDialog = false;
            }

            if (a >= 0)
            {
                _= ScreenManager.Instance.ShowDialogText(dialogues[a]);
                IsShowingDialog = true;
            }

            a++;
        }
    }

}
