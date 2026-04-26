using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyPerception))]
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
        Investigate,
        Searching,
        Suspicious
    }

    [Header("General")]
    [Tooltip("Turn on/off the behaviour")]
    [SerializeField] private bool _toggle = true;
    [Tooltip("Angle until rotation is complete")]
    [SerializeField] private float _rotationTolerance;

    [Header("Idle State")]
    [SerializeField] private float _proximityEngageDistance = 2f;
    [SerializeField] private float _startingRotationSpeed = 500f;

    [Header("Check State")]
    [Tooltip("How long the enemy will wait before returning to idle state")]
    [SerializeField] private float _checkStateTime = 2f;

    [Header("Searching State")]
    [SerializeField] private float _searchDuration = 8f;
    [SerializeField] private float _searchRadius = 6f;
    [SerializeField] private float _searchPauseTime = 1f;
    [SerializeField] private float _searchPointTolerance = 1f;

    [Header("Suspicious State")]
    [SerializeField] private float _suspicionDuration = 3f;
    [SerializeField] private float _suspicionRotateSpeed = 500f;

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
    private NavMeshAgent _agent;
    private EnemyPerception _perception;
    [SerializeField] private float runningSpeed = 2.5f;
    [SerializeField] private float walkingSpeed = 1f;

    
    private Vector3 _startingPosition;
    private float _startingStoppingDistance;
    private Quaternion _startingRotation;
    private float _checkStateElapsedTime;
    private float _getDistanceFromPlayer => Vector3.Distance(transform.position, _perception.Player.position);
    private Vector3 _investigateTarget;
    private float _lastNoiseTime;
    private float _searchElapsedTime;
    private float _searchPauseTimer;
    private bool _hasSearchPoint;
    private Vector3 _currentSearchPoint;
    private Vector3 _suspicionTarget;
    private float _suspicionElapsedTime;
    private State _lastDebugState;

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

    [Header("Debug")]
    [SerializeField] private bool neverEngage;
    [SerializeField] private bool shutUpPlease;
    [SerializeField] private bool stayInPlace;
    [SerializeField] private MeshRenderer _debugStateRenderer;
    [SerializeField] private Material _idleDebugMaterial;
    [SerializeField] private Material _patrolDebugMaterial;
    [SerializeField] private Material _engageDebugMaterial;
    [SerializeField] private Material _attackDebugMaterial;
    [SerializeField] private Material _checkDebugMaterial;
    [SerializeField] private Material _returnDebugMaterial;
    [SerializeField] private Material _investigateDebugMaterial;
    [SerializeField] private Material _searchingDebugMaterial;
    [SerializeField] private Material _suspiciousDebugMaterial;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _perception = GetComponent<EnemyPerception>();
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
        if (!_perception)
        {
            _perception = GetComponent<EnemyPerception>();
        }
        if (_perception)
        {
            _perception.OnStimulus += OnStimulus;
        }
    }

    private void OnDisable()
    {
        if (_perception)
        {
            _perception.OnStimulus -= OnStimulus;
        }
    }

    private void OnStimulus(Stimulus s)
    {
        if (_isDead || _isTakingHit || !_toggle)
        {
            return;
        }
        if (_currentState is State.Engage or State.Attack)
        {
            return;
        }

        if (s.Tier == StimulusTier.Strong && s.FromPlayer && !neverEngage)
        {
            if (s.Kind == StimulusKind.Sound)
            {
                _lastNoiseTime = Time.time;
            }
            EnterEngage();
            return;
        }

        if (s.Tier == StimulusTier.Moderate)
        {
            if (s.Kind == StimulusKind.Sound)
            {
                if (_currentState == State.Investigate)
                {
                    var distToNew = Vector3.Distance(transform.position, s.Position);
                    var distToCurrent = Vector3.Distance(transform.position, _investigateTarget);
                    _lastNoiseTime = Time.time;
                    if (distToNew >= distToCurrent)
                    {
                        return;
                    }
                }
                else
                {
                    _lastNoiseTime = Time.time;
                }
                EnterInvestigate(s.Position);
                return;
            }

            if (_currentState == State.Investigate)
            {
                return;
            }
            EnterInvestigate(s.Position);
            return;
        }

        if (s.Tier == StimulusTier.Faint)
        {
            // TODO: If post-engage, immediatly go into engage
            // TOOD: If investigate/searching/checking, go into suspicious or investigate
            
            if (_currentState is State.Investigate or State.Searching)
            {
                return;
            }
            EnterSuspicious(s.Position);
        }
    }

    private void EnterEngage()
    {
        _agent.isStopped = false;
        _agent.speed = runningSpeed;
        PlayChaseSound();
        _currentState = State.Engage;
    }

    private void EnterInvestigate(Vector3 target)
    {
        _investigateTarget = target;
        _agent.isStopped = false;
        _agent.speed = walkingSpeed;
        _agent.stoppingDistance = 0f;
        _currentState = State.Investigate;
    }

    private void EnterSuspicious(Vector3 target)
    {
        _suspicionTarget = target;
        _agent.isStopped = true;
        _agent.ResetPath();
        if (_currentState != State.Suspicious)
        {
            _suspicionElapsedTime = 0f;
        }
        _currentState = State.Suspicious;
    }

    private void Start()
    {
        _startingPosition = transform.position;

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
            case State.Searching:
                SearchingState();
                break;
            case State.Suspicious:
                SuspiciousState();
                break;
        }

        if (_debugStateRenderer && _currentState != _lastDebugState)
        {
            _lastDebugState = _currentState;
            var mat = GetStateDebugMaterial(_currentState);
            if (mat)
            {
                _debugStateRenderer.sharedMaterial = mat;
            }
        }
    }

    private Material GetStateDebugMaterial(State state)
    {
        return state switch
        {
            State.Idle        => _idleDebugMaterial,
            State.Patrol      => _patrolDebugMaterial,
            State.Engage      => _engageDebugMaterial,
            State.Attack      => _attackDebugMaterial,
            State.Check       => _checkDebugMaterial,
            State.Return      => _returnDebugMaterial,
            State.Investigate => _investigateDebugMaterial,
            State.Searching   => _searchingDebugMaterial,
            State.Suspicious  => _suspiciousDebugMaterial,
            _                 => null
        };
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
        _agent.SetDestination(_perception.Player.position);

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

        if (_getDistanceFromPlayer > _perception.MaxEngageDistance)
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

        var directionToPlayer = (_perception.Player.position - transform.position).normalized;
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
            _currentState = _shouldPatrol ? State.Patrol : State.Return;
        }
    }

    private void InvestigateState()
    {
        animator.SetFloat(MovementBlend, 0.5f, 0.1f, Time.deltaTime);
        _agent.SetDestination(_investigateTarget);

        if (Vector3.Distance(transform.position, _investigateTarget) <= _agent.stoppingDistance + 1.5f)
        {
            var noiseStoppedTime = Time.time - _lastNoiseTime;
            if (noiseStoppedTime < 1.5f)
            {
                _agent.isStopped = true;
                animator.SetFloat(MovementBlend, 0f, 0.1f, Time.deltaTime);
                return;
            }

            _agent.ResetPath();
            _searchElapsedTime = 0f;
            _searchPauseTimer = 0f;
            _hasSearchPoint = false;
            _currentState = State.Searching;
        }
    }

    private void  SearchingState()
    {
        _searchElapsedTime += Time.deltaTime;

        if (_searchElapsedTime >= _searchDuration)
        {
            _agent.isStopped = false;
            _agent.speed = walkingSpeed;
            _agent.stoppingDistance = 0f;
            _currentState = _shouldPatrol ? State.Patrol : State.Return;
            return;
        }

        if (!_hasSearchPoint)
        {
            animator.SetFloat(MovementBlend, 0f, 0.1f, Time.deltaTime);
            if (TryFindSearchPoint(out var newPoint))
            {
                _currentSearchPoint = newPoint;
                _agent.isStopped = false;
                _agent.speed = walkingSpeed;
                _agent.SetDestination(_currentSearchPoint);
                _hasSearchPoint = true;
            }
            return;
        }

        if (Vector3.Distance(transform.position, _currentSearchPoint) > _searchPointTolerance)
        {
            animator.SetFloat(MovementBlend, 0.5f, 0.1f, Time.deltaTime);
            return;
        }

        animator.SetFloat(MovementBlend, 0f, 0.1f, Time.deltaTime);
        _agent.isStopped = true;
        _searchPauseTimer += Time.deltaTime;
        if (_searchPauseTimer >= _searchPauseTime)
        {
            _searchPauseTimer = 0f;
            _hasSearchPoint = false;
        }
    }

    private void SuspiciousState()
    {
        animator.SetFloat(MovementBlend, 0f, 0.1f, Time.deltaTime);

        var direction = _suspicionTarget - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _suspicionRotateSpeed * Time.deltaTime);
        }

        _suspicionElapsedTime += Time.deltaTime;
        if (_suspicionElapsedTime >= _suspicionDuration)
        {
            _agent.isStopped = false;
            _agent.speed = walkingSpeed;
            _agent.stoppingDistance = 0f;
            _currentState = _shouldPatrol ? State.Patrol : State.Return;
        }
    }

    private bool TryFindSearchPoint(out Vector3 point)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var offset = UnityEngine.Random.insideUnitSphere * _searchRadius;
            offset.y = 0f;
            var candidate = _investigateTarget + offset;
            if (NavMesh.SamplePosition(candidate, out var hit, _searchRadius, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }
        point = Vector3.zero;
        return false;
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

    public void Disengage()
    {
        _isAttacking = false;
        animator.SetBool(IsAttacking, false);
        _agent.isStopped = false;
        _agent.speed = walkingSpeed;
        _agent.stoppingDistance = 0f;
        _checkStateElapsedTime = 0f;
        PlayIdleSound();
        _currentState = _shouldPatrol ? State.Patrol : State.Return;
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
                _agent.SetDestination(_perception.Player.position);
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
}