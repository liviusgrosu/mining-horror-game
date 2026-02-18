using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject _pickupText;
    [SerializeField] private GameObject _upgradeText;
    
    public Dictionary<string, int> MineralCounts = new();

    public GameObject OverlayUI;
    public GameObject UpgradeUI;

    public bool InMenu;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        MineralCounts.Add("Copper", 0);
        MineralCounts.Add("Silver", 0);
        MineralCounts.Add("Gold", 0);
    }

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void AddMineral(string mineral)
    {
        MineralCounts[mineral]++;
        GameObject.Find($"{mineral} Stat").GetComponentInChildren<TextMeshProUGUI>().text = MineralCounts[mineral].ToString();
    }
    
    public void TogglePickupText(bool state)
    {
        _pickupText.SetActive(state);
    }

    public void ToggleUpgradeText(bool state)
    {
        _upgradeText.SetActive(state);
    }

    public void OpenUpgradeUI()
    {
        ToggleCursorLock(true);
        OverlayUI.SetActive(false);
        UpgradeUI.SetActive(true);
        InMenu = true;
    }
    
    public void CloseUpgradeUI()
    {
        ToggleCursorLock(false);
        OverlayUI.SetActive(true);
        UpgradeUI.SetActive(false);
        InMenu = false;
    }

    private void ToggleCursorLock(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }
}
