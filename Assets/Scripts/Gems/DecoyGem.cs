using UnityEngine;

public class DecoyGem : MonoBehaviour
{
    [SerializeField] private float _lifetime = 10f;
    [SerializeField] private GameObject _decoyPrefab;
    [SerializeField] private GameObject _previewPrefab;
    [SerializeField] private float _maxPreviewDistance = 10f;
    [SerializeField] private LayerMask _validLayers;
    [SerializeField] private LayerMask _ignoreLayers;

    private GameObject _activeDecoy;
    private GameObject _previewInstance;
    private Vector3 _lastSpawnPos;
    private Quaternion _lastSpawnRot;
    private bool _isValidSpawn;

    private void Update()
    {
        if (GameManager.Instance && (GameManager.Instance.InMenu || GameManager.Instance.HasDied))
        {
            DestroyPreview();
            return;
        }

        var gemUI = GemSelectionUI.Instance;
        if (!gemUI || gemUI.IsOpen || gemUI.SelectedGem != GemType.Decoy)
        {
            DestroyPreview();
            return;
        }

        if (Input.GetKey(KeyCode.F))
        {
            UpdatePreview();
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            if (_isValidSpawn)
            {
                SpawnDecoy();
            }
            DestroyPreview();
        }
    }

    private void EvaluateSpawn()
    {
        var cam = Camera.main.transform;
        if (Physics.Raycast(cam.position, cam.forward, out var hit, _maxPreviewDistance, ~_ignoreLayers)
            && (_validLayers & (1 << hit.collider.gameObject.layer)) != 0)
        {
            _lastSpawnPos = hit.point;
            _lastSpawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            _isValidSpawn = true;
            return;
        }

        _isValidSpawn = false;
    }

    private void UpdatePreview()
    {
        EvaluateSpawn();

        if (!_isValidSpawn)
        {
            DestroyPreview();
            return;
        }

        if (!_previewInstance)
        {
            _previewInstance = Instantiate(_previewPrefab, _lastSpawnPos, _lastSpawnRot);
        }
        else
        {
            _previewInstance.transform.SetPositionAndRotation(_lastSpawnPos, _lastSpawnRot);
        }
    }

    private void DestroyPreview()
    {
        if (_previewInstance)
        {
            Destroy(_previewInstance);
            _previewInstance = null;
        }
        _isValidSpawn = false;
    }

    private void SpawnDecoy()
    {
        if (!GemSelectionUI.Instance.TryConsumeUse(GemType.Decoy))
        {
            return;
        }

        if (_activeDecoy)
        {
            Destroy(_activeDecoy);
        }

        _activeDecoy = Instantiate(_decoyPrefab, _lastSpawnPos, _lastSpawnRot);
        Destroy(_activeDecoy, _lifetime);
    }
}
