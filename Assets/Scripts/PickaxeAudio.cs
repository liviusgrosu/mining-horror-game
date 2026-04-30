using UnityEngine;
using Random = UnityEngine.Random;

public class PickaxeAudio : MonoBehaviour
{
    public AudioClip[] stoneSounds;
    public AudioClip[] gravelSounds;
    public AudioClip[] woodSounds;
    public AudioClip[] grassSounds;
    public AudioClip[] carpetSounds;
    public AudioClip[] metalSounds;
    public AudioClip missSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    private const string GravelTag = "Gravel";
    private const string StoneTag = "Stone";
    private const string WoodTag = "Wood";
    private const string GrassTag = "Grass";
    private const string CarpetTag = "Carpet";
    private const string MetalTag = "Metal";

    private AudioSource _audioSource;
    private int _lastClipIndex = -1;
    private AudioClip[] _lastSoundSet;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayImpactForTag(string surfaceTag)
    {
        var soundSet = GetSoundSetForTag(surfaceTag);
        if (soundSet == null || soundSet.Length == 0)
        {
            return;
        }
        _audioSource.PlayOneShot(GetRandomClip(soundSet), volume);
    }

    public void PlayMiss()
    {
        if (missSound)
        {
            _audioSource.PlayOneShot(missSound, volume);
        }
    }

    private AudioClip[] GetSoundSetForTag(string surfaceTag)
    {
        if (surfaceTag == GravelTag)
        {
            return gravelSounds;
        }
        if (surfaceTag == WoodTag)
        {
            return woodSounds;
        }
        if (surfaceTag == StoneTag)
        {
            return stoneSounds;
        }
        if (surfaceTag == GrassTag)
        {
            return grassSounds;
        }
        if (surfaceTag == CarpetTag)
        {
            return carpetSounds;
        }
        if (surfaceTag == MetalTag)
        {
            return metalSounds;
        }
        return stoneSounds;
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
