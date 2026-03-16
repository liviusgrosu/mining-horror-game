using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    [SerializeField]
    private float _movementSpeed = 6f;
    private float _yVelocity;
    [SerializeField]
    private float _gravity = -18f;

    // Sprinting
    [SerializeField]
    private float _sprintMultiplier = 1.8f;
    [SerializeField]
    private float _maxSprintTime = 5f;
    [SerializeField]
    private float _sprintCooldown = 6f;
    private float _sprintTimer;
    private float _cooldownTimer;
    private bool _isSprinting;
    private bool _isOnCooldown;

    public bool IsSprinting => _isSprinting;

    // Breathing audio
    [SerializeField]
    private AudioClip _breathingSlowClip;
    [SerializeField]
    private AudioClip _breathingHeavyClip;
    private AudioSource _breathingAudioSource;

    // Looking
    private Camera _camera;
    private float _yRotation;
    [SerializeField]
    private float _mouseSensitivity = 100f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _camera = Camera.main;

        _breathingAudioSource = gameObject.AddComponent<AudioSource>();
        _breathingAudioSource.loop = true;
        _breathingAudioSource.playOnAwake = false;
    }

    void Update()
    {
        if (GameManager.Instance.HasWon || GameManager.Instance.InMenu || GameManager.Instance.IsPaused || ScreenShakeEffect.Instance.IsCameraShaking)
        {
            _controller.Move(Vector3.zero);
            return;
        }

        if (GameManager.Instance.HasDied)
        {
            if (_controller.isGrounded && _yVelocity < 0)
                _yVelocity = -2f;

            _yVelocity += _gravity * Time.deltaTime;
            _controller.Move(Vector3.up * _yVelocity * Time.deltaTime);
            return;    
        }
        MovePlayer();
        Look();
    }

    private void Look()
    {
        var mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _mouseSensitivity;
        var mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * _mouseSensitivity;
        
        _yRotation -= mouseY;
        _yRotation = Mathf.Clamp(_yRotation, -80f, 80f);
        _camera.transform.localRotation = Quaternion.Euler(_yRotation, 0f, 0f);
        
        transform.Rotate(Vector3.up * mouseX);
    }

    private void MovePlayer()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        if (_controller.isGrounded && _yVelocity < 0)
        {
            _yVelocity = -2f;
        }

        bool isMoving = horizontal != 0f || vertical != 0f;
        HandleSprint(isMoving);
        HandleBreathingAudio();

        var movementDir = transform.right * horizontal + transform.forward * vertical;
        float speed = _isSprinting ? _movementSpeed * _sprintMultiplier : _movementSpeed;
        movementDir *= speed;

        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            _yVelocity = Mathf.Sqrt(-2f * _gravity);
        }

        _yVelocity += _gravity * Time.deltaTime;
        var velocity = movementDir + Vector3.up * _yVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }

    private void HandleBreathingAudio()
    {
        if (_isOnCooldown)
        {
            if (_breathingAudioSource.clip != _breathingHeavyClip)
            {
                _breathingAudioSource.clip = _breathingHeavyClip;
                _breathingAudioSource.Play();
            }
        }
        else if (_sprintTimer >= 0.5f)
        {
            if (_breathingAudioSource.clip != _breathingSlowClip)
            {
                _breathingAudioSource.clip = _breathingSlowClip;
                _breathingAudioSource.Play();
            }
        }
        else
        {
            if (_breathingAudioSource.isPlaying)
            {
                _breathingAudioSource.clip = null;
                _breathingAudioSource.Stop();
            }
        }
    }

    private void HandleSprint(bool isMoving)
    {
        if (_isOnCooldown)
        {
            _isSprinting = false;
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0f)
            {
                _isOnCooldown = false;
                _sprintTimer = 0f;
            }
            return;
        }

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving;

        if (wantsToSprint)
        {
            _isSprinting = true;
            _sprintTimer += Time.deltaTime;

            if (_sprintTimer >= _maxSprintTime)
            {
                _isSprinting = false;
                _isOnCooldown = true;
                _cooldownTimer = _sprintCooldown;
            }
        }
        else
        {
            _isSprinting = false;
            _sprintTimer = Mathf.Max(0f, _sprintTimer - Time.deltaTime);
        }
    }
}
