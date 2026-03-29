using System;
using System.Collections;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private float[] stageHp;
    [SerializeField] private float powerRequirement;
    [SerializeField] private InventoryItem requiredGem;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private ParticleSystem smokeVFX;
    [SerializeField] private AudioClip rocksTumblingSound;

    public float PowerRequirement => powerRequirement;
    public InventoryItem RequiredGem => requiredGem;

    public Material CurrentStageMaterial
    {
        get
        {
            if (_currentStageIndex >= transform.childCount)
            {
                return null;
            }
            var renderer = transform.GetChild(_currentStageIndex).GetComponentInChildren<MeshRenderer>();
            return renderer != null ? renderer.sharedMaterial : null;
        }
    }

    private int _currentStageIndex;
    private float _currentHp;
    private Coroutine _shakeCoroutine;
    private AudioSource _audioSource;
    private Collider _collider;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == 0);
        }

        if (stageHp.Length > 0)
            _currentHp = stageHp[0];
    }

    public void TakeDamage(float damage = 1f)
    {
        _currentHp -= damage;
        if (_currentHp <= 0f)
        {
            AdvanceStage();
        }
        else
        {
            ShakeCurrentStage();
        }
    }

    private void ShakeCurrentStage()
    {
        if (_currentStageIndex >= transform.childCount) return;

        var stageTransform = transform.GetChild(_currentStageIndex);
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }
        _shakeCoroutine = StartCoroutine(Shake(stageTransform));
    }

    private IEnumerator Shake(Transform target)
    {
        var originalPos = target.localPosition;
        var elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            var offset = UnityEngine.Random.insideUnitSphere * shakeIntensity;
            target.localPosition = originalPos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
        _shakeCoroutine = null;
    }

    private void AdvanceStage()
    {
        if (_currentStageIndex < transform.childCount)
        {
            transform.GetChild(_currentStageIndex).gameObject.SetActive(false);
        }

        _currentStageIndex++;

        if (smokeVFX != null)
        {
            smokeVFX.Play();
        }

        if (_audioSource != null && rocksTumblingSound != null)
        {
            _audioSource.PlayOneShot(rocksTumblingSound);
        }
        
        if (_currentStageIndex >= transform.childCount)
        {
            _collider.enabled = false;
            return;
        }

        _currentHp = _currentStageIndex < stageHp.Length ? stageHp[_currentStageIndex] : 1f;
        transform.GetChild(_currentStageIndex).gameObject.SetActive(true);
        ShakeCurrentStage();


    }
}
