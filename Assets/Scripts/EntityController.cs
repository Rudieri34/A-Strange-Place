using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class EntityController : MonoBehaviour
{
    [SerializeField] private GameObject _giftReward;
    [SerializeField] private string _giftItemUniqueID;

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
        if (InventorySystemManager.Instance.CurrentInventoryItems.Any(I => I.UniqueId == _giftItemUniqueID))
        {
            ToggleMessage();

            if (Input.GetButtonDown("Interact"))
                OfferGift();

        }

        transform.LookAt(_player.position);
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
            ScreenManager.Instance.ShowMessageText("'E'<br>Offer Gift");
            _isShowingMessage = true;
        }
    }
    private bool IsObjectNearby()
    {

        if (Vector3.Distance(_player.transform.position, transform.position) < 3F)
        {
            return true;
        }
        else
        {
            return false;
        }

    }



    private void OfferGift()
    {
        if (!IsObjectNearby())
            return;

        var manager = InventorySystemManager.Instance;

        if (manager.CurrentInventoryItems == null)
        {
            manager.CurrentInventoryItems = new List<InventoryItem>();
        }

        manager.RemoveItem(InventorySystemManager.Instance.CurrentInventoryItems.Find(I => I.UniqueId == _giftItemUniqueID));
        _giftReward.transform.position = transform.position;

        if (_isShowingMessage)
        {
            ScreenManager.Instance.HideMessageText();
            _isShowingMessage = false;
        }
    }

}
