using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int MaxHealth = 100;
    private int _currentHealth;

    [SerializeField] private Image _damageVignette;

    private void Start()
    {
        _currentHealth = MaxHealth;
        UpdateVignette();
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
    }

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(_currentHealth - amount, 0);
        UpdateVignette();

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
