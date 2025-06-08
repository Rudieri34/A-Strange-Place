using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC_DialogController : MonoBehaviour
{
    public string[] dialogues;
    public int a = 0;

    public bool _isShowingDialog;
    public bool _isShowingMessage;

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

        if (!IsObjectOnScreen() )
        {
            if (_isShowingMessage)
            {
                ScreenManager.Instance.HideMessageText();
                _isShowingMessage = false;
            }
            if (_isShowingDialog)
            {
                ScreenManager.Instance.HideDialogText();
                a = 0;
                _isShowingDialog = false;
            }
        }
        else if (!ScreenManager.Instance.isShowingMessage)
        {
            ScreenManager.Instance.ShowMessageText("'E'<br>Talk");
            _isShowingMessage = true;
        }

    }

    private bool IsObjectOnScreen()
    {
        // Calculate the middle of the screen
        Vector3 middleScreenPoint = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        // Convert the screen point to a ray
        Ray ray = Camera.main.ScreenPointToRay(middleScreenPoint);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, 3))
        {
            // Check if the hit object is valid
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                return true;
            }
            return false;

        }
        else
        {
            return false;
        }

    }

    void SetDialog()
    {
        transform.LookAt(_player);
        if (IsObjectOnScreen())
        {
            if (a >= dialogues.Length)
            {
                ScreenManager.Instance.HideDialogText();
                a = -1;
                _isShowingDialog = false;
            }

            if (a >= 0)
            {
                ScreenManager.Instance.ShowDialogText(dialogues[a]);
                _isShowingDialog = true;
            }

            a++;
        }
    }

}
