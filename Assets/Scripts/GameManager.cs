using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject _upgradeText;
    [SerializeField] private GameObject _questionMarkIcon;
    [SerializeField] private GameObject _pickupIcon;
    
    public GameObject OverlayUI;
    public GameObject UpgradeUI;
    public GameObject GameOverScreen;
    
    public bool InMenu;
    
    private bool triggeredFirstChase, triggeredSecondChase;

    [SerializeField]
    private TextMeshProUGUI _entranceDoorText, _normalRockHoverText, _mineralDepositHoverText, _blockageRockHoverText;
    private bool DisplayingHoverText;

    public bool HasWon, HasDied;
    public bool IsPaused;

    [SerializeField] private GameObject _controlsOverlay;
    [SerializeField] private GameObject _inventoryUI;

    [SerializeField] private CanvasGroup _mineralStatsCanvasGroup;
    private Coroutine _mineralStatsCoroutine;
    private float _mineralStatsTimer;
    private bool _mineralStatsVisible;

    private GameObject player;

    [SerializeField] private Volume _deathPostProcessVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !HasWon && !HasDied)
        {
            if (InMenu)
            {
                CloseUpgradeUI();
                return;
            }

            ToggleInventory();
        }
    }

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_mineralStatsCanvasGroup != null)
            _mineralStatsCanvasGroup.alpha = 0f;
        
        player = GameObject.Find("Player");
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
        player.GetComponent<CharacterController>().height = 0.1f;
        player.GetComponent<CapsuleCollider>().height = 0.1f;

        StartCoroutine(DeathBlurRoutine());

        ToggleCursorLock(true);
        GameOverScreen.SetActive(true);
        UpgradeUI.SetActive(false);
        OverlayUI.SetActive(true);

        HasDied = true;
    }

    private IEnumerator DeathBlurRoutine()
    {
        if (_deathPostProcessVolume == null) yield break;

        if (!_deathPostProcessVolume.profile.TryGet(out DepthOfField dof))
            yield break;

        _deathPostProcessVolume.gameObject.SetActive(true);

        const float startFocusDistance = 5f;
        const float blurDuration = 1f;

        dof.mode.Override(DepthOfFieldMode.Bokeh);
        dof.focusDistance.Override(startFocusDistance);

        float elapsed = 0f;

        while (elapsed < blurDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blurDuration;
            dof.focusDistance.Override(Mathf.Lerp(startFocusDistance, 0f, t));
            yield return null;
        }

        dof.focusDistance.Override(0f);
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;

        if (_controlsOverlay)
        {
            _controlsOverlay.SetActive(IsPaused);
        }

        ToggleCursorLock(IsPaused);
        Time.timeScale = IsPaused ? 0f : 1f;
    }

    public void ToggleInventory()
    {
        IsPaused = !IsPaused;

        if (_inventoryUI)
        {
            _inventoryUI.SetActive(IsPaused);
        }

        if (IsPaused && PickupNotification.Instance)
        {
            PickupNotification.Instance.ClearAll();
        }

        ToggleCursorLock(IsPaused);
        Time.timeScale = IsPaused ? 0f : 1f;
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
