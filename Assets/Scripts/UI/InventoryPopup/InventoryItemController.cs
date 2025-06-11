using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class InventoryItemController : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public Transform CurrentParent = null;
    public InventoryItem Item;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Transform _optionsBox;
    [SerializeField] private TMP_Text _useLabel;
    [SerializeField] private TMP_Text _discartLabel;


    bool _isShowingOptions;
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Dragging: " + gameObject.name);

        transform.DOComplete();
        CurrentParent = transform.parent;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        _itemImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging: " + gameObject.name);
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Dragging: " + gameObject.name);
        transform.SetParent(CurrentParent);
        transform.DOLocalMove(Vector3.zero, .2f);
        _itemImage.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleOptionsBox();
    }

    void ToggleOptionsBox()
    {
        if (_isShowingOptions)
            _optionsBox.transform.DOScale(Vector3.zero, .2f);
        else
            _optionsBox.transform.DOScale(Vector3.one, .2f);

        _isShowingOptions = !_isShowingOptions;
    }

    public void SetItem(InventoryItem item)
    {
        Item = item;
        _itemImage.sprite = InventorySystemManager.Instance.CompleteInventoryItemsList.Find(it => it.ItemName == item.ItemName).ItemSpriteIcon;


        switch (Item.UseType)
        {
            case ItemUseType.Drop:
                _useLabel.text = "Drop";
                break;

            case ItemUseType.Heal:
                _useLabel.text = "Heal";
                break;

            case ItemUseType.Throw:
                _useLabel.text = "Throw";
                break;

            case ItemUseType.Save:
                _useLabel.text = "Save";
                _discartLabel.text = "Clear";
                break;
        }

    }


    public void DiscartItem()
    {
        InventorySystemManager.Instance.RemoveItem(Item);
        ToggleOptionsBox();
    }


    public void UseItem()
    {
        InventorySystemManager.Instance.UseItem(Item);
        ToggleOptionsBox();
    }

}
