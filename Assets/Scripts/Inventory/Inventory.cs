using System;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public event Action OnChanged;

    private int _inventoryCapacity = 8;
    private int _pickaxeGemCapacity = 1;
    
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
    public IReadOnlyList<InventoryItem> PickaxeGems => _pickaxeGems;

    public InventoryItem TempItem1, TempItem2, TempItem3, TempItem4;

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
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Add(TempItem4);
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


    public void EquipGem(InventoryItem item)
    {
        if (_pickaxeGems.Count >= _pickaxeGemCapacity)
        {
            return;
        }

        if (_items.ContainsKey(item))
        {
            _items[item]--;
            if (_items[item] <= 0)
            {
                _items.Remove(item);
            }
        }

        _pickaxeGems.Add(item);
        OnChanged?.Invoke();
    }

    public void RemoveGem(InventoryItem item)
    {
        if (_pickaxeGems.Count >= _inventoryCapacity)
        {
            return;
        }

        if (_pickaxeGems.Contains(item))
        {
            _pickaxeGems.Remove(item);
        }
        Add(item);
        OnChanged?.Invoke();
    }

    public int GetCount(InventoryItem item)
    {
        return _items.GetValueOrDefault(item, 0);
    }

    public void Remove(InventoryItem item, int amount)
    {
        if (!_items.ContainsKey(item)) 
        {
            return;
        }        

        _items[item] -= amount;
        if (_items[item] <= 0)
        {
            _items.Remove(item);
        }

        OnChanged?.Invoke();
    }

    public InventoryItem GetItem(int id)
    {
        foreach (var item in _items.Keys)
        {
            if (item.Id == id)
            {
                return item;
            }
        }
        foreach (var item in _pickaxeGems)
        {
            if (item.Id == id)
            {
                return item;
            }
        }
        return null;
    }
}
