using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private Transform _spikes;
    [SerializeField] private Vector3 _retractedOffset = new Vector3(0f, -1f, 0f);
    [SerializeField] private float _moveDuration = 0.3f;
    [SerializeField] private bool _startExtended = true;

    private Vector3 _extendedLocalPosition;
    private Vector3 _retractedLocalPosition;
    private Coroutine _moveRoutine;
    private bool _isExtended;

    public bool IsExtended => _isExtended;

    private void Awake()
    {
        _extendedLocalPosition = _spikes.localPosition;
        _retractedLocalPosition = _extendedLocalPosition + _retractedOffset;
        _isExtended = _startExtended;
        _spikes.localPosition = _isExtended
            ? _extendedLocalPosition
            : _retractedLocalPosition;
    }

    public void Toggle()
    {
        SetExtended(!_isExtended);
    }

    public void SetExtended(bool extended)
    {
        if (_isExtended == extended)
        {
            return;
        }
        _isExtended = extended;
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
        }
        var target = _isExtended
            ? _extendedLocalPosition
            : _retractedLocalPosition;
        _moveRoutine = StartCoroutine(MoveTo(target));
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        var start = _spikes.localPosition;
        var elapsed = 0f;
        while (elapsed < _moveDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / _moveDuration);
            _spikes.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }
        _spikes.localPosition = target;
        _moveRoutine = null;
    }
}
