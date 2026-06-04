using System.Collections.Generic;
using UnityEngine;

public class ChandelierBreakable : MonoBehaviour
{
    [SerializeField] private GameObject _staticChandelier;
    [SerializeField] private GameObject _fallingChandelierPrefab;

    [Header("Feedback")]
    [SerializeField] private GameObject _breakVfxPrefab;
    [SerializeField] private AudioClip _breakSfx;

    private readonly List<GameObject> _chains = new();
    private bool _broken;

    private void Awake()
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("Chain"))
            {
                _chains.Add(t.gameObject);
            }
        }
    }

    public void Break()
    {
        if (_broken)
        {
            return;
        }
        _broken = true;

        var pose = _staticChandelier
            ? _staticChandelier.transform
            : transform;
        var pos = pose.position;
        var rot = pose.rotation;

        if (_breakVfxPrefab)
        {
            Instantiate(_breakVfxPrefab, pos, rot);
        }
        if (_breakSfx)
        {
            AudioSource.PlayClipAtPoint(_breakSfx, pos);
        }

        foreach (var chain in _chains)
        {
            if (chain)
            {
                Destroy(chain);
            }
        }

        if (_staticChandelier)
        {
            Destroy(_staticChandelier);
        }
        if (_fallingChandelierPrefab)
        {
            Instantiate(_fallingChandelierPrefab, pos, rot);
        }
    }
}
