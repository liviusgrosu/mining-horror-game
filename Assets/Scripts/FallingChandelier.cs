using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class FallingChandelier : MonoBehaviour
{
    [SerializeField] private int _crushDamage = 9999;
    [SerializeField] private NavMeshObstacle _navMeshObstacle;

    [Header("Noise")]
    [SerializeField] private float _noiseRadius = 12f;
    [SerializeField] private string _noiseSurfaceTag = "Metal";

    [Header("Feedback")]
    [SerializeField] private GameObject _landVFXPrefab;
    [SerializeField] private AudioClip _landSFX;

    private Rigidbody _rigidbody;
    private AudioSource _audioSource;
    private bool _hasEmittedLandingNoise;
    private bool _isSettled;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        
        if (_navMeshObstacle)
        {
            _navMeshObstacle.enabled = false;
        }
    }

    private void Update()
    {
        if (!_isSettled && _rigidbody.IsSleeping())
        {
            Settle();
        }
    }

    private void Settle()
    {
        _isSettled = true;
        _rigidbody.isKinematic = true;
        if (_navMeshObstacle)
        {
            _navMeshObstacle.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var isEnemy = collision.collider.CompareTag("Enemy");

        if (!_hasEmittedLandingNoise && !isEnemy)
        {
            _hasEmittedLandingNoise = true;
            NoiseEmitter.Emit(transform.position, _noiseRadius, _noiseSurfaceTag);
            var contactPoint = collision.GetContact(0).point;
            if (_landVFXPrefab)
            {
                Instantiate(_landVFXPrefab, contactPoint, Quaternion.identity);
            }
            if (_landSFX)
            {
                _audioSource.PlayOneShot(_landSFX);
            }
        }

        if (!isEnemy || _isSettled)
        {
            return;
        }
        var health = collision.collider.GetComponentInParent<EnemyHealth>();
        if (!health || health.IsDead)
        {
            return;
        }
        health.TakeDamage(_crushDamage);
    }
}
