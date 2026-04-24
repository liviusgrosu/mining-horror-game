using UnityEngine;

public class DecoyGem : MonoBehaviour
{
    [SerializeField] private float _lifetime = 10f;
    [SerializeField] private GameObject _decoyPrefab;
    [SerializeField] private float _spawnDistance = 2f;
    [SerializeField] private LayerMask _groundMask = ~0;

    private GameObject _activeDecoy;

    private void Update()
    {
        if (GameManager.Instance && (GameManager.Instance.InMenu || GameManager.Instance.HasDied))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnDecoy();
        }
    }

    private void SpawnDecoy()
    {
        if (_activeDecoy)
        {
            Destroy(_activeDecoy);
        }

        var cam = Camera.main.transform;
        var spawnPos = cam.position + cam.forward * _spawnDistance;

        if (Physics.Raycast(cam.position, cam.forward, out var hit, _spawnDistance))
        {
            spawnPos = hit.point;
        }

        if (Physics.Raycast(spawnPos, Vector3.down, out var groundHit, 50f, _groundMask))
        {
            spawnPos = groundHit.point;
        }

        _activeDecoy = Instantiate(_decoyPrefab, spawnPos, Quaternion.identity);
        Destroy(_activeDecoy, _lifetime);
    }
}
