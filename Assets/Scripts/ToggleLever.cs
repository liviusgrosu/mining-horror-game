using System.Collections;
using UnityEngine;

public class ToggleLever : MonoBehaviour
{
    [SerializeField] private Transform _handle;
    [SerializeField] private SpikeTrap _spikeTrap;
    [SerializeField] private bool _startInState2;
    [SerializeField] private float _state1AngleX = -60f;
    [SerializeField] private float _state2AngleX = -120f;
    [SerializeField] private float _rotateDuration = 0.2f;
    [SerializeField] private AudioClip _toggleSound;

    private AudioSource _audioSource;
    private bool _inState2;
    private bool _isAnimating;
    private float _currentAngleX;
    private float _baseY;
    private float _baseZ;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _inState2 = _startInState2;
        var euler = _handle.localEulerAngles;
        _baseY = euler.y;
        _baseZ = euler.z;
        _currentAngleX = _inState2
            ? _state2AngleX
            : _state1AngleX;
        ApplyAngle(_currentAngleX);
    }

    public void Toggle()
    {
        if (_isAnimating)
        {
            return;
        }
        if (_audioSource && _toggleSound)
        {
            _audioSource.PlayOneShot(_toggleSound);
        }
        _inState2 = !_inState2;
        var targetAngle = _inState2
            ? _state2AngleX
            : _state1AngleX;

        StartCoroutine(RotateHandle(targetAngle));
        if (_spikeTrap)
        {
            _spikeTrap.Toggle();
        }
    }

    private IEnumerator RotateHandle(float targetX)
    {
        _isAnimating = true;
        var startX = _currentAngleX;
        var elapsed = 0f;
        while (elapsed < _rotateDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / _rotateDuration);
            _currentAngleX = Mathf.Lerp(startX, targetX, t);
            ApplyAngle(_currentAngleX);
            yield return null;
        }
        _currentAngleX = targetX;
        ApplyAngle(_currentAngleX);
        _isAnimating = false;
    }

    private void ApplyAngle(float x)
    {
        _handle.localEulerAngles = new Vector3(x, _baseY, _baseZ);
    }
}
