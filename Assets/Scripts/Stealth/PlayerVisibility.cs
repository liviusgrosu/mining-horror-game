using System.Collections.Generic;
using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
    public static PlayerVisibility Instance { get; private set; }

    [Header("Sampling")]
    [Tooltip("How often (in seconds) visibility is recalculated. 0.1 = 10 times per second.")]
    [SerializeField] private float _updateInterval = 0.1f;
    [Tooltip("Layer mask for occlusion raycasts. Must exclude the Player layer to avoid self-hits.")]
    [SerializeField] private LayerMask _occlusionMask = ~0;

    [Header("Ambient")]
    [Tooltip("Base visibility that exists everywhere from baked ambient lighting. " +
             "0.1 means the player is never fully invisible even in pure darkness.")]
    [SerializeField] [Range(0f, 0.5f)] private float _ambientBase = 0.1f;

    [Header("Light Contribution")]
    [Tooltip("How quickly visibility smooths between recalculated values.")]
    [SerializeField] private float _smoothSpeed = 5f;

    public float VisibilityValue { get; private set; } = 1f;

    private float _targetVisibility = 1f;
    private float _timeSinceLastUpdate;
    private readonly List<StealthLight> _stealthLights = new();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RefreshLightCache();
    }

    public void RefreshLightCache()
    {
        _stealthLights.Clear();
        _stealthLights.AddRange(FindObjectsOfType<StealthLight>());
    }

    private void Update()
    {
        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate >= _updateInterval)
        {
            _timeSinceLastUpdate = 0f;
            _targetVisibility = ComputeVisibility();
        }

        VisibilityValue = Mathf.Lerp(VisibilityValue, _targetVisibility, _smoothSpeed * Time.deltaTime);
    }

    private float ComputeVisibility()
    {
        var total = _ambientBase;
        var playerPos = transform.position;

        foreach (var stealthLight in _stealthLights)
        {
            if (!stealthLight )
            {
                continue;
            }

            var light = stealthLight.Source;
            if (!light.enabled || !light.gameObject.activeInHierarchy)
            {
                continue;
            }

            var lightPos = light.transform.position;
            var distance = Vector3.Distance(playerPos, lightPos);

            if (distance > stealthLight.DetectionRange)
            {
                continue;
            }

            var direction = (lightPos - playerPos).normalized;
            if (Physics.Raycast(playerPos, direction, distance, _occlusionMask))
            {
                continue;
            }

            // Normalised proximity: 0 at edge of range, 1 at light source
            var t = 1f - (distance / stealthLight.DetectionRange);
            var falloff = stealthLight.FalloffCurve.Evaluate(t);
            total += stealthLight.MaxContribution * falloff;
        }

        return Mathf.Clamp01(total);
    }
}
