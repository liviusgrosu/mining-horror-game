using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _ambientMusic;
    [SerializeField]
    private AudioClip _chaseMusic;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _audioSource = GetComponent<AudioSource>();
    }
    
    public void PlayAmbientMusic()
    {
        if (_audioSource.clip == _ambientMusic)
        {
            return;
        }
        
        _audioSource.Stop();
        _audioSource.clip = _ambientMusic;
        _audioSource.Play();
    }
    
    public void PlayChaseMusic()
    {
        if (_audioSource.clip == _chaseMusic)
        {
            return;
        }
        
        _audioSource.Stop();
        _audioSource.clip = _chaseMusic;
        _audioSource.Play();
    }
}
