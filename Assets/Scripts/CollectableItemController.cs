using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]


[Serializable]
public class CollectableItemData
{
    public string UniqueId;
    public bool IsInInventory;
    public Vector3 WorldPosition;
}
public class CollectableItemController : MonoBehaviour
{
    private Transform _player;

    public string ItemName = "Default Item";
    public int Quantity = 1;
    public ItemUseType UseType = ItemUseType.Drop;
    public bool IsShowingMessage;

    public string UniqueId;

    public bool IsInInventory;


    Vector3 _lastPosition;


    public void SetUniqueId(string id) => UniqueId = id;
    public CollectableItemData GetSaveData()
    {
        return new CollectableItemData
        {
            UniqueId = UniqueId,
            IsInInventory = IsInInventory,
            WorldPosition = _lastPosition
        };
    }

    public void LoadFromSaveData(CollectableItemData data)
    {
        IsInInventory = data.IsInInventory;
        transform.position = data.WorldPosition;

        gameObject.SetActive(!IsInInventory);
    }

    void OnEnable()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        ToggleMessage();

        if (transform.position != _lastPosition)
            _lastPosition = transform.position;

        if (Input.GetButtonDown("Interact"))
        {
            CollectTitem();
        }
    }

    private void CollectTitem()
    {
        if (!IsObjectNearby())
            return;

        var manager = InventorySystemManager.Instance;

        if (manager.CurrentInventoryItems == null)
        {
            manager.CurrentInventoryItems = new List<InventoryItem>();
        }

        manager.AddItem(ItemName, Quantity, UseType, UniqueId);
        IsInInventory = true;
        Destroy(gameObject);
    }



    void ToggleMessage()
    {
        if (!IsObjectNearby())
        {
            if (!IsShowingMessage)
                return;

            ScreenManager.Instance.HideMessageText();
            IsShowingMessage = false;
        }
        else if (!ScreenManager.Instance.isShowingMessage)
        {
            ScreenManager.Instance.ShowMessageText("'E'<br>Collect Item");
            IsShowingMessage = true;
        }
    }

    private bool IsObjectNearby()
    {

        if (Vector3.Distance(_player.transform.position, transform.position) < 1.8F)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private void OnDestroy()
    {
        if (IsShowingMessage)
        {
            ScreenManager.Instance.HideMessageText();
            IsShowingMessage = false;
        }
    }
}
