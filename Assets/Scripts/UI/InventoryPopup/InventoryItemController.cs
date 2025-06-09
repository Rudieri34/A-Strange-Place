using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class InventoryItemController : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public Transform CurrentParent = null;
    [SerializeField] private Image _itemImage;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Dragging: " + gameObject.name);

        transform.DOKill();
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
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
