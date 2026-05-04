using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InvisibilityGem : MonoBehaviour
{
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _fadeInTime = 0.5f;
    [SerializeField] private float _fadeOutTime = 0.5f;
    [SerializeField] private float _visibilityReduction;
    [SerializeField] private float _transparentAlpha = 0.3f;
    [SerializeField] private ParticleSystem _activationVFX;
    [SerializeField] private Renderer[] _exemptRenderers;

    private float _timer;
    private bool _isActive;
    private readonly List<MaterialState> _savedStates = new();

    private struct MaterialState
    {
        public Material Material;
        public float SrcBlend;
        public float DstBlend;
        public float ZWrite;
        public float Surface;
        public int RenderQueue;
        public Color BaseColor;
    }

    private void Update()
    {
        if (GameManager.Instance && (GameManager.Instance.InMenu || GameManager.Instance.HasDied))
        {
            return;
        }

        var gemUI = GemSelectionUI.Instance;
        if (!gemUI || gemUI.IsOpen || gemUI.SelectedGem != GemType.Invisibility)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!GemSelectionUI.Instance.TryConsumeUse(GemType.Invisibility))
            {
                return;
            }

            _timer = _duration;

            if (_activationVFX)
            {
                _activationVFX.Play(true);
            }

            if (!_isActive)
            {
                StartCoroutine(ActivateInvisibility());
            }
        }
    }

    private IEnumerator ActivateInvisibility()
    {
        _isActive = true;
        _savedStates.Clear();

        var renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in renderers)
        {
            if (!meshRenderer)
            {
                continue;
            }

            if (System.Array.IndexOf(_exemptRenderers, meshRenderer) >= 0)
            {
                continue;
            }

            foreach (var mat in meshRenderer.materials)
            {
                var state = new MaterialState
                {
                    Material = mat,
                    SrcBlend = mat.GetFloat("_SrcBlend"),
                    DstBlend = mat.GetFloat("_DstBlend"),
                    ZWrite = mat.GetFloat("_ZWrite"),
                    Surface = mat.GetFloat("_Surface"),
                    RenderQueue = mat.renderQueue,
                    BaseColor = mat.GetColor("_BaseColor")
                };
                _savedStates.Add(state);

                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                mat.renderQueue = (int)RenderQueue.Transparent;
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
        }

        yield return StartCoroutine(LerpAlpha(1f, _transparentAlpha, _fadeInTime));

        if (PlayerVisibility.Instance)
        {
            PlayerVisibility.Instance.VisibilityMultiplier = _visibilityReduction;
        }

        while (_timer > 0f)
        {
            _timer -= Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(LerpAlpha(_transparentAlpha, 1f, _fadeOutTime));

        if (PlayerVisibility.Instance)
        {
            PlayerVisibility.Instance.VisibilityMultiplier = 1f;
        }

        foreach (var state in _savedStates)
        {
            RestoreMaterial(state);
        }
        _savedStates.Clear();

        _isActive = false;
    }

    private IEnumerator LerpAlpha(float from, float to, float time)
    {
        var elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / time);
            var alpha = Mathf.Lerp(from, to, t);

            foreach (var state in _savedStates)
            {
                var color = state.BaseColor;
                color.a = alpha;
                state.Material.SetColor("_BaseColor", color);
            }

            yield return null;
        }
    }

    private void RestoreMaterial(MaterialState state)
    {
        var mat = state.Material;
        mat.SetFloat("_Surface", state.Surface);
        mat.SetFloat("_SrcBlend", state.SrcBlend);
        mat.SetFloat("_DstBlend", state.DstBlend);
        mat.SetFloat("_ZWrite", state.ZWrite);
        mat.renderQueue = state.RenderQueue;
        mat.SetColor("_BaseColor", state.BaseColor);

        if (state.Surface < 0.5f)
        {
            mat.SetOverrideTag("RenderType", "Opaque");
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
    }
}
