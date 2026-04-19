using UnityEngine;

public class CharacterFootsteps : MonoBehaviour
{
    [Header("Gravel Footstep Sounds")]
    public AudioClip[] gravelSounds;

    [Header("Stone Footstep Sounds")]
    public AudioClip[] stoneSounds;

    [Header("Wood Footstep Sounds")]
    public AudioClip[] woodSounds;

    [Header("Grass Footstep Sounds")]
    public AudioClip[] grassSounds;
    
    [Header("Carpet Footstep Sounds")]
    public AudioClip[] carpetSounds;
    
    [Header("Metal Footstep Sounds")]
    public AudioClip[] metalSounds;
    
    [Header("Footstep Settings")]
    public float stepInterval = 0.45f;
    public float sprintStepInterval = 0.3f;
    public float crouchStepInterval = 0.65f;

    [Range(0f, 1f)]
    public float footstepVolume = 0.8f;

    public float raycastDistance = 1.5f;

    public string gravelTag = "Gravel";
    public string stoneTag = "Stone";
    public string woodTag = "Wood";
    public string grassTag = "Grass";
    public string carpetTag = "Carpet";
    public string metalTag = "Metal";

    [Header("Noise")]
    [SerializeField] private float _baseNoiseRadius = 8f;
    [SerializeField] private float _gravelNoise = 0.9f;
    [SerializeField] private float _stoneNoise = 0.5f;
    [SerializeField] private float _woodNoise = 0.65f;
    [SerializeField] private float _grassNoise = 0.4f;
    [SerializeField] private float _carpetNoise = 0.3f;
    [SerializeField] private float _metalNoise = 0.85f;
    [SerializeField] private float _walkSpeedMultiplier = 0.5f;
    [SerializeField] private float _sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float _crouchSpeedMultiplier = 0.2f;

    private AudioSource _audioSource;
    private CharacterController _characterController;
    private PlayerMovement _playerMovement;
    private int _ignoreSelfMask;

    private float _stepTimer;
    private int _lastClipIndex = -1;
    private AudioClip[] _lastSoundSet;
    private float _lastNoiseRadius;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _characterController = GetComponent<CharacterController>();
        _playerMovement = GetComponent<PlayerMovement>();

        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _ignoreSelfMask = ~(1 << gameObject.layer);
    }

    private void Update()
    {
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        var isMoving = _characterController.velocity.magnitude > 0.1f;
        var isGrounded = _characterController.isGrounded;

        if (isMoving && isGrounded)
        {
            _stepTimer += Time.deltaTime;

            var currentInterval = _playerMovement != null && _playerMovement.IsCrouching ? crouchStepInterval :
                                  _playerMovement != null && _playerMovement.IsSprinting ? sprintStepInterval : stepInterval;

            if (_stepTimer >= currentInterval)
            {
                PlayFootstepForSurface();
                EmitFootstepNoise();
                _stepTimer = 0f;
            }
        }
        else
        {
            _stepTimer = stepInterval;
        }
    }

    private void EmitFootstepNoise()
    {
        var surfaceNoise = GetSurfaceNoiseLevel();
        var speedMultiplier = _playerMovement && _playerMovement.IsCrouching ? _crouchSpeedMultiplier :
                              _playerMovement && _playerMovement.IsSprinting ? _sprintSpeedMultiplier : _walkSpeedMultiplier;

        _lastNoiseRadius = _baseNoiseRadius * surfaceNoise * speedMultiplier;
        NoiseEmitter.Emit(transform.position, _lastNoiseRadius);
    }

    private float GetSurfaceNoiseLevel()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out var hit, raycastDistance, _ignoreSelfMask))
        {
            return _stoneNoise;
        }

        var tag = hit.collider.tag;

        if (tag == gravelTag)
        {
            return _gravelNoise;
        }
        if (tag == woodTag)
        {
            return _woodNoise;
        }
        if (tag == grassTag)
        {
            return _grassNoise;
        }
        if (tag == carpetTag)
        {
            return _carpetNoise;
        }
        if (tag == metalTag)
        {
            return _metalNoise;
        }
        return _stoneNoise;
    }

    private void PlayFootstepForSurface()
    {
        var soundSet = GetSoundSetForSurface();

        if (soundSet == null || soundSet.Length == 0)
        {
            return;
        }

        _audioSource.PlayOneShot(GetRandomClip(soundSet), footstepVolume);
    }

    private AudioClip[] GetSoundSetForSurface()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, raycastDistance, _ignoreSelfMask))
        {
            var tag = hit.collider.tag;

            if (tag == gravelTag)
            {
                return gravelSounds;
            }
            if (tag == woodTag)
            {
                return woodSounds;
            }
            if (tag == stoneTag)
            {
                return stoneSounds;
            }
            if (tag == grassTag)
            {
                return grassSounds;
            }
            if (tag == carpetTag)
            {
                return carpetSounds;
            }
            if (tag == metalTag)
            {
                return metalSounds;
            }
            return stoneSounds;
        }
        return stoneSounds;
    }

    private void OnDrawGizmos()
    {
        if (_lastNoiseRadius <= 0f)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, _lastNoiseRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, _lastNoiseRadius);
    }

    private AudioClip GetRandomClip(AudioClip[] sounds)
    {
        if (sounds.Length == 1)
        {
            return sounds[0];
        }

        if (sounds != _lastSoundSet)
        {
            _lastClipIndex = -1;
            _lastSoundSet = sounds;
        }

        int index;
        do
        {
            index = Random.Range(0, sounds.Length);
        }
        while (index == _lastClipIndex);

        _lastClipIndex = index;
        return sounds[index];
    }
}
