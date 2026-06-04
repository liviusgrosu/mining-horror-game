using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BugEmberAttractor : MonoBehaviour
{
    public static readonly List<BugEmberAttractor> Active = new();

    [SerializeField] private float _attractRadius = 15f;
    [SerializeField] private float _minParticleLifetimeRemaining = 0.1f;

    public float AttractRadius => _attractRadius;

    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _buffer;
    private int _count;
    private int _lastReadFrame = -1;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        Active.Add(this);
    }

    private void OnDisable()
    {
        Active.Remove(this);
    }

    public void CollectEmbers(Vector3 from, float maxDistance, List<Vector3> output)
    {
        RefreshParticlesIfNeeded();

        var worldSpace = _ps.main.simulationSpace == ParticleSystemSimulationSpace.World;
        for (var i = 0; i < _count; i++)
        {
            var p = _buffer[i];
            if (p.remainingLifetime < _minParticleLifetimeRemaining)
            {
                continue;
            }
            var worldPos = worldSpace ? p.position : transform.TransformPoint(p.position);
            if (Vector3.Distance(from, worldPos) <= maxDistance)
            {
                output.Add(worldPos);
            }
        }
    }

    private void RefreshParticlesIfNeeded()
    {
        if (_lastReadFrame == Time.frameCount)
        {
            return;
        }
        _lastReadFrame = Time.frameCount;

        var max = _ps.main.maxParticles;
        if (_buffer == null || _buffer.Length < max)
        {
            _buffer = new ParticleSystem.Particle[max];
        }
        _count = _ps.GetParticles(_buffer);
    }
}
