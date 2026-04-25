using UnityEngine;
using UnityEngine.UI;

public enum GemType { None, Invisibility, Dampening, Decoy }

public class GemSelectionUI : MonoBehaviour
{
    public static GemSelectionUI Instance { get; private set; }

    [SerializeField] private GameObject _gemScreen;
    [SerializeField] private Button _invisibilityButton;
    [SerializeField] private Button _dampenButton;
    [SerializeField] private Button _decoyButton;
    [SerializeField] private Image _invisibilityRing;
    [SerializeField] private Image _dampenRing;
    [SerializeField] private Image _decoyRing;

    public GemType SelectedGem { get; private set; } = GemType.None;
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _gemScreen.SetActive(false);
        _invisibilityButton.onClick.AddListener(() => SelectGem(GemType.Invisibility));
        _dampenButton.onClick.AddListener(() => SelectGem(GemType.Dampening));
        _decoyButton.onClick.AddListener(() => SelectGem(GemType.Decoy));
        RefreshRings();
    }

    public void OpenScreen()
    {
        IsOpen = true;
        _gemScreen.SetActive(true);
    }

    public void CloseScreen()
    {
        IsOpen = false;
        _gemScreen.SetActive(false);
    }

    private void SelectGem(GemType type)
    {
        SelectedGem = type;
        RefreshRings();
    }

    private void RefreshRings()
    {
        _invisibilityRing.enabled = SelectedGem == GemType.Invisibility;
        _dampenRing.enabled = SelectedGem == GemType.Dampening;
        _decoyRing.enabled = SelectedGem == GemType.Decoy;
    }
}
