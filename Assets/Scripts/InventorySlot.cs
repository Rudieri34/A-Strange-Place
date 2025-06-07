using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public InventoryItem item;
    public int amount;

    public void AddItem(InventoryItem newItem)
    {
        if (item == newItem)
        {
            amount++;
        }
        else
        {
            item = newItem;
            amount = 1;
        }
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;
    }
}