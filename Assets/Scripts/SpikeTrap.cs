using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private Transform _spikes;
    [SerializeField] private Vector3 _retractedOffset = new Vector3(0f, -1f, 0f);
    [SerializeField] private float _moveDuration = 0.3f;
    [SerializeField] private bool _startExtended = true;

    [Header("Damage")]
    [SerializeField] private BoxCollider _hitVolume;
    [SerializeField] private int _damage = 9999;

    private Vector3 _extendedLocalPosition;
    private Vector3 _retractedLocalPosition;
    private Coroutine _moveRoutine;
    private bool _isExtended;
    private readonly HashSet<EnemyHealth> _hitEnemies = new();
    private static readonly Collider[] _overlapBuffer = new Collider[16];

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
        if (!_isExtended)
        {
            _hitEnemies.Clear();
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
            if (_isExtended)
            {
                DamageEnemiesInVolume();
            }
            yield return null;
        }
        _spikes.localPosition = target;
        _moveRoutine = null;
    }

    private void DamageEnemiesInVolume()
    {
        if (!_hitVolume)
        {
            return;
        }
        var t = _hitVolume.transform;
        var center = t.TransformPoint(_hitVolume.center);
        var halfExtents = Vector3.Scale(_hitVolume.size * 0.5f, t.lossyScale);
        halfExtents = new Vector3(Mathf.Abs(halfExtents.x), Mathf.Abs(halfExtents.y), Mathf.Abs(halfExtents.z));
        var count = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlapBuffer, t.rotation, ~0, QueryTriggerInteraction.Ignore);
        for (var i = 0; i < count; i++)
        {
            var col = _overlapBuffer[i];
            if (!col.CompareTag("Enemy"))
            {
                continue;
            }
            var health = col.GetComponentInParent<EnemyHealth>();
            if (!health || health.IsDead || _hitEnemies.Contains(health))
            {
                continue;
            }
            _hitEnemies.Add(health);
            health.TakeDamage(_damage);
        }
    }
}
