using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject _upgradeText;
    [SerializeField] private GameObject _questionMarkIcon;
    [SerializeField] private GameObject _pickupIcon;
    
    public Dictionary<string, int> MineralCounts = new();

    public GameObject OverlayUI;
    public GameObject UpgradeUI;
    public GameObject GameOverScreen;
    
    public bool InMenu;

    public GameObject monster1Patrol, monster1Chase, monster2Chase;
    public GameObject rockBlockage1;
    private bool triggeredFirstChase, triggeredSecondChase;

    [SerializeField]
    private TextMeshProUGUI _entranceDoorText, _normalRockHoverText, _mineralDepositHoverText, _blockageRockHoverText;
    private bool DisplayingHoverText;

    public bool HasWon;

    [SerializeField] private CanvasGroup _mineralStatsCanvasGroup;
    private Coroutine _mineralStatsCoroutine;
    private float _mineralStatsTimer;
    private bool _mineralStatsVisible;

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

        if (_mineralStatsCanvasGroup != null)
            _mineralStatsCanvasGroup.alpha = 0f;
    }
    
    public void AddMineral(string mineral)
    {
        MineralCounts[mineral]++;
        GameObject.Find($"{mineral} Stat").GetComponentInChildren<TextMeshProUGUI>().text = MineralCounts[mineral].ToString();

        ShowMineralStats();

        if (mineral == "Gold" && !triggeredFirstChase)
        {
            monster1Patrol.SetActive(false);
            monster1Chase.SetActive(true);
            rockBlockage1.SetActive(true);
            triggeredFirstChase = true;
            ScreenShakeEffect.Instance.BeginShaking();
            OtherSFXManager.Instance.PlayEarthQuakeEffect();
        }
    }

    private void ShowMineralStats()
    {
        if (_mineralStatsCanvasGroup == null) return;

        const float displayDuration = 3f;

        if (_mineralStatsVisible)
        {
            // Already showing — just reset the timer, don't restart the fade
            _mineralStatsTimer = displayDuration;
            return;
        }

        if (_mineralStatsCoroutine != null)
            StopCoroutine(_mineralStatsCoroutine);

        _mineralStatsCoroutine = StartCoroutine(MineralStatsFadeRoutine(displayDuration));
    }

    private IEnumerator MineralStatsFadeRoutine(float displayDuration)
    {
        const float fadeDuration = 0.3f;

        // Fade in
        var elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _mineralStatsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        _mineralStatsCanvasGroup.alpha = 1f;
        _mineralStatsVisible = true;

        // Wait, using a timer that can be reset externally
        _mineralStatsTimer = displayDuration;
        while (_mineralStatsTimer > 0f)
        {
            _mineralStatsTimer -= Time.deltaTime;
            yield return null;
        }

        // Fade out
        _mineralStatsVisible = false;
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _mineralStatsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        _mineralStatsCanvasGroup.alpha = 0f;
        _mineralStatsCoroutine = null;
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
    
    public void TogglePickupIcon(bool state)
    {
        _pickupIcon.SetActive(state);
    }

    public void ToggleQuestionMark(bool state)
    {
        _questionMarkIcon.SetActive(state);
    }

    public void ToggleOffAllText()
    {
        _pickupIcon.SetActive(false);
        _upgradeText.SetActive(false);
        _questionMarkIcon.SetActive(false);
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
    
    public void ShowBlockageRockText()
    {
        if (DisplayingHoverText)
        {
            return;
        }

        DisplayingHoverText = true;
        // Start the fade coroutine
        StopAllCoroutines();
        StartCoroutine(FadeTextInAndOut(_blockageRockHoverText));
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
    
    public void WinGame()
    {
        HasWon = true;
        StartCoroutine(FadeOutAllAudio());
        /*var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            enemy.SetActive(false);
        }*/
    }

    private IEnumerator FadeOutAllAudio()
    {
        const float fadeDuration = 3f;
        var audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        var startVolumes = new float[audioSources.Length];

        for (int i = 0; i < audioSources.Length; i++)
        {
            startVolumes[i] = audioSources[i].volume;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            for (int i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i] != null)
                {
                    audioSources[i].volume = Mathf.Lerp(startVolumes[i], 0f, t);
                }
            }

            yield return null;
        }

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                audioSources[i].volume = 0f;
                audioSources[i].Stop();
            }
        }
    }
}
