using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int ItemId = -1;
    public Image Icon;
    public TextMeshProUGUI Quantity;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemId == -1) return;

        var item = Inventory.Instance.GetItem(ItemId);
        if (item != null)
        {
            InventoryUI.Instance.ShowTooltip(item.Name);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryUI.Instance.HideTooltip();
    }
}
