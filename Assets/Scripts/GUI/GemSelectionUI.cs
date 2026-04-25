using TMPro;
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
    [SerializeField] private TMP_Text _invisibilityCountText;
    [SerializeField] private TMP_Text _dampenCountText;
    [SerializeField] private TMP_Text _decoyCountText;

    [SerializeField]
    private int maxUses = 10;

    private int _invisibilityUses;
    private int _dampenUses;
    private int _decoyUses;

    public GemType SelectedGem { get; private set; } = GemType.None;
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        Instance = this;
        _invisibilityUses = _dampenUses = _decoyUses = maxUses;
    }

    private void Start()
    {
        _gemScreen.SetActive(false);
        _invisibilityButton.onClick.AddListener(() => SelectGem(GemType.Invisibility));
        _dampenButton.onClick.AddListener(() => SelectGem(GemType.Dampening));
        _decoyButton.onClick.AddListener(() => SelectGem(GemType.Decoy));
        RefreshRings();
        RefreshUsageUI();
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

    public bool TryConsumeUse(GemType type)
    {
        switch (type)
        {
            case GemType.Invisibility:
                if (_invisibilityUses <= 0) { return false; }
                _invisibilityUses--;
                if (_invisibilityUses == 0 && SelectedGem == GemType.Invisibility) { SelectedGem = GemType.None; }
                break;
            case GemType.Dampening:
                if (_dampenUses <= 0) { return false; }
                _dampenUses--;
                if (_dampenUses == 0 && SelectedGem == GemType.Dampening) { SelectedGem = GemType.None; }
                break;
            case GemType.Decoy:
                if (_decoyUses <= 0) { return false; }
                _decoyUses--;
                if (_decoyUses == 0 && SelectedGem == GemType.Decoy) { SelectedGem = GemType.None; }
                break;
            default:
                return false;
        }

        RefreshRings();
        RefreshUsageUI();
        return true;
    }

    public void RestoreUse(GemType type)
    {
        switch (type)
        {
            case GemType.Invisibility:
                _invisibilityUses = Mathf.Min(_invisibilityUses + 1, maxUses);
                break;
            case GemType.Dampening:
                _dampenUses = Mathf.Min(_dampenUses + 1, maxUses);
                break;
            case GemType.Decoy:
                _decoyUses = Mathf.Min(_decoyUses + 1, maxUses);
                break;
        }
        RefreshRings();
        RefreshUsageUI();
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

    private void RefreshUsageUI()
    {
        _invisibilityCountText.text = _invisibilityUses.ToString();
        _dampenCountText.text = _dampenUses.ToString();
        _decoyCountText.text = _decoyUses.ToString();

        _invisibilityButton.interactable = _invisibilityUses > 0;
        _dampenButton.interactable = _dampenUses > 0;
        _decoyButton.interactable = _decoyUses > 0;
    }
}
