using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    public int Power;

    [SerializeField] private Renderer _gemIndicator;

    private void OnEnable()
    {
        if (GemSelectionUI.Instance)
        {
            GemSelectionUI.Instance.OnSelectedGemChanged += ApplyGemMaterial;
            ApplyGemMaterial(GemSelectionUI.Instance.SelectedGem);
        }
    }

    private void OnDisable()
    {
        if (GemSelectionUI.Instance)
        {
            GemSelectionUI.Instance.OnSelectedGemChanged -= ApplyGemMaterial;
        }
    }

    private void ApplyGemMaterial(GemType type)
    {
        if (!_gemIndicator || !GemSelectionUI.Instance)
        {
            return;
        }
        var mat = GemSelectionUI.Instance.GetMaterialFor(type);
        if (mat)
        {
            _gemIndicator.material = mat;
        }
    }
}
