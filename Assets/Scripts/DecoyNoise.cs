using UnityEngine;

public class DecoyNoise : MonoBehaviour
{
    [SerializeField] private float _noiseRadius = 15f;
    [SerializeField] private float _emitInterval = 1f;

    private float _timer;
    private float _lastNoiseRadius;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _emitInterval)
        {
            _timer = 0f;
            _lastNoiseRadius = _noiseRadius;
            NoiseEmitter.Emit(transform.position, _noiseRadius, "Decoy");
        }
    }

    private void OnDrawGizmos()
    {
        var radius = Application.isPlaying ? _lastNoiseRadius : _noiseRadius;
        if (radius <= 0f)
        {
            return;
        }

        Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(0.5f, 0f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
