using UnityEngine;

public class BreathingEmission : MonoBehaviour
{
    [Header("Emission Settings")]
    [Tooltip("The minimum emission intensity (trough of the breath)")]
    public float minIntensity = -5f;
 
    [Tooltip("The maximum emission intensity (peak of the breath)")]
    public float maxIntensity = 2f;
 
    [Header("Timing")]
    [Tooltip("How many seconds one full breath cycle takes")]
    public float breathDuration = 2f;
 
    private Material _material;
    private Color _baseEmissionColor;

    private void Start()
    {
        _material = GetComponent<Renderer>().material;
        _baseEmissionColor = _material.GetColor("_EmissionColor").linear;
        _material.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        var t = (Mathf.Sin(Time.time * (2f * Mathf.PI / breathDuration)) + 1f) / 2f;
        var intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
 
        var linearMultiplier = Mathf.Pow(2f, intensity);
        _material.SetColor("_EmissionColor", _baseEmissionColor * linearMultiplier);
    }

    private void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }
}
