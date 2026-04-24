using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InvisibilityGem : MonoBehaviour
{
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _visibilityReduction = 0.25f;
    [SerializeField] private float _transparentAlpha = 0.3f;
    [SerializeField] private List<Renderer> _excludeRenderers;

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

        if (Input.GetKeyDown(KeyCode.Alpha1) && !_isActive)
        {
            StartCoroutine(ActivateInvisibility());
        }
    }

    private IEnumerator ActivateInvisibility()
    {
        _isActive = true;
        _savedStates.Clear();

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (!renderer || _excludeRenderers.Contains(renderer))
            {
                continue;
            }

            foreach (var mat in renderer.materials)
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

                SetMaterialTransparent(mat);
            }
        }

        if (PlayerVisibility.Instance)
        {
            PlayerVisibility.Instance.VisibilityMultiplier = _visibilityReduction;
        }

        yield return new WaitForSeconds(_duration);

        foreach (var state in _savedStates)
        {
            RestoreMaterial(state);
        }
        _savedStates.Clear();

        if (PlayerVisibility.Instance)
        {
            PlayerVisibility.Instance.VisibilityMultiplier = 1f;
        }

        _isActive = false;
    }

    private void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.renderQueue = (int)RenderQueue.Transparent;
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        var color = mat.GetColor("_BaseColor");
        color.a = _transparentAlpha;
        mat.SetColor("_BaseColor", color);
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
