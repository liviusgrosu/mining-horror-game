using UnityEngine;

public class GemVisibilityEffect : MonoBehaviour
{
    [SerializeField] private Material _gemMaterial;
    [SerializeField] private float _maxRimIntensity = 3f;
    [SerializeField] private float _maxFacingBoost = 0.5f;

    private static readonly int RimIntensityID = Shader.PropertyToID("_RimIntensity");
    private static readonly int FacingBoostID = Shader.PropertyToID("_FacingBoost");

    private void Update()
    {
        if (!_gemMaterial || !PlayerVisibility.Instance)
        {
            return;
        }

        var t = Mathf.Clamp01(PlayerVisibility.Instance.VisibilityValue);
        _gemMaterial.SetFloat(RimIntensityID, _maxRimIntensity * t);
        _gemMaterial.SetFloat(FacingBoostID, _maxFacingBoost * t);
    }
}
