using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class Wisp : MonoBehaviour
{
    [SerializeField] private EnemyPathing _pathing;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private float _fadeOutDuration = 0.5f;

    private Light _light;
    private float _targetIntensity;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        _light = GetComponent<Light>();
        _targetIntensity = _light.intensity;
        _light.intensity = 0f;
    }

    public void SetPath(EnemyPathing pathing)
    {
        _pathing = pathing;
    }

    private void Start()
    {
        if (!_pathing || _pathing.Points == null || _pathing.Points.Count == 0)
        {
            Destroy(gameObject);
            return;
        }
        if (_pathing.Points[0])
        {
            transform.position = _pathing.Points[0].position;
        }
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        StartFade(_targetIntensity, _fadeInDuration);

        for (var i = 1; i < _pathing.Points.Count; i++)
        {
            var target = _pathing.Points[i];
            if (!target)
            {
                continue;
            }
            var from = transform.position;
            var to = target.position;
            var distance = Vector3.Distance(from, to);
            if (distance <= 0f || _speed <= 0f)
            {
                transform.position = to;
                continue;
            }
            var duration = distance / _speed;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }
            transform.position = to;
        }

        yield return StartFade(0f, _fadeOutDuration);

        Destroy(gameObject);
    }

    private Coroutine StartFade(float to, float duration)
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }
        _fadeRoutine = StartCoroutine(FadeLight(_light.intensity, to, duration));
        return _fadeRoutine;
    }

    private IEnumerator FadeLight(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            _light.intensity = to;
            yield break;
        }
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            _light.intensity = Mathf.Lerp(from, to, t);
            yield return null;
        }
        _light.intensity = to;
    }
}
