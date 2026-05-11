using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New Gem Ability", menuName = "Inventory/Gem Ability")]
public class GemAbilityItem : InventoryItem
{
    public GemType GemType;
    public KeyCode Hotkey;
    public VideoClip Video;
}
