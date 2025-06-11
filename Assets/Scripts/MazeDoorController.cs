using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeDoorController : MonoBehaviour
{
    [SerializeField] private Transform _doorR, _doorL;
    [SerializeField] private string _keyItemUniqueID;

    Transform _player;
    bool _isShowingMessage;

    void Start()
    {
        _player = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        //only if has gift
        if (InventorySystemManager.Instance.CurrentInventoryItems.Any(I => I.UniqueId == _keyItemUniqueID))
        {
            ToggleMessage();

            if (Input.GetButtonDown("Interact"))
                UseKey();

        }
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
            ScreenManager.Instance.ShowMessageText("'E'<br>Use Key");
            _isShowingMessage = true;
        }
    }
    private bool IsObjectNearby()
    {
        if (Vector3.Distance(_player.transform.position, transform.position) < 5F)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    private void UseKey()
    {
        if (!IsObjectNearby())
            return;

        var manager = InventorySystemManager.Instance;

        if (manager.CurrentInventoryItems == null)
        {
            manager.CurrentInventoryItems = new List<InventoryItem>();
        }

        manager.RemoveItem(InventorySystemManager.Instance.CurrentInventoryItems.Find(I => I.UniqueId == _keyItemUniqueID));
        _doorR.DOLocalRotate(new Vector3(0, 85, 0), .5f);
        _doorL.DOLocalRotate(new Vector3(0, -85, 0), .5f);

        if (_isShowingMessage)
        {
            ScreenManager.Instance.HideMessageText();
            _isShowingMessage = false;
        }
    }

}
