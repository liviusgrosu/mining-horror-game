using UnityEngine;

public class WispSpawner : MonoBehaviour
{
    [SerializeField] private Wisp _prefab;
    [SerializeField] private EnemyPathing _pathing;
    [SerializeField] private float _spawnInterval = 5f;
    [SerializeField] private float _initialDelay;

    private void Start()
    {
        InvokeRepeating(nameof(Spawn), _initialDelay, _spawnInterval);
    }

    private void Spawn()
    {
        if (!_prefab || !_pathing)
        {
            return;
        }
        var instance = Instantiate(_prefab);
        instance.SetPath(_pathing);
    }
}
