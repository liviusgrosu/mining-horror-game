using TMPro;
using UnityEngine;

public class NoiseDebugUI : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        NoiseEmitter.OnNoise += HandleNoise;
    }

    private void OnDisable()
    {
        NoiseEmitter.OnNoise -= HandleNoise;
    }

    private void HandleNoise(Vector3 position, float radius)
    {
        _text.text = $"{radius:F2}";
    }
}
