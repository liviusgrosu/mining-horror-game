using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _loopAudioSource;
    [SerializeField] private AudioSource _oneShotAudioSource;

    [SerializeField] private AudioClip _idleSound;
    [SerializeField] private AudioClip _chaseSound;
    [SerializeField] private AudioClip _takeDamageSound;
    [SerializeField] private AudioClip _dieSound;

    [Header("Debug")]
    [SerializeField] private bool _shutUpPlease;

    private void Awake()
    {
        if (_loopAudioSource)
        {
            _loopAudioSource.enabled = !_shutUpPlease;
        }
    }

    public void PlayIdleLoop()
    {
        if (MusicManager.Instance)
        {
            MusicManager.Instance.FadeToAmbientMusic();
        }
        _loopAudioSource.Stop();
        _loopAudioSource.clip = _idleSound;
        _loopAudioSource.Play();
    }

    public void PlayChaseLoop()
    {
        if (MusicManager.Instance)
        {
            MusicManager.Instance.PlayChaseMusic();
        }
        _loopAudioSource.Stop();
        _loopAudioSource.clip = _chaseSound;
        _loopAudioSource.Play();
    }

    public void PlayHurt()
    {
        if (_takeDamageSound)
        {
            _oneShotAudioSource.PlayOneShot(_takeDamageSound);
        }
    }

    public void PlayDie()
    {
        if (_dieSound)
        {
            _oneShotAudioSource.PlayOneShot(_dieSound);
        }
    }

    public void StopAll()
    {
        if (_loopAudioSource)
        {
            _loopAudioSource.Stop();
        }
        if (_oneShotAudioSource)
        {
            _oneShotAudioSource.Stop();
        }
    }
}
