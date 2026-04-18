using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the alpha of a UI Image from PlayerVisibility.VisibilityValue.
/// 0 visibility = fully transparent, 1 visibility = fully opaque.
/// Drag the target Image into the Inspector.
/// </summary>
public class VisibilityIndicator : MonoBehaviour
{
    [SerializeField] private Image _image;

    private void Update()
    {
        if (PlayerVisibility.Instance == null || _image == null)
        {
            return;
        }

        var c = _image.color;
        c.a = PlayerVisibility.Instance.VisibilityValue;
        _image.color = c;
    }
}
