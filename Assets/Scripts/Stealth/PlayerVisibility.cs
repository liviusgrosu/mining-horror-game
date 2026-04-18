using System.Collections.Generic;
using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
    public static PlayerVisibility Instance { get; private set; }

    [Header("Sampling")]
    [SerializeField] private float _updateInterval = 0.1f;
    [SerializeField] private LayerMask _occlusionMask = ~0;

    [Header("Ambient")]

    [SerializeField] [Range(0f, 0.5f)] private float _ambientBase = 0.1f;

    [Header("Light Contribution")]
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

    private void RefreshLightCache()
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
            if (!stealthLight)
            {
                continue;
            }

            var light = stealthLight.Source;
            if (!light.enabled || !light.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (light.type == LightType.Rectangle || light.type == LightType.Disc)
            {
                var localPlayer = light.transform.InverseTransformPoint(playerPos);
                if (localPlayer.z < 0f)
                {
                    continue;
                }
            }

            var effectivePos = GetEffectiveLightPoint(light, playerPos);
            if (light.type == LightType.Rectangle || light.type == LightType.Disc)
            {
                effectivePos += (playerPos - effectivePos).normalized * 0.05f;
            }
            var distance = Vector3.Distance(playerPos, effectivePos);

            if (distance > stealthLight.DetectionRange)
            {
                continue;
            }

            var coneFactor = 1f;
            if (light.type == LightType.Spot)
            {
                var lightToPlayer = (playerPos - light.transform.position).normalized;
                var angleToPlayer = Vector3.Angle(light.transform.forward, lightToPlayer);
                var halfSpotAngle = light.spotAngle * 0.5f;
                if (angleToPlayer > halfSpotAngle)
                {
                    continue;
                }

                coneFactor = 1f - (angleToPlayer / halfSpotAngle);
            }

            var direction = (effectivePos - playerPos).normalized;
            if (Physics.Raycast(playerPos, direction, distance, _occlusionMask))
            {
                continue;
            }

            var t = 1f - (distance / stealthLight.DetectionRange);
            var falloff = stealthLight.FalloffCurve.Evaluate(t);
            var contribution = stealthLight.MaxContribution * falloff * coneFactor;
            total += contribution;
        }

        return Mathf.Clamp01(total);
    }

    private static Vector3 GetEffectiveLightPoint(Light light, Vector3 playerPos)
    {
        if (light.type == LightType.Rectangle)
        {
            var t = light.transform;
            var localPlayer = t.InverseTransformPoint(playerPos);
            var halfW = light.areaSize.x * 0.5f;
            var halfH = light.areaSize.y * 0.5f;
            var closest = new Vector3(
                Mathf.Clamp(localPlayer.x, -halfW, halfW),
                Mathf.Clamp(localPlayer.y, -halfH, halfH),
                0f
            );
            return t.TransformPoint(closest);
        }

        if (light.type == LightType.Disc)
        {
            var t = light.transform;
            var localPlayer = t.InverseTransformPoint(playerPos);
            var radius = light.areaSize.x * 0.5f;
            var onPlane = new Vector2(localPlayer.x, localPlayer.y);
            if (onPlane.magnitude > radius)
            {
                onPlane = onPlane.normalized * radius;
            }
            return t.TransformPoint(new Vector3(onPlane.x, onPlane.y, 0f));
        }

        return light.transform.position;
    }
}
