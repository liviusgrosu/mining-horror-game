using System;
using TMPro;
using UnityEngine;

public class UpdateLightLevelText : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _text.text = $"{PlayerVisibility.Instance.VisibilityValue:P0}";
    }
}
