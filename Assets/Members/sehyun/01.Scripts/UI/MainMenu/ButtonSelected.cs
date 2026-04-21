using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;


public class ButtonSelected : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    private RectTransform rectTransfrom;

    [Header("ButtonSetting")]
    [SerializeField] private Vector3 selectedSize = new Vector3(1.2f, 1.2f, 1);
    [SerializeField] private Vector3 deselectedSize = Vector3.one;

    private void Awake()
    {
        rectTransfrom = GetComponent<RectTransform>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        rectTransfrom.DOScale(selectedSize, .1f);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        rectTransfrom.DOScale(deselectedSize, .1f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}