using System.Collections;
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
    public GameObject GameOverScreen;
    
    public bool InMenu;

    public GameObject monster1Patrol, monster1Chase, monster2Chase;
    public GameObject rockBlockage1;
    private bool triggeredFirstChase, triggeredSecondChase;

    [SerializeField]
    private TextMeshProUGUI _entranceDoorText, _normalRockHoverText, _mineralDepositHoverText;
    private bool DisplayingHoverText;
    
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

        if (mineral == "Gold" && !triggeredFirstChase)
        {
            monster1Patrol.SetActive(false);
            monster1Chase.SetActive(true);
            rockBlockage1.SetActive(true);
            triggeredFirstChase = true;
            ScreenShakeEffect.Instance.BeginShaking();
        }
    }

    public void SpawnFinalEncounter()
    {
        if (triggeredSecondChase)
        {
            return;
        }
        monster1Chase.SetActive(false);
        monster2Chase.SetActive(true);
        triggeredSecondChase = true;
    }
    
    public void TogglePickupText(bool state)
    {
        _pickupText.SetActive(state);
    }

    public void ToggleUpgradeText(bool state)
    {
        _upgradeText.SetActive(state);
    }

    public void ToggleOffAllText()
    {
        _pickupText.SetActive(false);
        _upgradeText.SetActive(false);   
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

    public void OpenGameOverScreen()
    {
        ToggleCursorLock(true);
        GameOverScreen.SetActive(true);
        UpgradeUI.SetActive(false);
        OverlayUI.SetActive(true);
    }

    private void ToggleCursorLock(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }
    
    public void ShowEntranceDoorText()
    {
        if (DisplayingHoverText)
        {
            return;
        }

        DisplayingHoverText = true;
        // Start the fade coroutine
        StopAllCoroutines();
        StartCoroutine(FadeTextInAndOut(_entranceDoorText));
    }

    public void ShowNormalRockHoverText()
    {
        if (DisplayingHoverText)
        {
            return;
        }

        DisplayingHoverText = true;
        StopAllCoroutines();
        StartCoroutine(FadeTextInAndOut(_normalRockHoverText));
    }
    
    public void ShowMineralDepositHoverText()
    {
        if (DisplayingHoverText)
        {
            return;
        }

        DisplayingHoverText = true;
        StopAllCoroutines();
        StartCoroutine(FadeTextInAndOut(_mineralDepositHoverText));
    }

    private IEnumerator FadeTextInAndOut(TextMeshProUGUI text)
    {
        const float fadeDuration = 0.25f; 
        const float pauseDuration = 2f; 
    
        text.gameObject.SetActive(true);
        var originalColor = text.color;
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    
        var elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            var alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    
        yield return new WaitForSeconds(pauseDuration);
    
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            var alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        text.gameObject.SetActive(false);
        DisplayingHoverText = false;
    }
}
