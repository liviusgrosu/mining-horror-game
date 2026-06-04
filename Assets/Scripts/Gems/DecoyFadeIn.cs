using System.Collections;
using UnityEngine;

public class DecoyFadeIn : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] _renderers;
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private float _fadeOutDuration = 0.5f;

    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int EmissionMultiplierId = Shader.PropertyToID("_EmissionMultiplier");

    private MaterialPropertyBlock _block;

    private void Awake()
    {
        _block = new MaterialPropertyBlock();
        Apply(0f);
    }

    private void OnEnable()
    {
        StartCoroutine(FadeIn());
    }

    public void BeginLifecycle(float totalLifetime)
    {
        StartCoroutine(Lifecycle(totalLifetime));
    }

    private IEnumerator FadeIn()
    {
        var elapsed = 0f;
        while (elapsed < _fadeInDuration)
        {
            elapsed += Time.deltaTime;
            Apply(Mathf.Clamp01(elapsed / _fadeInDuration));
            yield return null;
        }
        Apply(1f);
    }

    private IEnumerator Lifecycle(float totalLifetime)
    {
        var hold = Mathf.Max(0f, totalLifetime - _fadeOutDuration);
        if (hold > 0f)
        {
            yield return new WaitForSeconds(hold);
        }

        var elapsed = 0f;
        while (elapsed < _fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            Apply(1f - Mathf.Clamp01(elapsed / _fadeOutDuration));
            yield return null;
        }
        Apply(0f);
        Destroy(gameObject);
    }

    private void Apply(float t)
    {
        foreach (var r in _renderers)
        {
            if (!r)
            {
                continue;
            }
            r.GetPropertyBlock(_block);
            _block.SetFloat(AlphaId, t);
            _block.SetFloat(EmissionMultiplierId, t);
            r.SetPropertyBlock(_block);
        }
    }
}
