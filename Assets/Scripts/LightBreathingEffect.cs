using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightBreathingEffect : MonoBehaviour
{
    [SerializeField] private float _minIntensity = 0.5f;
    [SerializeField] private float _maxIntensity = 1.5f;
    [SerializeField] private float _speed = 1f;

    private Light _light;
    private float _offset;

    private void Awake()
    {
        _light = GetComponent<Light>();
        _offset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * _speed + _offset) + 1f) * 0.5f;
        _light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, t);
    }
}
