using UnityEngine;

/// <summary>
/// Marks a Light as relevant to the stealth visibility system.
/// Add this component to any light source that should expose the player
/// (torches, lanterns, glowing crystals, etc.).
///
/// Stealth math is fully decoupled from URP's physical light units:
///   - DetectionRange: how far this light reaches for stealth purposes
///   - MaxContribution: the most this light can add to VisibilityValue (0-1)
///   - FalloffCurve: shape of contribution from edge (x=0) to source (x=1)
///     Default is linear. Raise the curve for a harsher snap, lower it for a
///     gradual fade-in as you approach.
/// </summary>
[RequireComponent(typeof(Light))]
public class StealthLight : MonoBehaviour
{
    [Tooltip("How far this light can contribute to player visibility. " +
             "Independent of the Light's render range.")]
    [SerializeField] private float _detectionRange = 10f;

    [Tooltip("Maximum visibility contribution from this light (0-1). " +
             "1 = can fully expose the player on its own. " +
             "0.5 = can only push visibility to 50% on its own.")]
    [SerializeField] [Range(0f, 1f)] private float _maxContribution = 1f;

    [Tooltip("Falloff curve. X = normalised distance from edge (0) to source (1). " +
             "Y = contribution multiplier. " +
             "Default linear means contribution grows evenly as you approach. " +
             "An S-curve or sharp knee gives a more distinct safe/unsafe boundary.")]
    [SerializeField] private AnimationCurve _falloffCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public Light Source { get; private set; }
    public float DetectionRange => _detectionRange;
    public float MaxContribution => _maxContribution;
    public AnimationCurve FalloffCurve => _falloffCurve;

    private void Awake()
    {
        Source = GetComponent<Light>();
    }
}
