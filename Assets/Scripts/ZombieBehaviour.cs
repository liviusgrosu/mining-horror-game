using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBehaviour : MonoBehaviour
{
    private static readonly int MovementBlend = Animator.StringToHash("MovementBlend");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int TakeHit = Animator.StringToHash("Take Hit");

    public enum State
    {
        Idle,
        Patrol,
        Engage,
        Attack,
        Check,
        Return
    }

    [Header("General")]
    [Tooltip("Turn on/off the behaviour")]
    [SerializeField] private bool _toggle = true;
    [Tooltip("Angle until rotation is complete")]
    [SerializeField] private float _rotationTolerance;

    [Header("Idle State")]
    [Tooltip("FOV of enemy")]
    [SerializeField] private float _fov;
    [Tooltip("How far the player needs to be from the enemy to engage")]
    [SerializeField] private float _engageDistance;
    [Tooltip("How fast the enemy will rotate back to the starting direction they were facing")]
    [SerializeField] private float _startingRotationSpeed = 250f;

    [Header("Check State")]
    [Tooltip("How long the enemy will wait before returning to idle state")]
    [SerializeField] private float _checkStateTime = 2f;

    [Header("Attack State")]
    [Tooltip("How fast the enemy will rotate to the player after finishing an attack")]
    [SerializeField] private float _toPlayerRotateAttackSpeed = 250f;
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
    [SerializeField]
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

    // Patrolling values
    private bool _shouldPatrol => _initialState == State.Patrol;
    [SerializeField]
    private EnemyPathing _pathing;
    private int _currentPointIndex = 0;

    [SerializeField]
    private bool startAtIdle;

    [SerializeField] private bool initiateChase;
    
    [SerializeField]
    private Animator animator;

    private float _animationTime;
    
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField] private AudioClip idleSound;
    [SerializeField] private AudioClip chaseSound;

    [Header("Damage Collider")]
    [SerializeField] private Collider _damageCollider;

    [Header("Health")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private float _hitStunDuration = 0.5f;
    private int _currentHealth;
    private bool _isDead;
    private bool _isTakingHit;
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _startingStoppingDistance = _agent.stoppingDistance;
        _startingRotation = transform.rotation;
        _currentState = _initialState;
        _audioSource.loop = true;
        _currentHealth = _maxHealth;
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

        CheckIfPlayerInFov();

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

        if (!initiateChase && _getDistanceFromPlayer > _engageDistance)
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
        if (_currentState is State.Attack or State.Engage) return;
        if (!(Vector3.Distance(transform.position, _player.position) <= _engageDistance)) return;
        var enemyToPlayer = _player.position - transform.position;

        if (!(Vector3.Angle(enemyToPlayer, transform.forward) <= _fov)) return;
        if (!Physics.Raycast(transform.position, _player.position - transform.position, out var hit, _engageDistance)) return;
        if (!hit.transform.CompareTag("Player")) return;

        _agent.isStopped = false;
        _agent.speed = runningSpeed;
        PlayChaseSound();
        _currentState = State.Engage;
    }

    public void EndChase()
    {
        if (!initiateChase) return;
        
        _agent.isStopped = true;
        _checkStateElapsedTime = 0f;
        PlayIdleSound();
        _currentState = State.Check;
        initiateChase = false;
    }

    private void PlayIdleSound()
    {
        MusicManager.Instance.PlayAmbientMusic();
        _audioSource.Stop();
        _audioSource.clip = idleSound;
        _audioSource.Play();
    }

    private void PlayChaseSound()
    {
        MusicManager.Instance.PlayChaseMusic();
        _audioSource.Stop();
        _audioSource.clip = chaseSound;
        _audioSource.Play();
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

        _attackCooldownTimer = 0f;
        StartCoroutine(HitStun());
    }

    private IEnumerator HitStun()
    {
        _isTakingHit = true;
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

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

        _audioSource.Stop();
        MusicManager.Instance.FadeToAmbientMusic();
        animator.Play("Die", 0, 0f);
    }
}