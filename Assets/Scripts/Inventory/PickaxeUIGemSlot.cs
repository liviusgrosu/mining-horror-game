using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickaxeUIGemSlot : MonoBehaviour
{
    public int ItemId = -1;
    [HideInInspector]
    public Image Icon;

    private void Awake()
    {
        Icon = GetComponent<Image>();
    }
}
