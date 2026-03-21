using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    [SerializeField]
    private float movementSpeed = 6f;
    private float _yVelocity;
    [SerializeField]
    private float gravity = -18f;

    // Sprinting
    [SerializeField]
    private float sprintMultiplier = 1.8f;
    [SerializeField]
    private float maxSprintTime = 5f;
    [SerializeField]
    private float sprintCooldown = 6f;
    private float _sprintTimer;
    private float _cooldownTimer;
    private bool _isSprinting;
    private bool _isOnCooldown;

    public bool IsSprinting => _isSprinting;

    // Breathing audio
    [SerializeField]
    private AudioClip breathingSlowClip;
    [SerializeField]
    private AudioClip breathingHeavyClip;
    private AudioSource _breathingAudioSource;

    // Looking
    private Camera _camera;
    private float _yRotation;
    [SerializeField]
    private float mouseSensitivity = 100f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _camera = Camera.main;

        _breathingAudioSource = gameObject.AddComponent<AudioSource>();
        _breathingAudioSource.loop = true;
        _breathingAudioSource.playOnAwake = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (GameManager.Instance)
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

                _yVelocity += gravity * Time.deltaTime;
                _controller.Move(Vector3.up * (_yVelocity * Time.deltaTime));
                return;    
            }
        }
        
        MovePlayer();
        Look();
    }

    private void Look()
    {
        var mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivity;
        var mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivity;
        
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

        var isMoving = horizontal != 0f || vertical != 0f;
        HandleSprint(isMoving);
        HandleBreathingAudio();

        var movementDir = transform.right * horizontal + transform.forward * vertical;
        var speed = _isSprinting ? movementSpeed * sprintMultiplier : movementSpeed;
        movementDir *= speed;

        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            _yVelocity = Mathf.Sqrt(-2f * gravity);
        }

        _yVelocity += gravity * Time.deltaTime;
        var velocity = movementDir + Vector3.up * _yVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }

    private void HandleBreathingAudio()
    {
        if (_isOnCooldown)
        {
            if (_breathingAudioSource.clip != breathingHeavyClip)
            {
                _breathingAudioSource.clip = breathingHeavyClip;
                _breathingAudioSource.Play();
            }
        }
        else if (_sprintTimer >= 0.5f)
        {
            if (_breathingAudioSource.clip != breathingSlowClip)
            {
                _breathingAudioSource.clip = breathingSlowClip;
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

        var wantsToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving;

        if (wantsToSprint)
        {
            _isSprinting = true;
            _sprintTimer += Time.deltaTime;

            if (_sprintTimer >= maxSprintTime)
            {
                _isSprinting = false;
                _isOnCooldown = true;
                _cooldownTimer = sprintCooldown;
            }
        }
        else
        {
            _isSprinting = false;
            _sprintTimer = Mathf.Max(0f, _sprintTimer - Time.deltaTime);
        }
    }
}
