using UnityEngine;

public class GemLoudnessEffect : MonoBehaviour
{
    [SerializeField] private Material _gemMaterial;
    [SerializeField] private float _maxRimIntensity = 3f;
    [SerializeField] private float _maxFacingBoost = 0.5f;
    [SerializeField] private float _minRadius = 0.5f;
    [SerializeField] private float _maxRadius = 10.2f;
    [SerializeField] private float _decayDuration = 0.6f;

    private static readonly int RimIntensityID = Shader.PropertyToID("_RimIntensity");
    private static readonly int FacingBoostID = Shader.PropertyToID("_FacingBoost");

    private float _currentLevel;

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

        if (_currentLevel > 0f && _decayDuration > 0f)
        {
            _currentLevel = Mathf.Max(0f, _currentLevel - Time.deltaTime / _decayDuration);
        }

        _gemMaterial.SetFloat(RimIntensityID, _maxRimIntensity * _currentLevel);
        _gemMaterial.SetFloat(FacingBoostID, _maxFacingBoost * _currentLevel);
    }

    private void HandleFootstepNoise(float radius)
    {
        var pulse = Mathf.Clamp01(Mathf.InverseLerp(_minRadius, _maxRadius, radius));
        if (pulse > _currentLevel)
        {
            _currentLevel = pulse;
        }
    }
}
