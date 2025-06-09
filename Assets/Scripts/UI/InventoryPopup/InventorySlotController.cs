using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotController : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount > 0)
            return;

        GameObject dropped = eventData.pointerDrag;
        InventoryItemController itemController = dropped.GetComponent<InventoryItemController>();
        itemController.CurrentParent = transform;
    }
}
