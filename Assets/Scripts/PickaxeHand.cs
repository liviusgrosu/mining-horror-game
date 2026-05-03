using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickaxeHand : MonoBehaviour
{
    public static PickaxeHand Instance;

    [SerializeField]
    private List<GameObject> _pickAxes;
    [SerializeField]
    private Transform _pickaxeParent;
    private GameObject _currentPickaxe;
    private int _pickaxeIndex = -1;

    private Animator _animator;
    private Transform _camera;

    private bool _hasPendingHit;
    private RaycastHit _pendingHit;

    [Header("Bob")]
    [SerializeField] private float _bobAmountY = 0.02f;
    [SerializeField] private float _bobAmountX = 0.01f;
    [SerializeField] private float _bobSmooth = 10f;
    [SerializeField] private float _walkBobMultiplier = 0.5f;
    [SerializeField] private float _crouchBobMultiplier = 0.3f;
    private float _bobTimer;
    private Vector3 _bobOffset;
    private Vector3 _initialLocalPosition;
    private CharacterController _playerController;
    private PlayerMovement _playerMovement;
    private CharacterFootsteps _playerFootsteps;

    [Header("Sway")]
    [SerializeField] private float _lookSwayAmount = 0.01f;
    [SerializeField] private float _lookSwayClamp = 0.08f;
    [SerializeField] private float _verticalSwayAmount = 0.02f;
    [SerializeField] private float _verticalSwayClamp = 0.15f;
    [SerializeField] private float _swaySmooth = 6f;
    private Vector3 _swayOffset;

    private Vector3 _lastNoisePosition;
    private float _lastNoiseRadius;

    [SerializeField] private GameObject sparkVFX, dustEffect, bloodVFX, lightBloodVFX, materialHitVFX, woodChipVFX;

    [Header("Runes")]
    [SerializeField] private InventoryItem deathRune;

    public LayerMask ignoreMask;

    [SerializeField]
    private float _hitRange = 5f;

    [Header("Noise")]
    [SerializeField] private float _miningNoiseRadius = 12f;

    private AudioSource _audioSource;
    private PickaxeAudio _pickaxeAudio;

    [SerializeField]
    public AudioClip pickaxeUpgradeSound;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _pickaxeAudio = GetComponent<PickaxeAudio>();
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _camera = Camera.main.transform;
        _initialLocalPosition = transform.localPosition;
        _playerController = GetComponentInParent<CharacterController>();
        _playerMovement = GetComponentInParent<PlayerMovement>();
        _playerFootsteps = GetComponentInParent<CharacterFootsteps>();
        SwitchPickaxe("Bronze Pickaxe");
    }

    void Update()
    {
        if (GameManager.Instance && (GameManager.Instance.InMenu || GameManager.Instance.HasDied))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _hasPendingHit = Physics.Raycast(_camera.position, _camera.forward, out _pendingHit, _hitRange, ~ignoreMask);
            if (_hasPendingHit)
            {
                _animator.SetTrigger("SwingHit");
            }
            else
            {
                _animator.SetTrigger("SwingMiss");
            }
        }
    }

    private void LateUpdate()
    {
        if (GameManager.Instance && (GameManager.Instance.InMenu || GameManager.Instance.HasDied))
        {
            return;
        }

        var isMoving = _playerController
            && _playerController.velocity.magnitude > 0.1f
            && _playerController.isGrounded;

        if (isMoving && _playerFootsteps)
        {
            var currentInterval = _playerMovement && _playerMovement.IsCrouching
                ? _playerFootsteps.crouchStepInterval
                : _playerMovement && _playerMovement.IsSprinting
                    ? _playerFootsteps.sprintStepInterval
                    : _playerFootsteps.stepInterval;

            _bobTimer += Time.deltaTime / currentInterval;
        }
        else
        {
            _bobTimer = 0f;
        }

        var bobMultiplier = _playerMovement && _playerMovement.IsCrouching
            ? _crouchBobMultiplier
            : _playerMovement && _playerMovement.IsSprinting
                ? 1f
                : _walkBobMultiplier;

        var bobY = Mathf.Sin(_bobTimer * Mathf.PI * 2f) * _bobAmountY * bobMultiplier;
        var bobX = Mathf.Cos(_bobTimer * Mathf.PI) * _bobAmountX * bobMultiplier;

        var mouseX = Input.GetAxisRaw("Mouse X");
        var mouseY = Input.GetAxisRaw("Mouse Y");
        var verticalVelocity = _playerController
            ? _playerController.velocity.y
            : 0f;

        var swayTargetX = Mathf.Clamp(-mouseX * _lookSwayAmount, -_lookSwayClamp, _lookSwayClamp);
        var swayTargetY = Mathf.Clamp(-mouseY * _lookSwayAmount, -_lookSwayClamp, _lookSwayClamp)
            + Mathf.Clamp(-verticalVelocity * _verticalSwayAmount, -_verticalSwayClamp, _verticalSwayClamp);
        var swayTarget = new Vector3(swayTargetX, swayTargetY, 0f);
        _swayOffset = Vector3.Lerp(_swayOffset, swayTarget, _swaySmooth * Time.deltaTime);

        var bobTarget = new Vector3(bobX, bobY, 0f);
        _bobOffset = Vector3.Lerp(_bobOffset, bobTarget, _bobSmooth * Time.deltaTime);

        transform.localPosition = _initialLocalPosition + _bobOffset + _swayOffset;
    }

    public void SwitchPickaxe(string name)
    {
        var chosenPickaxe = _pickAxes.Find(pickaxe => pickaxe.name == name);
        Destroy(_currentPickaxe);
        _currentPickaxe = Instantiate(chosenPickaxe, _pickaxeParent, false);
        _currentPickaxe.transform.localPosition = chosenPickaxe.transform.localPosition;
        _currentPickaxe.transform.localRotation = chosenPickaxe.transform.localRotation;
    }

    public void CheckHit()
    {
        if (_hasPendingHit)
        {
            var hit = _pendingHit;
            _hasPendingHit = false;
            if (hit.collider.CompareTag("VoxelTerrain"))
            {
                var voxelTerrain = hit.collider.GetComponentInParent<VoxelTerrain>();
                if (voxelTerrain != null)
                {
                    voxelTerrain.Mine(hit.point);
                }

                _lastNoisePosition = hit.point;
                _lastNoiseRadius = _miningNoiseRadius;
                NoiseEmitter.Emit(hit.point, _miningNoiseRadius, hit.collider.tag);
                _pickaxeAudio.PlayImpactForTag(hit.collider.tag);
                SpawnCloudEffect(hit.point);
                var voxelRenderer = hit.collider.GetComponent<MeshRenderer>();
                if (voxelRenderer != null)
                    SpawnMaterialHitEffect(hit.point, voxelRenderer.sharedMaterial);
            }
            else if (hit.collider.CompareTag("Destructible"))
            {
                var destructible = hit.collider.GetComponentInParent<Destructible>();
                var canDamage = destructible != null
                    && _currentPickaxe.GetComponent<Pickaxe>().Power >= destructible.PowerRequirement
                    && (destructible.RequiredGem == null || Inventory.Instance.PickaxeGems.Contains(destructible.RequiredGem));

                if (canDamage)
                {
                    var mat = destructible.CurrentStageMaterial;
                    destructible.TakeDamage();
                    _lastNoisePosition = hit.point;
                    _lastNoiseRadius = _miningNoiseRadius;
                    NoiseEmitter.Emit(hit.point, _miningNoiseRadius, hit.collider.tag);
                    _pickaxeAudio.PlayImpactForTag(hit.collider.tag);
                    SpawnCloudEffect(hit.point);
                    if (mat != null)
                    {
                        SpawnMaterialHitEffect(hit.point, mat);
                    }
                }
                else
                {
                    _pickaxeAudio.PlayImpactForTag(hit.collider.tag);
                    SpawnSparkEffect(hit.point, hit.normal);
                }
            }
            else if (hit.collider.CompareTag("Chain"))
            {
                var chandelier = hit.collider.GetComponentInParent<ChandelierBreakable>();
                if (chandelier)
                {
                    chandelier.Break();
                }
                _pickaxeAudio.PlayImpactForTag(hit.collider.tag);
                SpawnSparkEffect(hit.point, hit.normal);
            }
            else if (hit.collider.CompareTag("Enemy"))
            {
                var hasDeathRune = deathRune != null && Inventory.Instance.PickaxeGems.Contains(deathRune);
                if (hasDeathRune)
                {
                    var enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                    if (enemyHealth)
                    {
                        enemyHealth.TakeDamage(_currentPickaxe.GetComponent<Pickaxe>().Power * 10);
                    }
                    SpawnBloodEffect(hit.point, hit.normal);
                }
                else
                {
                    SpawnLightBloodEffect(hit.point, hit.normal);
                }
            }
            else
            {
                _pickaxeAudio.PlayImpactForTag(hit.collider.tag);
                _lastNoisePosition = hit.point;
                _lastNoiseRadius = _miningNoiseRadius;
                NoiseEmitter.Emit(hit.point, _miningNoiseRadius, hit.collider.tag);
                var tag = hit.collider.tag;
                if (tag == "Wood")
                {
                    SpawnWoodChipEffect(hit.point, hit.normal);
                }
                else if (tag != "Grass" && tag != "Carpet")
                {
                    SpawnSparkEffect(hit.point, hit.normal);
                }
            }
        }
        else
        {
            _pickaxeAudio.PlayMiss();
        }
    }

    private void SpawnCloudEffect(Vector3 point)
    {
        var vfx = Instantiate(dustEffect, point, Quaternion.identity);
        Destroy(vfx, 1f);
    }

    private void SpawnSparkEffect(Vector3 point, Vector3 normal)
    {
        var vfx = Instantiate(sparkVFX, point, Quaternion.LookRotation(normal));
        Destroy(vfx, 1f);
    }

    private void SpawnWoodChipEffect(Vector3 point, Vector3 normal)
    {
        if (!woodChipVFX)
        {
            return;
        }
        var vfx = Instantiate(woodChipVFX, point, Quaternion.LookRotation(normal));
        Destroy(vfx, 1f);
    }

    private void SpawnMaterialHitEffect(Vector3 point, Material mat)
    {
        var vfx = Instantiate(materialHitVFX, point, Quaternion.identity);
        for (var i = 1; i <= 3; i++)
        {
            var gibble = vfx.transform.Find("Gibble " + i);
            if (gibble != null)
            {
                var renderer = gibble.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = mat;
                }
            }
        }
        Destroy(vfx, 2f);
    }

    private void SpawnBloodEffect(Vector3 point, Vector3 normal)
    {
        var vfx = Instantiate(bloodVFX, point, Quaternion.LookRotation(normal));
        Destroy(vfx, 1f);
    }

    private void SpawnLightBloodEffect(Vector3 point, Vector3 normal)
    {
        var vfx = Instantiate(lightBloodVFX, point, Quaternion.LookRotation(normal));
        Destroy(vfx, 1f);
    }

    public void PlayUpgradePickupSound()
    {
        _audioSource.PlayOneShot(pickaxeUpgradeSound);
    }

    private void OnDrawGizmos()
    {
        if (_lastNoiseRadius <= 0f)
        {
            return;
        }
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(_lastNoisePosition, _lastNoiseRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(_lastNoisePosition, _lastNoiseRadius);
    }
}
