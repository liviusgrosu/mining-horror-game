using System;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public event Action OnChanged;

    private readonly Dictionary<InventoryItem, int> _items = new();
    
    private readonly List<InventoryItem> _pickaxeGems = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public IReadOnlyDictionary<InventoryItem, int> Items => _items;

    public InventoryItem TempItem1, TempItem2, TempItem3;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Add(TempItem1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Add(TempItem2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Add(TempItem3);
        }
    }

    public void Add(InventoryItem item)
    {
        if (!_items.TryAdd(item, 1))
        {
            _items[item]++;
        }

        OnChanged?.Invoke();
    }

    public InventoryItem GetItem(int id)
    {
        foreach (var item in _items.Keys)
        {
            if (item.Id == id) return item;
        }
        return null;
    }
}
