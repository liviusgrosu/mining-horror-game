using UnityEngine;

public class DecoyGem : MonoBehaviour
{
    [SerializeField] private float _lifetime = 10f;
    [SerializeField] private GameObject _decoyPrefab;
    [SerializeField] private GameObject _previewPrefab;
    [SerializeField] private GameObject _invalidPreviewPrefab;
    [SerializeField] private float _validDistance = 2f;
    [SerializeField] private float _maxPreviewDistance = 10f;
    [SerializeField] private LayerMask _validLayers;
    [SerializeField] private LayerMask _ignoreLayers;

    private enum PreviewState { Hidden, Valid, Invalid }

    private GameObject _activeDecoy;
    private GameObject _previewInstance;
    private GameObject _activePreviewPrefab;
    private PreviewState _currentState;
    private Vector3 _lastSpawnPos;

    private void Update()
    {
        if (GameManager.Instance && (GameManager.Instance.InMenu || GameManager.Instance.HasDied))
        {
            DestroyPreview();
            return;
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            UpdatePreview();
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            if (_currentState == PreviewState.Valid)
            {
                SpawnDecoy();
            }

            DestroyPreview();
        }
    }

    private PreviewState Evaluate(out Vector3 spawnPos)
    {
        spawnPos = Vector3.zero;
        var cam = Camera.main.transform;

        if (!Physics.Raycast(cam.position, cam.forward, out var hit, _maxPreviewDistance, ~_ignoreLayers))
        {
            return PreviewState.Hidden;
        }

        spawnPos = hit.point;
        var isValidLayer = (_validLayers & (1 << hit.collider.gameObject.layer)) != 0;

        if (hit.distance <= _validDistance && isValidLayer)
        {
            return PreviewState.Valid;
        }

        return PreviewState.Invalid;
    }

    private void UpdatePreview()
    {
        _currentState = Evaluate(out _lastSpawnPos);

        if (_currentState == PreviewState.Hidden)
        {
            DestroyPreview();
            return;
        }

        var targetPrefab = _currentState == PreviewState.Valid
            ? _previewPrefab
            : _invalidPreviewPrefab;

        if (!_previewInstance || _activePreviewPrefab != targetPrefab)
        {
            DestroyPreview();
            _activePreviewPrefab = targetPrefab;
            _previewInstance = Instantiate(targetPrefab, _lastSpawnPos, Quaternion.identity);
        }
        else
        {
            _previewInstance.transform.position = _lastSpawnPos;
        }
    }

    private void DestroyPreview()
    {
        if (_previewInstance)
        {
            Destroy(_previewInstance);
            _previewInstance = null;
            _activePreviewPrefab = null;
        }
    }

    private void SpawnDecoy()
    {
        if (_activeDecoy)
        {
            Destroy(_activeDecoy);
        }

        _activeDecoy = Instantiate(_decoyPrefab, _lastSpawnPos, Quaternion.identity);
        Destroy(_activeDecoy, _lifetime);
    }
}
