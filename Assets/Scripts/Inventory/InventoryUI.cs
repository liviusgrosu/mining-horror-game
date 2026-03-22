using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [SerializeField] private GameObject _tooltip;
    [SerializeField] private TextMeshProUGUI _tooltipText;
    private List<InventoryUISlot> _itemUISlots;
    private bool _tooltipVisible;

    private void Awake()
    {
        Instance = this;

        var cg = _tooltip.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = _tooltip.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnChanged += Refresh;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnChanged -= Refresh;
        }
    }

    private void Update()
    {
        if (_tooltipVisible)
        {
            _tooltip.transform.position = Input.mousePosition + new Vector3(0, -70f, 0);
        }
    }

    public void ShowTooltip(string itemName)
    {
        _tooltipText.text = itemName;
        _tooltip.SetActive(true);
        _tooltipVisible = true;
    }

    public void HideTooltip()
    {
        _tooltip.SetActive(false);
        _tooltipVisible = false;
    }

    private void Refresh()
    {
        _itemUISlots = transform.GetComponentsInChildren<InventoryUISlot>().ToList();

        // Clear all inventory slots first
        foreach (var slot in _itemUISlots)
        {
            slot.Clear();
        }

        // Populate inventory slots
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
