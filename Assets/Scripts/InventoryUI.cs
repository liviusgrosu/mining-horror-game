using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private List<InventoryUISlot> _itemUISlots;

    private void OnEnable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        _itemUISlots = transform.GetComponentsInChildren<InventoryUISlot>().ToList();

        foreach (var (item, quantity) in Inventory.Instance.Items)
        {
            var slot = _itemUISlots.Find(s => s.ItemId == item.Id);
            if (slot && slot.Quantity)
            {
                slot.Quantity.text = quantity.ToString();
                continue;
            }

            var nextEmptySlot = _itemUISlots.Find(s => s.ItemId == -1);
            nextEmptySlot.Icon.sprite = item.Icon;
            nextEmptySlot.ItemId = item.Id;
            nextEmptySlot.Quantity.text = quantity.ToString();
        }
    }
}
