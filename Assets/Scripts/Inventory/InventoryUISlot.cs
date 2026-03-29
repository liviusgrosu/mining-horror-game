using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int ItemId = -1;
    public Image Icon;
    public TextMeshProUGUI Quantity;
    public Sprite EmptySprite;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ItemId == -1)
        {
            return;
        }

        var item = Inventory.Instance.GetItem(ItemId);
        if (item == null) return;

        switch (item.Type)
        {
            case ItemType.GemSlot:
                Inventory.Instance.EquipGem(item);
                break;
            case ItemType.Consumable:
                PlayerHealth.Instance.UseHealthBottle();
                // Because we're hard coding to use HP bottle, im commenting this out
                //Inventory.Instance.Remove(item, 1);
                break;
        }
        InventoryUI.Instance.HideTooltip();
    }

    public void Clear()
    {
        ItemId = -1;
        Icon.sprite = EmptySprite;
        if (Quantity)
        {
            Quantity.text = "";
        }
    }
}
