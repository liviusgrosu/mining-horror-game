using UnityEngine;

public class GemVisibilityEffect : MonoBehaviour
{
    [SerializeField] private Material _gemMaterial;
    [SerializeField] private Color _baseEmissiveColor = Color.white;
    [SerializeField] private float _minIntensity = 1f;
    [SerializeField] private float _maxIntensity = 4f;

    private void Update()
    {
        if (!_gemMaterial || !PlayerVisibility.Instance)
        {
            return;
        }

        var visibility = PlayerVisibility.Instance.VisibilityValue;
        var intensity = Mathf.Lerp(_minIntensity, _maxIntensity, visibility);
        _gemMaterial.SetColor("_EmissionColor", _baseEmissiveColor * intensity);
    }
}
