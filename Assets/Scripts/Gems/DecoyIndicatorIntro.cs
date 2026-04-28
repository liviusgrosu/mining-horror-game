using System.Collections;
using UnityEngine;

public class DecoyIndicatorIntro : MonoBehaviour
{
    [SerializeField] private Renderer _rayRenderer;
    [SerializeField] private Transform _plane;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _falloffFrom = 10f;
    [SerializeField] private float _falloffTo = 0.7f;
    [SerializeField] private string _falloffProperty = "_VerticalFalloff";

    private Vector3 _planeTargetScale;
    private MaterialPropertyBlock _block;

    private void Awake()
    {
        if (_plane)
        {
            _planeTargetScale = _plane.localScale;
        }
        _block = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        if (_plane)
        {
            _plane.localScale = Vector3.zero;
        }
        ApplyFalloff(_falloffFrom);

        var elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / _duration);
            if (_plane)
            {
                _plane.localScale = Vector3.Lerp(Vector3.zero, _planeTargetScale, t);
            }
            ApplyFalloff(Mathf.Lerp(_falloffFrom, _falloffTo, t));
            yield return null;
        }

        if (_plane)
        {
            _plane.localScale = _planeTargetScale;
        }
        ApplyFalloff(_falloffTo);
    }

    private void ApplyFalloff(float value)
    {
        if (!_rayRenderer)
        {
            return;
        }
        _rayRenderer.GetPropertyBlock(_block);
        _block.SetFloat(_falloffProperty, value);
        _rayRenderer.SetPropertyBlock(_block);
    }
}
