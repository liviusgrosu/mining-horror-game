using System.Collections;
using UnityEngine;

public class NoiseDebugCube : MonoBehaviour
{
    [SerializeField] private float _flashDuration = 0.2f;
    [SerializeField] private string _surfaceTagMask = "Ground";
    
    private Renderer _renderer;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = Color.red;
    }

    private void OnEnable()
    {
        NoiseEmitter.OnNoise += HandleNoise;
    }

    private void OnDisable()
    {
        NoiseEmitter.OnNoise -= HandleNoise;
    }

    private void HandleNoise(Vector3 position, float radius, string surfaceTag)
    {
        if (Vector3.Distance(transform.position, position) > radius || surfaceTag != _surfaceTagMask)
        {
            return;
        }

        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        _renderer.material.color = Color.green;
        yield return new WaitForSeconds(_flashDuration);
        _renderer.material.color = Color.red;
        _flashCoroutine = null;
    }
}
