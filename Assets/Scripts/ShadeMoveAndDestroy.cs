using System;
using UnityEngine;

public class ShadeMoveAndDestroy : MonoBehaviour
{
    [SerializeField] private GameObject _shade;
    [SerializeField] private Transform _destination;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private AudioClip triggerSound;

    private AudioSource _audioSource;
    private Camera _playerCamera;
    private bool _activated;
    private bool _moving;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        _playerCamera = Camera.main;
        if (_shade != null)
        {
            _shade.SetActive(false);
        }
    }

    private void Update()
    {
        if (!_activated || _moving)
        {
            return;
        }

        if (!IsShadeVisibleToCamera())
        {
            return;
        }
        
        _moving = true;
        _audioSource.PlayOneShot(triggerSound);
    }

    private void LateUpdate()
    {
        if (!_moving)
        {
            return;
        }

        _shade.transform.position = Vector3.MoveTowards(
            _shade.transform.position,
            _destination.position,
            _moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(_shade.transform.position, _destination.position) < 0.1f)
        {
            Destroy(_shade);
            enabled = false;
        }
    }

    private bool IsShadeVisibleToCamera()
    {
        var viewportPoint = _playerCamera.WorldToViewportPoint(_shade.transform.position);

        return !(viewportPoint.z <= 0) && !(viewportPoint.x < 0) && !(viewportPoint.x > 1) 
               && !(viewportPoint.y < 0) && !(viewportPoint.y > 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_activated)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        _activated = true;
        _shade.SetActive(true);
    }
}
