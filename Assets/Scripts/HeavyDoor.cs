using System.Collections;
using UnityEngine;

public class HeavyDoor : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private ParticleSystem smokeVFX;
    [SerializeField] private AudioClip soundEffect;
    [SerializeField] private float moveDuration = 1.5f;

    private AudioSource _audioSource;
    private bool _triggered;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        _triggered = true;
        StartCoroutine(MoveDoor());
    }

    private IEnumerator MoveDoor()
    {
        var start = startPos.position;
        var end = endPos.position;
        var elapsed = 0f;

        if (_audioSource != null && soundEffect != null)
        {
            _audioSource.PlayOneShot(soundEffect);
        }

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
            door.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        door.position = end;

        if (smokeVFX != null)
        {
            smokeVFX.Play();
        }

    }
}
