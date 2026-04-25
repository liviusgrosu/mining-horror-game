using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public InventoryItem Item = null;
    public Image Icon;
    public TextMeshProUGUI Quantity;
    public Sprite EmptySprite;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Item) return;

        InventoryUI.Instance.ShowItemDescription(Item.Name, Item.Description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryUI.Instance.HideItemDescription();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Item)
        {
            return;
        }

        switch (Item.Type)
        {
            case ItemType.GemSlot:
                Inventory.Instance.EquipGem(Item);
                break;
            case ItemType.Consumable:
                PlayerHealth.Instance.UseHealthBottle();
                break;
        }
        InventoryUI.Instance.HideItemDescription();
    }

    public void Clear()
    {
        Item = null;
        Icon.sprite = EmptySprite;
        if (Quantity)
        {
            Quantity.text = "";
        }
    }
}
