using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBehaviour : MonoBehaviour
{
    private static readonly int MovementBlend = Animator.StringToHash("MovementBlend");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

    public enum State
    {
        Idle,
        Patrol,
        Engage,
        Attack,
        Check,
        Return,
        Investigate
    }

    [Header("General")]
    [Tooltip("Turn on/off the behaviour")]
    [SerializeField] private bool _toggle = true;
    [Tooltip("Angle until rotation is complete")]
    [SerializeField] private float _rotationTolerance;

    [Header("Idle State")]
    [SerializeField] private float _fov;
    [SerializeField] private float _verticalSightHeight = 2f;
    [SerializeField] private float _litEngageDistance;
    [SerializeField] private float _darkEngageDistance = 2f;
    [SerializeField] private float _litInvestigationDistance = 8f;
    [SerializeField] private float _darkInvestigationDistance = 2f;
    [SerializeField] private float _proximityEngageDistance = 2f;
    [SerializeField] private float _startingRotationSpeed = 500f;

    [Header("Check State")]
    [Tooltip("How long the enemy will wait before returning to idle state")]
    [SerializeField] private float _checkStateTime = 2f;

    [Header("Attack State")]
    [Tooltip("How fast the enemy will rotate to the player after finishing an attack")]
    [SerializeField] private float _toPlayerRotateAttackSpeed = 500f;
    [Tooltip("Damage dealt per attack")]
    [SerializeField] private int _attackDamage = 20;
    [Tooltip("Cooldown between attacks in seconds")]
    [SerializeField] private float _attackCooldown = 2f;
    [Tooltip("Distance to trigger attack")]
    [SerializeField] private float _attackRange = 2f;
    private float _attackCooldownTimer;
    private bool _isAttacking;

    [SerializeField] private float _movementThreshold = 0.1f;
    private bool _wasMoving;

    [SerializeField]
    private State _initialState = State.Idle;
    private State _currentState = State.Idle;
    private Transform _player;
    private NavMeshAgent _agent;
    [SerializeField] private float runningSpeed = 2.5f;
    [SerializeField] private float walkingSpeed = 1f;

    
    private Vector3 _startingPosition;
    private float _startingStoppingDistance;
    private Quaternion _startingRotation;
    private float _checkStateElapsedTime;
    private float _getDistanceFromPlayer => Vector3.Distance(transform.position, _player.position);
    private Vector3 _investigateTarget;

    // Patrolling values
    private bool _shouldPatrol => _initialState == State.Patrol;
    [SerializeField]
    private EnemyPathing _pathing;  
    private int _currentPointIndex = 0;

    [SerializeField]
    private Animator animator;

    private float _animationTime;

    [Header("Audio")]
    [SerializeField]
    private AudioSource _loopAudioSource;
    [SerializeField]
    private AudioSource _oneShotAudioSource;

    [SerializeField] private AudioClip idleSound;
    [SerializeField] private AudioClip chaseSound;
    [SerializeField] private AudioClip takeDamageSound;
    [SerializeField] private AudioClip dieSound;

    [Header("Damage Collider")]
    [SerializeField] private Collider _damageCollider;

    [Header("Blood Pool")]
    [SerializeField] private Transform _bloodPool;
    [SerializeField] private float _bloodPoolExpandTime = 3f;

    [Header("Health")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private float _hitStunDuration = 0.5f;
    private int _currentHealth;
    private bool _isDead;
    private bool _isTakingHit;

    [Header("Sound Occlusion")]
    [SerializeField] private LayerMask _occlusionMask;
    [SerializeField] private float _hardSurfaceAttenuation = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float _noiseEngageRatio = 0.35f;

    private bool _gizmoNoiseActive;
    private bool _gizmoNoiseHeard;
    private Vector3 _gizmoNoisePosition;

    [Header("Debug")]
    [SerializeField] private bool neverEngage;
    [SerializeField] private bool shutUpPlease;
    [SerializeField] private bool stayInPlace;
    [SerializeField] private bool isBlind;
    [SerializeField] private bool isDeaf;
    
    [Header("Legacy (DO NOT USE)")]
    [SerializeField] 
    private bool initiateChase;
    [SerializeField]
    private bool startAtIdle;
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.angularSpeed = 500f;
        _startingStoppingDistance = _agent.stoppingDistance;
        _startingRotation = transform.rotation;
        _currentState = _initialState;
        _currentHealth = _maxHealth;
        
        // Debugging
        _loopAudioSource.enabled = !shutUpPlease;
        walkingSpeed = stayInPlace ? 0f : walkingSpeed;
    }
    
    private void OnEnable()
    {
        NoiseEmitter.OnNoise += HandleNoise;
    }

    private void OnDisable()
    {
        NoiseEmitter.OnNoise -= HandleNoise;
    }

    private void HandleNoise(Vector3 position, float radius, string _)
    {
        if (isDeaf)
        {
            return;
        }

        if (_currentState is State.Engage or State.Attack)
        {
            return;
        }

        var direction = position - transform.position;

        if (direction.magnitude > radius)
        {
            return;
        }

        var effectiveRadius = radius;

        if (Physics.Raycast(transform.position, direction.normalized, out var hit, direction.magnitude, _occlusionMask))
        {
            if (hit.collider.CompareTag("Stone") || 
                hit.collider.CompareTag("Wood")  || 
                hit.collider.CompareTag("Grass") ||
                hit.collider.CompareTag("Metal") ||
                hit.collider.CompareTag("Gravel"))
            {
                effectiveRadius *= _hardSurfaceAttenuation;
            }
        }

        var heard = direction.magnitude <= effectiveRadius;
        _gizmoNoiseActive = true;
        _gizmoNoiseHeard = heard;
        _gizmoNoisePosition = position;

        if (!heard)
        {
            return;
        }

        if (!neverEngage && direction.magnitude <= effectiveRadius * _noiseEngageRatio)
        {
            _agent.isStopped = false;
            _agent.speed = runningSpeed;
            PlayChaseSound();
            _currentState = State.Engage;
            return;
        }

        _investigateTarget = position;
        _agent.isStopped = false;
        _agent.speed = walkingSpeed;
        _agent.stoppingDistance = 0f;
        _currentState = State.Investigate;
    }

    private void Start()
    {
        _startingPosition = transform.position;
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        if (_shouldPatrol)
        {
            SetPathingDestination();
        }

        PlayIdleSound();
    }

    private void Update()
    {
        if (!_toggle || _isTakingHit)
        {
            return;
        }

        if (!GameManager.Instance.HasDied)
        {
            if (!isBlind)
            {
                CheckIfPlayerInFov();
                CheckIfPlayerLit();
            }
        }
        switch (_currentState)
        {
            case State.Idle:
                IdleState();
                break;
            case State.Patrol:
                PatrolState();
                break;
            case State.Engage:
                EngageState();
                break;
            case State.Attack:
                AttackState();
                break;
            case State.Check:
                CheckState();
                break;
            case State.Return:
                ReturnState();
                break;
            case State.Investigate:
                InvestigateState();
                break;
        }
    }

    private void IdleState()
    {
        animator.SetFloat(MovementBlend, 0f, 0.1f, Time.deltaTime);
        if (Quaternion.Angle(transform.rotation, _startingRotation) > _rotationTolerance)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _startingRotation, _startingRotationSpeed * Time.deltaTime);
        }
    }

    private void PatrolState()
    {
        animator.SetFloat(MovementBlend, 0.5f, 0.1f, Time.deltaTime);
        
        var distanceToDestination = Vector3.Distance(transform.position, _agent.destination);
        if (distanceToDestination <= _agent.stoppingDistance + 1)
        {
            _currentPointIndex = (_currentPointIndex + 1) % _pathing.Points.Count;
            SetPathingDestination();
        }
    }

    private void SetPathingDestination()
    {
        _agent.SetDestination(_pathing.Points[_currentPointIndex].position);
    }

    private void EngageState()
    {
        animator.SetFloat(MovementBlend, 1f, 0.1f, Time.deltaTime);
        _agent.SetDestination(_player.position);

        if (_getDistanceFromPlayer <= _attackRange)
        {
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
            _isAttacking = true;
            _attackCooldownTimer = 0f;
            animator.SetBool(IsAttacking, true);
            animator.Play("Attack", 0, 0f);
            _currentState = State.Attack;
        }

        if (!initiateChase && _getDistanceFromPlayer > _litEngageDistance)
        {
            _agent.ResetPath();
            _agent.isStopped = true;
            _checkStateElapsedTime = 0f;
            _currentState = State.Check;
            PlayIdleSound();
        }
    }

    private void AttackState()
    {
        animator.SetFloat(MovementBlend, 0f, 0.1f, Time.deltaTime);

        var directionToPlayer = (_player.position - transform.position).normalized;
        directionToPlayer.y = 0f;
        if (directionToPlayer != Vector3.zero)
        {
            var targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _toPlayerRotateAttackSpeed * Time.deltaTime);
        }

        _attackCooldownTimer += Time.deltaTime;

        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Attack") && _getDistanceFromPlayer > _attackRange * 1.5f)
        {
            _isAttacking = false;
            animator.SetBool(IsAttacking, false);
            _agent.isStopped = false;
            _currentState = State.Engage;
            return;
        }

        if (_attackCooldownTimer >= _attackCooldown)
        {
            _attackCooldownTimer = 0f;
            animator.SetBool(IsAttacking, true);
            animator.Play("Attack", 0, 0f);
        }
    }

    private void CheckState()
    {
        animator.SetFloat(MovementBlend, 0f, 0.1f, Time.deltaTime);
        _checkStateElapsedTime += Time.deltaTime;

        if (_checkStateElapsedTime >= _checkStateTime)
        {
            _agent.isStopped = false;
            _agent.speed = walkingSpeed;
            _agent.stoppingDistance = 0f;
            _checkStateElapsedTime = 0f;
            _currentState = _shouldPatrol || startAtIdle ? State.Patrol : State.Return;
        }
    }

    private void InvestigateState()
    {
        animator.SetFloat(MovementBlend, 0.5f, 0.1f, Time.deltaTime);
        _agent.SetDestination(_investigateTarget);

        if (Vector3.Distance(transform.position, _investigateTarget) <= _agent.stoppingDistance + 1f)
        {
            _agent.ResetPath();
            _agent.isStopped = true;
            _checkStateElapsedTime = 0f;
            _currentState = State.Check;
        }
    }

    private void ReturnState()
    {
        animator.SetFloat(MovementBlend, 0.5f, 0.1f, Time.deltaTime);
        _agent.SetDestination(_startingPosition);
        if (Vector3.Distance(transform.position, _startingPosition) < 0.15f)
        {
            _agent.stoppingDistance = _startingStoppingDistance;
            _currentState = State.Idle;
        }
    }

    private void CheckIfPlayerInFov()
    {
        if (_currentState is State.Attack or State.Engage)
        {
            return;
        }

        var visibility = PlayerVisibility.Instance ? PlayerVisibility.Instance.VisibilityValue : 1f;
        var effectiveEngageDistance = Mathf.Lerp(_darkEngageDistance, _litEngageDistance, visibility);

        if (_getDistanceFromPlayer > effectiveEngageDistance)
        {
            return;
        }

        var verticalDiff = _player.position.y - transform.position.y;
        if (Mathf.Abs(verticalDiff) > _verticalSightHeight)
        {
            return;
        }

        var enemyToPlayer = _player.position - transform.position;
        var flatDirection = new Vector3(enemyToPlayer.x, 0f, enemyToPlayer.z);
        var flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        if (!(Vector3.Angle(flatDirection, flatForward) <= _fov))
        {
            return;
        }

        if (!Physics.Raycast(transform.position, enemyToPlayer, out var hit, effectiveEngageDistance))
        {
            return;
        }

        if (!hit.transform.CompareTag("Player"))
        {
            return;
        }

        if (neverEngage)
        {
            return;
        }

        _agent.isStopped = false;
        _agent.speed = runningSpeed;
        PlayChaseSound();
        _currentState = State.Engage;
    }

    private void CheckIfPlayerLit()
    {
        if (_currentState is State.Engage or State.Attack or State.Investigate)
        {
            return;
        }

        var visibility = PlayerVisibility.Instance ? PlayerVisibility.Instance.VisibilityValue : 1f;
        var effectiveInvestigationDistance = Mathf.Lerp(_darkInvestigationDistance, _litInvestigationDistance, visibility);
        
        if (_getDistanceFromPlayer > effectiveInvestigationDistance)
        {
            return;
        }

        var directionToPlayer = _player.position - transform.position;
        if (!Physics.Raycast(transform.position, directionToPlayer.normalized, out var hit, effectiveInvestigationDistance))
        {
            return;
        }

        if (!hit.transform.CompareTag("Player"))
        {
            return;
        }

        _investigateTarget = _player.position;
        _agent.isStopped = false;
        _agent.speed = walkingSpeed;
        _agent.stoppingDistance = 0f;
        _currentState = State.Investigate;
    }

    public void EndChase()
    {
        if (!initiateChase)
        {
            return;
        }

        _agent.isStopped = true;
        _checkStateElapsedTime = 0f;
        PlayIdleSound();
        _currentState = State.Check;
        initiateChase = false;
    }

    public void Disengage()
    {
        initiateChase = false;
        _isAttacking = false;
        animator.SetBool(IsAttacking, false);
        _agent.isStopped = false;
        _agent.speed = walkingSpeed;
        _agent.stoppingDistance = 0f;
        _checkStateElapsedTime = 0f;
        PlayIdleSound();
        _currentState = _shouldPatrol || startAtIdle ? State.Patrol : State.Return;
    }

    private void PlayIdleSound()
    {
        MusicManager.Instance.FadeToAmbientMusic();
        _loopAudioSource.Stop();
        _loopAudioSource.clip = idleSound;
        _loopAudioSource.Play();
    }

    private void PlayChaseSound()
    {
        MusicManager.Instance.PlayChaseMusic();
        _loopAudioSource.Stop();
        _loopAudioSource.clip = chaseSound;
        _loopAudioSource.Play();
    }


    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        if (!neverEngage && _currentState is not (State.Engage or State.Attack))
        {
            _agent.speed = runningSpeed;
            PlayChaseSound();
            _currentState = State.Engage;
        }

        _attackCooldownTimer = 0f;
        StartCoroutine(HitStun());
    }

    private IEnumerator HitStun()
    {
        _isTakingHit = true;
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        if (takeDamageSound)
        {
            _oneShotAudioSource.PlayOneShot(takeDamageSound);
        }
        animator.CrossFadeInFixedTime("Take Hit", 0.1f, 0);
        yield return new WaitForSeconds(_hitStunDuration);

        _isTakingHit = false;
        if (!_isDead)
        {
            if (_currentState == State.Attack)
            {
                _attackCooldownTimer = 0f;
                animator.SetBool(IsAttacking, true);
                animator.Play("Attack", 0, 0f);
            }
            else
            {
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
                animator.CrossFadeInFixedTime("Movement", 0.15f, 0);
            }
        }
    }

    public void EnableDamageCollider()
    {
        _damageCollider.enabled = true;
    }

    public void DisableDamageCollider()
    {
        _damageCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth.Instance.TakeDamage(_attackDamage);
            _damageCollider.enabled = false;
        }
    }

    private void Die()
    {
        _isDead = true;
        _toggle = false;

        _agent.isStopped = true;
        _agent.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        _loopAudioSource.Stop();
        _oneShotAudioSource.Stop();
        if (dieSound)
        {
            _oneShotAudioSource.PlayOneShot(dieSound);
        }
        MusicManager.Instance.FadeToAmbientMusic();
        animator.Play("Die", 0, 0f);

        if (_bloodPool)
            StartCoroutine(ExpandBloodPool());
    }

    private IEnumerator ExpandBloodPool()
    {
        yield return new WaitForSeconds(1f);
        var elapsedTime = 0f;
        var targetScale = new Vector3(0.3f, _bloodPool.localScale.y, 0.3f);

        while (elapsedTime < _bloodPoolExpandTime)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / _bloodPoolExpandTime;
            var scale = _bloodPool.localScale;
            scale.x = Mathf.Lerp(0f, targetScale.x, t);
            scale.z = Mathf.Lerp(0f, targetScale.z, t);
            _bloodPool.localScale = scale;
            yield return null;
        }

        _bloodPool.localScale = targetScale;
    }

    private void OnDrawGizmos()
    {
        if (!_gizmoNoiseActive)
        {
            return;
        }

        Gizmos.color = _gizmoNoiseHeard ? Color.green : Color.red;
        Gizmos.DrawLine(_gizmoNoisePosition, transform.position);
    }
}