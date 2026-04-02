using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickaxeHand : MonoBehaviour
{
    public static PickaxeHand Instance;
    
    [SerializeField]
    private List<GameObject> _pickAxes;
    private GameObject _currentPickaxe;
    private int _pickaxeIndex = -1;
    
    private Animator _animator;
    private Transform _camera;

    [SerializeField] private GameObject sparkVFX, dustEffect, bloodVFX, materialHitVFX;
    
    public LayerMask ignoreMask;
    
    private AudioSource _audioSource;
    
    [SerializeField]
    public AudioClip pickaxeValidSound;

    [SerializeField]
    public AudioClip pickaxeInvalidSound;

    [SerializeField] 
    public AudioClip pickaxeUpgradeSound;
    
    [SerializeField] 
    public AudioClip pickaxeMissSound;
    
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
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _camera = Camera.main.transform;
        SwitchPickaxe("Bronze Pickaxe");    
    }

    void Update()
    {
        if (GameManager.Instance && GameManager.Instance.InMenu)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger("Swing");
        }
    }

    public void SwitchPickaxe(string name)
    {
        var chosenPickaxe = _pickAxes.Find(pickaxe => pickaxe.name == name);
        Destroy(_currentPickaxe);
        _currentPickaxe = Instantiate(chosenPickaxe, transform.position, transform.rotation);
        _currentPickaxe.transform.SetParent(transform);
    }

    public void CheckHit()
    {
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out var hit, 5.0f, ~ignoreMask))
        {
            if (hit.collider.CompareTag("VoxelTerrain"))
            {
                var voxelTerrain = hit.collider.GetComponentInParent<VoxelTerrain>();
                if (voxelTerrain != null)
                {
                    voxelTerrain.Mine(hit.point);
                }

                _audioSource.PlayOneShot(pickaxeValidSound);
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
                    _audioSource.PlayOneShot(pickaxeValidSound);
                    SpawnCloudEffect(hit.point);
                    if (mat != null)
                    {
                        SpawnMaterialHitEffect(hit.point, mat);
                    }
                }
                else
                {
                    _audioSource.PlayOneShot(pickaxeInvalidSound);
                    SpawnSparkEffect(hit.point, hit.normal);
                }
            }
            else if (hit.collider.CompareTag("Enemy"))
            {
                var shade = hit.collider.GetComponentInParent<ZombieBehaviour>();
                if (shade != null)
                {
                    shade.TakeDamage(_currentPickaxe.GetComponent<Pickaxe>().Power * 10);
                }
                _audioSource.PlayOneShot(pickaxeValidSound);
                SpawnBloodEffect(hit.point, hit.normal);
            }
            else
            {
                _audioSource.PlayOneShot(pickaxeInvalidSound);
                SpawnSparkEffect(hit.point, hit.normal);
            }
        }
        else
        {
            _audioSource.PlayOneShot(pickaxeMissSound);
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
                    renderer.material = mat;
            }
        }
        Destroy(vfx, 2f);
    }

    private void SpawnBloodEffect(Vector3 point, Vector3 normal)
    {
        var vfx = Instantiate(bloodVFX, point, Quaternion.LookRotation(normal));
        Destroy(vfx, 1f);
    }

    public void PlayUpgradePickupSound()
    {
        _audioSource.PlayOneShot(pickaxeUpgradeSound);
    }
}
