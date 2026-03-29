using System.Collections;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private int stageHp = 3;
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
            if (_currentStageIndex >= _stageCount) return null;
            var target = GetStageTransform(_currentStageIndex);
            var renderer = target.GetComponent<MeshRenderer>();
            return renderer != null ? renderer.sharedMaterial : null;
        }
    }

    private int _stageCount;
    private bool _hasChildren;
    private int _currentStageIndex;
    private int _currentHp;
    private Coroutine _shakeCoroutine;
    private AudioSource _audioSource;
    private Collider _collider;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider>();
        _hasChildren = transform.childCount > 0;
        _stageCount = _hasChildren ? transform.childCount : 1;
    }

    private void Start()
    {
        if (_hasChildren)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                SetStageVisible(i, i == 0);
            }
        }

        _currentHp = stageHp;
    }

    public void TakeDamage(float damage = 1f)
    {
        _currentHp -= (int)damage;
        if (_currentHp <= 0)
        {
            AdvanceStage();
        }
        else
        {
            ShakeCurrentStage();
        }
    }

    private Transform GetStageTransform(int index)
    {
        return _hasChildren ? transform.GetChild(index) : transform;
    }

    private void SetStageVisible(int index, bool visible)
    {
        var target = GetStageTransform(index);
        var meshRenderer = target.GetComponent<MeshRenderer>();
        if (meshRenderer) meshRenderer.enabled = visible;
    }

    private void ShakeCurrentStage()
    {
        if (_currentStageIndex >= _stageCount) return;

        var stageTransform = GetStageTransform(_currentStageIndex);
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
            var offset = Random.insideUnitSphere * shakeIntensity;
            target.localPosition = originalPos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
        _shakeCoroutine = null;
    }

    private void AdvanceStage()
    {
        SetStageVisible(_currentStageIndex, false);
        _currentStageIndex++;

        if (smokeVFX != null)
        {
            smokeVFX.Play();
        }

        if (_audioSource != null && rocksTumblingSound != null)
        {
            _audioSource.PlayOneShot(rocksTumblingSound);
        }

        if (_currentStageIndex >= _stageCount)
        {
            _collider.enabled = false;
            return;
        }

        _currentHp = stageHp;
        SetStageVisible(_currentStageIndex, true);
        ShakeCurrentStage();
    }
}
