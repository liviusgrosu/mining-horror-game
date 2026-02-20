using System;
using UnityEngine;
using UnityEngine.AI;

public class ShadeBehaviour : MonoBehaviour
{
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
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _startingStoppingDistance = _agent.stoppingDistance;
        _startingRotation = transform.rotation;
        _currentState = _initialState;
    }

    private void Start()
    {
        _startingPosition = transform.position;
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        if (_shouldPatrol)
        {
            SetPathingDestination();
        }
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
        if (Quaternion.Angle(transform.rotation, _startingRotation) > _rotationTolerance)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _startingRotation, _startingRotationSpeed * Time.deltaTime);
        }
    }

    private void PatrolState()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance)
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
        _agent.SetDestination(_player.position);

        if (_getDistanceFromPlayer <= _agent.stoppingDistance + 0.1f)
        {
            _agent.velocity = Vector3.zero;
            GameManager.Instance.OpenGameOverScreen();
        }

        if (_getDistanceFromPlayer > _engageDistance)
        {
            _agent.isStopped = true;
            _checkStateElapsedTime = 0f;
            _currentState = State.Check;
        }
    }

    private void AttackState()
    {
        var direction = (_player.position - transform.position).normalized;
        direction.y = 0;

        var targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _toPlayerRotateAttackSpeed * Time.deltaTime);

        if (_getDistanceFromPlayer > _agent.stoppingDistance)
        {
            _agent.velocity = _agent.desiredVelocity;
            _currentState = State.Engage;
        }
    }

    private void CheckState()
    {
        _checkStateElapsedTime += Time.deltaTime;

        if (_checkStateElapsedTime >= _checkStateTime)
        {
            _agent.isStopped = false;
            _agent.velocity = _agent.desiredVelocity;
            _agent.stoppingDistance = 0f;
            _checkStateElapsedTime = 0f;
            _currentState = _shouldPatrol || startAtIdle ? State.Patrol : State.Return;
        }
    }

    private void ReturnState()
    {
        _agent.SetDestination(_startingPosition);
        if (Vector3.Distance(transform.position, _startingPosition) < 0.15f)
        {
            _agent.stoppingDistance = _startingStoppingDistance;
            _currentState = State.Idle;
        }
    }

    private void CheckIfPlayerInFov()
    {
        if (_currentState == State.Attack)
        {
            return;
        }

        if (Vector3.Distance(transform.position, _player.position) <= _engageDistance)
        {
            var enemyToPlayer = _player.position - transform.position;

            if (Vector3.Angle(enemyToPlayer, transform.forward) <= _fov)
            {
                _agent.isStopped = false;
                _currentState = State.Engage;
            }
        }
    }
}