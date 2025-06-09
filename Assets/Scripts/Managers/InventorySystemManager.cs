using UnityEngine;
using System.Collections.Generic;
using InnominatumDigital.Base;
using UnityEngine.U2D;
using System;
using System.Linq;
using Unity.VisualScripting;


[Serializable]
public enum ItemUseType
{
    Heal,
    Drop,
    Throw,
    Save
}



[Serializable]
public class InventoryItemObject
{
    public string ItemName;
    public GameObject ItemPrebab;
    public Sprite ItemSpriteIcon;
}

[Serializable]
public class InventoryItem
{
    public string ItemName;
    public int Quantity;
    public int SlotPosition;
    public ItemUseType UseType;
    public string UniqueId;

    public InventoryItem(string itemName, int quantity, int slotPosition, ItemUseType useType, string uniqueId)
    {
        ItemName = itemName;
        Quantity = quantity;
        SlotPosition = slotPosition;
        UseType = useType;
        UniqueId = uniqueId;
    }
}

public class InventorySystemManager : SingletonBase<InventorySystemManager>
{
    public Action InventoryUpdated;

    public List<InventoryItemObject> CompleteInventoryItemsList;

    public List<InventoryItem> CurrentInventoryItems;

    private List<CollectableItemController> _allCollectables = new List<CollectableItemController>();

    private void Start()
    {
        _allCollectables = FindObjectsByType<CollectableItemController>(FindObjectsSortMode.None).ToList();

        CurrentInventoryItems = SaveManager.Instance.InventoryItems;
        SaveManager.Instance.LoadCollectables(_allCollectables);
    }

    private void FindFirstSlotAvailable(string itemName, int quantity, out int quantityOut, out int slotPosition)
    {
        quantityOut = quantity;
        slotPosition = -1;

        // Try to stack with an existing item
        foreach (var item in CurrentInventoryItems)
        {
            if (item.ItemName == itemName)
            {
                item.Quantity += quantity;
                quantityOut = item.Quantity;
                slotPosition = item.SlotPosition;
                Debug.Log($"Stacked {itemName}. New quantity: {item.Quantity} at slot {slotPosition}");
                return;
            }
        }

        // If item not found, find the first empty slot index
        HashSet<int> usedSlots = new HashSet<int>();
        foreach (var item in CurrentInventoryItems)
        {
            usedSlots.Add(item.SlotPosition);
        }

        int i = 0;
        while (usedSlots.Contains(i))
        {
            i++;
        }

        slotPosition = i;
        quantityOut = quantity;
    }

    public void AddItem(string itemName, int quantity, ItemUseType type, string uniqueId)
    {
        if (CurrentInventoryItems == null)
        {
            CurrentInventoryItems = new List<InventoryItem>();
        }

        FindFirstSlotAvailable(itemName, quantity, out int finalQuantity, out int slotPosition);

        if (CurrentInventoryItems.Exists(i => i.ItemName == itemName && i.SlotPosition == slotPosition))
        {
            return;
        }

        InventoryItem newItem = new InventoryItem(itemName, finalQuantity, slotPosition, type, uniqueId);
        CurrentInventoryItems.Add(newItem);
        Debug.Log($"Added new {newItem.ItemName} to inventory at slot {slotPosition}. Total items: {CurrentInventoryItems.Count}");

        InventoryUpdated?.Invoke();
    }

    public void UseItem(InventoryItem item)
    {
        if (item == null || !CurrentInventoryItems.Contains(item))
        {
            Debug.LogWarning("Cannot use item: null or not in inventory.");
            return;
        }

        switch (item.UseType)
        {
            case ItemUseType.Heal:
                HealPlayer(item);
                Spentitem(item);
                break;

            case ItemUseType.Drop:
                DropItem(item);
                Spentitem(item);
                break;

            case ItemUseType.Throw:
                ThrowItem(item);
                Spentitem(item);
                break;
            case ItemUseType.Save:
                WriteSave();
                break;
        }

        
    }

    void Spentitem(InventoryItem item)
    {
        // Reduce quantity or remove item
        item.Quantity--;
        if (item.Quantity <= 0)
        {
            CurrentInventoryItems.Remove(item);
            Debug.Log($"{item.ItemName} used up and removed from inventory.");
        }

        InventoryUpdated?.Invoke();
    }

    private void HealPlayer(InventoryItem item)
    {
        // Example healing logic
        Debug.Log($"Used {item.ItemName} to heal the player.");
        // Integrate with your health system here

        InventoryUpdated?.Invoke();
    }

    private void DropItem(InventoryItem item)
    {
        Debug.Log($"Dropped {item.ItemName} from inventory.");

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject drop = Instantiate(CompleteInventoryItemsList.Find(it => it.ItemName == item.ItemName).ItemPrebab, playerTransform.position + playerTransform.forward, Quaternion.identity);
        var controller = drop.GetComponent<CollectableItemController>();
        if (controller != null)
        {
            controller.SetUniqueId(item.UniqueId);
        }
        InventoryUpdated?.Invoke();
    }

    private void ThrowItem(InventoryItem item)
    {
        Debug.Log($"Threw {item.ItemName}.");

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject projectile = Instantiate(CompleteInventoryItemsList.Find(it => it.ItemName == item.ItemName).ItemPrebab, playerTransform.position + playerTransform.forward, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().AddForce(playerTransform.forward * (50), ForceMode.Impulse);

        var controller = projectile.GetComponent<CollectableItemController>();
        if (controller != null)
        {
            controller.SetUniqueId(item.UniqueId);
        }
        InventoryUpdated?.Invoke();
    }

    private void WriteSave()
    {
        SaveManager.Instance.PlayerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        SaveManager.Instance.InventoryItems = CurrentInventoryItems;
        List<CollectableItemData> dataList = _allCollectables
            .Select(controller => controller.GetSaveData())
            .ToList();
        SaveManager.Instance.SaveCollectables(dataList);
    }

    public void RemoveItem(InventoryItem itemToRemove)
    {
        if(itemToRemove.UseType == ItemUseType.Save)
        {
            SaveManager.Instance.ClearSaveData();
            SaveManager.Instance.ClearSaveData();
            return;
        }

        if (CurrentInventoryItems != null)
        {
            if (CurrentInventoryItems.Count > 0)
            {
                CurrentInventoryItems.Remove(itemToRemove);
                Debug.Log($"Removed {itemToRemove.ItemName} from inventory. Total items: {CurrentInventoryItems.Count}");
            }
            else
            {
                Debug.Log("No items to remove from inventory.");
            }
        }
        else
        {
            Debug.Log("Inventory is empty.");
        }

        InventoryUpdated?.Invoke();
    }



    public void ChangeItemPositionOnList(InventoryItem item, int newPos)
    {
        if (CurrentInventoryItems != null && CurrentInventoryItems.Contains(item))
        {
            item.SlotPosition = newPos;
            Debug.Log($"Moved {item.ItemName} to position {newPos}.");
        }
        else
        {
            Debug.LogWarning("Item not found in inventory or inventory is null.");
        }
    }
}