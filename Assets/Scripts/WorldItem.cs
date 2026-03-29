using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    public InventoryItem Item;
    [Tooltip("Optional pickup sound override. If set, plays instead of the default pickup sound.")]
    public AudioClip PickupSoundOverride;

    private Transform _outline;

    private void Start()
    {
        _outline = transform.GetChild(0);
    }

    public void ToggleOutline(bool state)
    {
        _outline.gameObject.SetActive(state);
    }
}
