using System.Collections;
using UnityEngine;

public class TrapDoor : MonoBehaviour
{
    [SerializeField] private Transform _door;
    [SerializeField] private Vector3 _rotationAxis = Vector3.right;
    [SerializeField] private float _openAngle = 90f;
    [SerializeField] private float _openDuration = 0.1f;
    [SerializeField] private float _holdDuration = 3f;
    [SerializeField] private float _closeDuration = 1f;

    [Header("Enemy Catch")]
    [SerializeField] private BoxCollider _catchVolume;
    [SerializeField] private float _enemyFallSpeed = 5f;
    [SerializeField] private float _enemyFallDuration = 5f;

    [Header("Feedback")]
    [SerializeField] private AudioClip _openSound;
    [SerializeField] private AudioClip _closeSound;

    private AudioSource _audioSource;
    private Quaternion _closedRotation;
    private Quaternion _openedRotation;
    private bool _isActive;
    private static readonly Collider[] _overlapBuffer = new Collider[16];

    public bool IsActive => _isActive;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_door)
        {
            _closedRotation = _door.localRotation;
            _openedRotation = _closedRotation * Quaternion.AngleAxis(_openAngle, _rotationAxis.normalized);
        }
    }

    public bool Trigger()
    {
        if (_isActive || !_door)
        {
            return false;
        }
        StartCoroutine(RunSequence());
        return true;
    }

    private IEnumerator RunSequence()
    {
        _isActive = true;
        if (_audioSource && _openSound)
        {
            _audioSource.PlayOneShot(_openSound);
        }
        CatchEnemiesInVolume();
        yield return RotateDoor(_closedRotation, _openedRotation, _openDuration);
        yield return new WaitForSeconds(_holdDuration);
        if (_audioSource && _closeSound)
        {
            _audioSource.PlayOneShot(_closeSound);
        }
        yield return RotateDoor(_openedRotation, _closedRotation, _closeDuration);
        _isActive = false;
    }

    private void CatchEnemiesInVolume()
    {
        if (!_catchVolume)
        {
            return;
        }
        var t = _catchVolume.transform;
        var center = t.TransformPoint(_catchVolume.center);
        var halfExtents = Vector3.Scale(_catchVolume.size * 0.5f, t.lossyScale);
        halfExtents = new Vector3(Mathf.Abs(halfExtents.x), Mathf.Abs(halfExtents.y), Mathf.Abs(halfExtents.z));
        var count = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlapBuffer, t.rotation, ~0, QueryTriggerInteraction.Ignore);
        for (var i = 0; i < count; i++)
        {
            var col = _overlapBuffer[i];
            if (!col.CompareTag("Enemy"))
            {
                continue;
            }
            var ai = col.GetComponentInParent<EnemyAI>();
            if (ai)
            {
                ai.FallIntoTrap(_enemyFallSpeed, _enemyFallDuration);
            }
        }
    }

    private IEnumerator RotateDoor(Quaternion from, Quaternion to, float duration)
    {
        if (duration <= 0f)
        {
            _door.localRotation = to;
            yield break;
        }
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            _door.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        _door.localRotation = to;
    }
}
