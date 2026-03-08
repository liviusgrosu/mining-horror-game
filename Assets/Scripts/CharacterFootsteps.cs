using UnityEngine;

public class CharacterFootsteps : MonoBehaviour
{
    [Header("Gravel Footstep Sounds")]
    public AudioClip[] gravelSounds;

    [Header("Stone Footstep Sounds")]
    public AudioClip[] stoneSounds;

    [Header("Footstep Settings")]
    [Tooltip("Time in seconds between each footstep")]
    public float stepInterval = 0.45f;

    [Range(0f, 1f)]
    public float footstepVolume = 0.8f;

    [Tooltip("How far down to raycast to detect the surface")]
    public float raycastDistance = 1.5f;

    [Tooltip("Tag applied to gravel surfaces in the scene")]
    public string gravelTag = "Gravel";

    [Tooltip("Tag applied to stone surfaces in the scene")]
    public string stoneTag = "Stone";

    private AudioSource _audioSource;
    private CharacterController _characterController;

    private float _stepTimer;
    private int _lastClipIndex = -1;
    private AudioClip[] _lastSoundSet;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _characterController = GetComponent<CharacterController>();

        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
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

            if (_stepTimer >= stepInterval)
            {
                PlayFootstepForSurface();
                _stepTimer = 0f;
            }
        }
        else
        {
            _stepTimer = stepInterval;
        }
    }

    private void PlayFootstepForSurface()
    {
        var soundSet = GetSoundSetForSurface();

        if (soundSet == null || soundSet.Length == 0)
        {
            return;
        }

        var clip = GetRandomClip(soundSet);
        _audioSource.PlayOneShot(clip, footstepVolume);
    }

    private AudioClip[] GetSoundSetForSurface()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, raycastDistance))
        {
            var tag = hit.collider.tag;

            if (tag == gravelTag)
            {
                return gravelSounds;
            }

            if (tag == stoneTag)
            {
                return stoneSounds;
            }

            return stoneSounds;
        }
        return stoneSounds;
    }

    private AudioClip GetRandomClip(AudioClip[] sounds)
    {
        if (sounds.Length == 1)
            return sounds[0];

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
