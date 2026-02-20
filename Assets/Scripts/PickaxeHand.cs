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
        if (Physics.Raycast(transform.position, _camera.transform.forward, out var hit))
        {
            if (hit.collider.CompareTag("Mineral Deposit"))
            {
                var mineralDeposit = hit.collider.GetComponent<MineralDeposit>();
                if (_currentPickaxe.GetComponent<Pickaxe>().Power >= mineralDeposit.PowerRequirement)
                {
                    mineralDeposit.ProduceMineral(hit.point, hit.normal);       
                }
                else
                {
                    // produce effect
                }
            }

            if (hit.collider.CompareTag("Breakable Wall"))
            {
                var wall = hit.collider.GetComponent<BreakableWall>();
                if (_currentPickaxe.GetComponent<Pickaxe>().Power >= wall.PowerRequirement)
                {
                    wall.TakeDamage();       
                }
            }
        }
    }
}
