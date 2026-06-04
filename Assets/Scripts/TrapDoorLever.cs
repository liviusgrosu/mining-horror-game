using System.Collections;
using UnityEngine;

public class TrapDoorLever : MonoBehaviour
{
    [SerializeField] private Transform _handle;
    [SerializeField] private TrapDoor _trapDoor;
    [SerializeField] private float _restingAngleX = -60f;
    [SerializeField] private float _pulledAngleX = -120f;
    [SerializeField] private float _pullDuration = 0.1f;
    [SerializeField] private float _resetDuration = 1f;
    [SerializeField] private AudioClip _pullSound;
    [SerializeField] private AudioClip _resetSound;

    private AudioSource _audioSource;
    private bool _isLocked;
    private float _currentAngleX;
    private float _baseY;
    private float _baseZ;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        var euler = _handle.localEulerAngles;
        _baseY = euler.y;
        _baseZ = euler.z;
        _currentAngleX = _restingAngleX;
        ApplyAngle(_currentAngleX);
    }

    public void Toggle()
    {
        if (_isLocked)
        {
            return;
        }
        if (!_trapDoor || _trapDoor.IsActive)
        {
            return;
        }
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        _isLocked = true;
        if (_audioSource && _pullSound)
        {
            _audioSource.PlayOneShot(_pullSound);
        }
        yield return RotateHandle(_pulledAngleX, _pullDuration);
        _trapDoor.Trigger();
        while (_trapDoor.IsActive)
        {
            yield return null;
        }
        if (_audioSource && _resetSound)
        {
            _audioSource.PlayOneShot(_resetSound);
        }
        yield return RotateHandle(_restingAngleX, _resetDuration);
        _isLocked = false;
    }

    private IEnumerator RotateHandle(float targetX, float duration)
    {
        var startX = _currentAngleX;
        if (duration <= 0f)
        {
            _currentAngleX = targetX;
            ApplyAngle(_currentAngleX);
            yield break;
        }
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            _currentAngleX = Mathf.Lerp(startX, targetX, t);
            ApplyAngle(_currentAngleX);
            yield return null;
        }
        _currentAngleX = targetX;
        ApplyAngle(_currentAngleX);
    }

    private void ApplyAngle(float x)
    {
        _handle.localEulerAngles = new Vector3(x, _baseY, _baseZ);
    }
}
