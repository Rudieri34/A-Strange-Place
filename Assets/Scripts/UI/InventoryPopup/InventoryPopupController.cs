using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class InventoryPopupController : MonoBehaviour
{
    [SerializeField] private Transform _content;
    [SerializeField] private InventorySlotController[] _slots;
    [SerializeField] private InventoryItemController inventoryItemPrefab;

    private SimpleObjectPool<InventoryItemController> _itemPool;
    private List<InventoryItemController> _activeItems = new();

    void Start()
    {
        _itemPool = new SimpleObjectPool<InventoryItemController>(inventoryItemPrefab, _content, 10);
        InventorySystemManager.Instance.InventoryUpdated += SetupinventoryPopup;

        SetupinventoryPopup();

        _content.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    private void SetupinventoryPopup()
    {
        foreach (var item in _activeItems)
        {
            _itemPool.Return(item);
            item.transform.SetParent(transform);
        }
        _activeItems.Clear();

        foreach (var inventoryItem in InventorySystemManager.Instance.CurrentInventoryItems)
        {
            var itemController = _itemPool.Get();
            itemController.transform.SetParent(_slots[inventoryItem.SlotPosition].transform, false);
            itemController.SetItem(inventoryItem);
            _activeItems.Add(itemController);
        }
    }

    public void ClosePopup()
    {
        _content.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
            .OnComplete(() => ScreenManager.Instance.HidePopup("InventoryPopup"));
    }


    void OnDestroy()
    {
        if (InventorySystemManager.Instance != null)
            InventorySystemManager.Instance.InventoryUpdated -= SetupinventoryPopup;
    }
}
