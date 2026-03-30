using UnityEngine;

public class ShadeMoveAndDestroy : MonoBehaviour
{
    [SerializeField] private GameObject _shade;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private AudioClip triggerSound;

    private AudioSource _audioSource;
    private Vector3 _startPosition;
    private bool _triggered;

    private void Update()
    {
        if (!_triggered)
        {
            return;
        }

        _shade.transform.position += Vector3.right * (_moveSpeed * Time.deltaTime);

        if (Vector3.Distance(_shade.transform.position, _startPosition) >= 10f)
        {
            Destroy(_shade);
            enabled = false;
        }
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
        _startPosition = _shade.transform.position;
        if (triggerSound)
        {
            if (!_audioSource) _audioSource = GetComponent<AudioSource>();
            if (_audioSource) _audioSource.PlayOneShot(triggerSound);
        }
    }
}
