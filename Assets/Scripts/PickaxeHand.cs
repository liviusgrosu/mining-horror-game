using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
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

    [SerializeField] private GameObject sparkVFX, dustEffect;
    
    public LayerMask ignoreMask;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _camera = Camera.main.transform;
        SwitchPickaxe("Bronze Pickaxe");    
    }

    void Update()
    {
        if (GameManager.Instance.InMenu)
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

        if (name == "Gold Pickaxe")
        {
            GameManager.Instance.SpawnFinalEncounter();
        }
    }

    public void CheckHit()
    {
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out var hit, 5.0f, ~ignoreMask))
        {
            Debug.Log(hit.transform.name);
            if (hit.collider.CompareTag("Mineral Deposit"))
            {
                var wp = hit.collider.GetComponent<MineralDeposit>();
                if (wp != null)
                {
                    wp.OnHit(hit.point, hit.normal, 5);
                }

                SpawnCloudEffect(hit.point);
            }
            else if (hit.collider.CompareTag("Breakable Wall") || hit.collider.CompareTag("Entrance Door"))
            {
                var wall = hit.collider.GetComponent<BreakableWall>();
                if (_currentPickaxe.GetComponent<Pickaxe>().Power >= wall.PowerRequirement)
                {
                    wall.TakeDamage();       
                }
                SpawnCloudEffect(hit.point);
            }
            else
            {
                SpawnSparkEffect(hit.point, hit.normal);
            }
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
}
