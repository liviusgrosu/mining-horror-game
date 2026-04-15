using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickaxeUIGemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int ItemId = -1;
    public Image Icon;
    public Sprite EmptySprite;
    
    public void Clear()
    {
        ItemId = -1;
        Icon.sprite = EmptySprite;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemId == -1)
        {
            return;
        }

        var item = Inventory.Instance.GetItem(ItemId);
        if (item != null)
        {
            InventoryUI.Instance.ShowItemDescription(item.Name, item.Description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryUI.Instance.HideItemDescription();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ItemId == -1)
        {
            return;
        }

        var item = Inventory.Instance.GetItem(ItemId);
        if (item != null)
        {
            Inventory.Instance.RemoveGem(item);
            InventoryUI.Instance.HideItemDescription();
        }
    }
}
