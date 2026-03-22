using UnityEngine;

public enum ItemType { Consumable, Material, GemSlot  }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public int Id;
    public string Name;
    public Sprite Icon;
    public ItemType Type;
}
