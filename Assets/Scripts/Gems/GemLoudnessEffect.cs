using UnityEngine;

public class GemLoudnessEffect : MonoBehaviour
{
    [SerializeField] private Material _gemMaterial;
    [SerializeField] private Color _baseEmissiveColor = Color.white;
    [SerializeField] private float _minIntensity = 0.5f;
    [SerializeField] private float _maxIntensity = 6f;
    [SerializeField] private float _minRadius = 0.5f;
    [SerializeField] private float _maxRadius = 10.2f;
    [SerializeField] private float _decayDuration = 0.6f;

    private float _currentIntensity;

    private void Awake()
    {
        _currentIntensity = _minIntensity;
    }

    private void OnEnable()
    {
        CharacterFootsteps.OnFootstepNoise += HandleFootstepNoise;
    }

    private void OnDisable()
    {
        CharacterFootsteps.OnFootstepNoise -= HandleFootstepNoise;
    }

    private void Update()
    {
        if (!_gemMaterial)
        {
            return;
        }

        if (_currentIntensity > _minIntensity && _decayDuration > 0f)
        {
            var decayPerSecond = (_maxIntensity - _minIntensity) / _decayDuration;
            _currentIntensity = Mathf.Max(_minIntensity, _currentIntensity - decayPerSecond * Time.deltaTime);
        }

        _gemMaterial.SetColor("_EmissionColor", _baseEmissiveColor * Mathf.Max(0f, _currentIntensity));
    }

    private void HandleFootstepNoise(float radius)
    {
        var t = Mathf.InverseLerp(_minRadius, _maxRadius, radius);
        var pulse = Mathf.Lerp(_minIntensity, _maxIntensity, t);
        if (pulse > _currentIntensity)
        {
            _currentIntensity = pulse;
        }
    }
}
