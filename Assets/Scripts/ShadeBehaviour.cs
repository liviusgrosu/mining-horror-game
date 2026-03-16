using System;
using UnityEngine;
using UnityEngine.AI;

public class ShadeBehaviour : MonoBehaviour
{
    private static readonly int MovementBlend = Animator.StringToHash("MovementBlend");

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
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _startingStoppingDistance = _agent.stoppingDistance;
        _startingRotation = transform.rotation;
        _currentState = _initialState;
        _audioSource.loop = true;
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
        if (!_toggle)
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

        if (_getDistanceFromPlayer <= _agent.stoppingDistance + 0.1f)
        {
            _agent.velocity = Vector3.zero;
            GameManager.Instance.OpenGameOverScreen();
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
}