using System.Collections.Generic;
using UnityEngine;

public class Bug : MonoBehaviour
{
    [SerializeField] private Collider _bounds;
    [SerializeField] private float _wanderSpeed = 1.5f;
    [SerializeField] private float _attractionSpeed = 3f;
    private float _turnSpeed = 6f;
    [SerializeField] private float _arriveDistance = 0.15f;
    [SerializeField] private float _wanderPauseMin = 0.2f;
    [SerializeField] private float _wanderPauseMax = 1.2f;
    [SerializeField] private float _shimmyAngle = 18f;
    [SerializeField] private float _shimmyFrequency = 14f;
    private float _separationRadius = 0.4f;
    private float _separationStrength = 1.5f;
    private float _targetRefreshInterval = 0.15f;
    private string _decoyNoiseTag = "Decoy";
    private float _decoyNoiseLifetime = 1.5f;

    [Header("Keeps a single pulsing decoy from filling the list with duplicate positions.")]
    [SerializeField] private float _decoyMergeDistance = 0.5f;
    [SerializeField] private LayerMask _bugLayer;

    private struct HeardNoise
    {
        public Vector3 Position;
        public float Radius;
        public float ExpireTime;
    }

    private readonly List<HeardNoise> _heardDecoys = new();
    private readonly List<Vector3> _emberCandidates = new();

    private Vector3 _wanderTarget;
    private float _wanderPauseUntil;
    private float _baseY;
    private float _shimmyPhase;
    private Quaternion _baseRotation;
    private Vector3 _emberTarget;
    private bool _hasEmberTarget;
    private float _nextTargetRefresh;

    private void OnEnable()
    {
        NoiseEmitter.OnNoise += HandleNoise;
    }

    private void OnDisable()
    {
        NoiseEmitter.OnNoise -= HandleNoise;
    }

    private void Start()
    {
        _baseY = transform.position.y;
        _shimmyPhase = Random.value * Mathf.PI * 2f;
        _baseRotation = transform.rotation;
        PickWanderTarget();
    }

    private void Update()
    {
        if (Time.time >= _nextTargetRefresh)
        {
            _nextTargetRefresh = Time.time + _targetRefreshInterval;
            RefreshEmberTarget();
        }

        var target = _hasEmberTarget ? _emberTarget : _wanderTarget;

        if (!_hasEmberTarget && Time.time < _wanderPauseUntil)
        {
            ApplyShimmy();
            return;
        }

        var flat = new Vector3(target.x - transform.position.x, 0f, target.z - transform.position.z);
        var distance = flat.magnitude;

        if (distance <= _arriveDistance)
        {
            if (!_hasEmberTarget)
            {
                _wanderPauseUntil = Time.time + Random.Range(_wanderPauseMin, _wanderPauseMax);
                PickWanderTarget();
            }
            ApplyShimmy();
            return;
        }

        var direction = flat / distance;
        direction += GetSeparation() * _separationStrength;
        direction.y = 0f;
        direction.Normalize();

        var speed = _hasEmberTarget ? _attractionSpeed : _wanderSpeed;
        var step = direction * (speed * Time.deltaTime);
        transform.position += step;

        var lookRot = Quaternion.LookRotation(direction, Vector3.up);
        _baseRotation = Quaternion.Slerp(_baseRotation, lookRot, _turnSpeed * Time.deltaTime);

        ApplyShimmy();
    }

    private void HandleNoise(Vector3 position, float radius, string surfaceTag)
    {
        if (surfaceTag != _decoyNoiseTag)
        {
            return;
        }

        var expire = Time.time + _decoyNoiseLifetime;
        for (var i = 0; i < _heardDecoys.Count; i++)
        {
            if (Vector3.Distance(_heardDecoys[i].Position, position) <= _decoyMergeDistance)
            {
                _heardDecoys[i] = new HeardNoise { Position = position, Radius = radius, ExpireTime = expire };
                return;
            }
        }
        _heardDecoys.Add(new HeardNoise { Position = position, Radius = radius, ExpireTime = expire });
    }

    private void RefreshEmberTarget()
    {
        _hasEmberTarget = false;
        var bestDistance = float.MaxValue;

        for (var i = _heardDecoys.Count - 1; i >= 0; i--)
        {
            if (Time.time >= _heardDecoys[i].ExpireTime)
            {
                _heardDecoys.RemoveAt(i);
            }
        }

        for (var i = 0; i < _heardDecoys.Count; i++)
        {
            var noise = _heardDecoys[i];
            var d = Vector3.Distance(transform.position, noise.Position);
            if (d <= noise.Radius && d < bestDistance)
            {
                bestDistance = d;
                _emberTarget = noise.Position;
                _hasEmberTarget = true;
            }
        }
        if (_hasEmberTarget)
        {
            return;
        }

        _emberCandidates.Clear();
        var attractors = BugEmberAttractor.Active;
        for (var i = 0; i < attractors.Count; i++)
        {
            var attractor = attractors[i];
            if (!attractor)
            {
                continue;
            }
            attractor.CollectEmbers(transform.position, attractor.AttractRadius, _emberCandidates);
        }
        if (_emberCandidates.Count > 0)
        {
            _emberTarget = _emberCandidates[Random.Range(0, _emberCandidates.Count)];
            _hasEmberTarget = true;
        }
    }

    private void ApplyShimmy()
    {
        var yaw = Mathf.Sin(Time.time * _shimmyFrequency + _shimmyPhase) * _shimmyAngle;
        transform.rotation = _baseRotation * Quaternion.Euler(0f, yaw, 0f);
    }

    private Vector3 GetSeparation()
    {
        if (_separationRadius <= 0f)
        {
            return Vector3.zero;
        }

        var hits = Physics.OverlapSphere(transform.position, _separationRadius, _bugLayer, QueryTriggerInteraction.Collide);
        var push = Vector3.zero;
        var count = 0;
        for (var i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == transform)
            {
                continue;
            }
            var offset = transform.position - hits[i].transform.position;
            offset.y = 0f;
            var dist = offset.magnitude;
            if (dist <= 0.0001f)
            {
                offset = new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f);
                dist = offset.magnitude;
            }
            push += offset / (dist * dist);
            count++;
        }
        if (count == 0)
        {
            return Vector3.zero;
        }
        return push / count;
    }

    private void PickWanderTarget()
    {
        if (!_bounds)
        {
            _wanderTarget = transform.position;
            return;
        }

        var b = _bounds.bounds;
        _wanderTarget = new Vector3(
            Random.Range(b.min.x, b.max.x),
            _baseY,
            Random.Range(b.min.z, b.max.z));
    }
}
