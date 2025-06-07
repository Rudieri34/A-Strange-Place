using System;
using System.Collections.Generic;

[Serializable]
public class InventoryData
{
    public List<string> itemNames = new List<string>();
    public List<int> itemCounts = new List<int>();

    public InventoryData(InventorySystem inventory)
    {
        foreach (var slot in inventory.slots)
        {
            if (slot.item != null)
            {
                itemNames.Add(slot.item.itemName);
                itemCounts.Add(slot.amount);
            }
        }
    }
}