using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class SliderSellected : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Setting")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color deselectedColor = Color.white;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image fillImage;

    public void OnSelect(BaseEventData eventData)
    {
        nameText.DOColor(selectedColor, .2f);
        fillImage.DOColor(selectedColor, .2f);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        nameText.DOColor(deselectedColor, .2f);
        fillImage.DOColor(deselectedColor, .2f);
    }
}