using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;
    public List<InventorySlot> slots = new List<InventorySlot>(20);

    void Awake()
    {
        if (Instance == null) Instance = this;
        for (int i = 0; i < 20; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    public void AddItem(InventoryItem item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item && item.isStackable)
            {
                slot.AddItem(item);
                return;
            }
        }

        foreach (InventorySlot slot in slots)
        {
            if (slot.item == null)
            {
                slot.AddItem(item);
                return;
            }
        }
    }
}