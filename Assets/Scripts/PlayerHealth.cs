using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct HealthStatus
{
    public Sprite sprite;
    public string text;
}

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;
    
    public int MaxHealth = 100;
    private int _currentHealth;

    [SerializeField] private Image _damageVignette;

    [Header("Consumables")]
    [SerializeField] private InventoryItem _healthBottle;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] gettingHitSFX;
    [SerializeField] private AudioClip healthBottleUseSFX;
    private AudioSource _audioSource;

    [Header("Health Status UI")]
    [SerializeField] private Image _healthStatusImage;
    [SerializeField] private TMP_Text _healthStatusText;
    [SerializeField] private HealthStatus _healthy;
    [SerializeField] private HealthStatus _damaged;
    [SerializeField] private HealthStatus _dying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        _currentHealth = MaxHealth;
        UpdateVignette();
        UpdateHealthStatus();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(10);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Heal(10);
        }
        
        if (Input.GetKeyDown(KeyCode.H) && !GameManager.Instance.IsPaused)
        {
            UseHealthBottle();
        }
    }

    public void UseHealthBottle()
    {
        if (!_healthBottle || _currentHealth >= MaxHealth)
        {
            return;
        }

        if (Inventory.Instance.GetCount(_healthBottle) <= 0)
        {
            return;
        }

        Heal(25);
        Inventory.Instance.Remove(_healthBottle, 1);

        if (healthBottleUseSFX && _audioSource)
        {
            _audioSource.PlayOneShot(healthBottleUseSFX);
        }
    }

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(_currentHealth - amount, 0);
        UpdateVignette();
        UpdateHealthStatus();
        PlayHitSound();
        if (ScreenShakeEffect.Instance) ScreenShakeEffect.Instance.ShakeOnce(0.15f, 0.05f);

        if (_currentHealth <= 0)
        {
            GameManager.Instance.OpenGameOverScreen();
            enabled = false;
        }
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
        UpdateVignette();
        UpdateHealthStatus();
    }

    private void PlayHitSound()
    {
        if (gettingHitSFX.Length == 0 || !_audioSource)
        {
            return;
        }
        _audioSource.PlayOneShot(gettingHitSFX[Random.Range(0, gettingHitSFX.Length)]);
    }

    private void UpdateHealthStatus()
    {
        if (!_healthStatusImage)
        {
            return;
        }

        var status = _currentHealth switch
        {
            >= 66 => _healthy,
            >= 33 => _damaged,
            _ => _dying
        };

        _healthStatusImage.sprite = status.sprite;
        if (_healthStatusText)
        {
            _healthStatusText.text = status.text;
        }
    }

    private void UpdateVignette()
    {
        if (!_damageVignette)
        {
            return;
        }

        var healthPercent = (float)_currentHealth / MaxHealth;
        var alpha = healthPercent >= 0.5f ? 0f : Mathf.Lerp(0.8f, 0f, healthPercent / 0.5f);
        _damageVignette.color = new Color(1f, 1f, 1f, alpha);
    }
}
