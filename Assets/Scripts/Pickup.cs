using System.Resources;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private Transform _camera;
    [SerializeField]
    private GameObject _hoveringOver;
    private IInteractable _hoveringOverInteractable;
    
    public LayerMask ignoreMask;
    
    private AudioSource _audioSource;
    
    [SerializeField]
    private AudioClip _pickupSound;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        _camera = Camera.main.transform;
    }
    
    private void Update()
    {
        if (Physics.Raycast(
            _camera.position, _camera.transform.forward * 1.2f, out var hit, 3.0f, ~ignoreMask))
        {
            if (hit.collider.gameObject != _hoveringOver)
            {
                GameManager.Instance.ToggleOffAllText();
                _hoveringOverInteractable?.ToggleOutline(false);
                _hoveringOver = hit.collider.gameObject;
            }
            
            if (_hoveringOver.CompareTag("Mineral"))
            {
                GameManager.Instance.TogglePickupIcon(true);
                _hoveringOverInteractable = _hoveringOver.GetComponent<IInteractable>();
                _hoveringOverInteractable?.ToggleOutline(true);
            }
            else if (_hoveringOver.CompareTag("Anvil"))
            {
                GameManager.Instance.ToggleQuestionMark(true);
            }
            else if (_hoveringOver.CompareTag("Entrance Door"))
            {
                GameManager.Instance.ToggleQuestionMark(true);
            }
            else if (_hoveringOver.CompareTag("Mineral Deposit"))
            {
                GameManager.Instance.ToggleQuestionMark(true);
            }
            else if (_hoveringOver.CompareTag("Blockage Rock"))
            {
                GameManager.Instance.ToggleQuestionMark(true);
            }
            else if (_hoveringOver.CompareTag("Breakable"))
            {
                GameManager.Instance.ToggleQuestionMark(true);
            }
            else
            {
                _hoveringOver = null;
            }
        }
        else
        {
            GameManager.Instance.ToggleOffAllText();
            _hoveringOverInteractable?.ToggleOutline(false);
            _hoveringOver = null;
        }

        if (Input.GetKey(KeyCode.E) && _hoveringOver)
        {
            if (_hoveringOver.CompareTag("Mineral") && _hoveringOverInteractable != null)
            {
                _audioSource.PlayOneShot(_pickupSound);
                GameManager.Instance.AddMineral(_hoveringOver.GetComponent<Mineral>().MineralName);
                _hoveringOverInteractable = null;
                Destroy(_hoveringOver);
            }
            if (_hoveringOver.CompareTag("Mineral Deposit"))
            {
                GameManager.Instance.ShowMineralDepositHoverText();
            }
            else if (_hoveringOver.CompareTag("Anvil"))
            {
                GameManager.Instance.OpenUpgradeUI();
            }
            
            else if (_hoveringOver.CompareTag("Entrance Door"))
            {
                GameManager.Instance.ShowEntranceDoorText();
            }
            
            else if (_hoveringOver.CompareTag("Blockage Rock"))
            {
                GameManager.Instance.ShowBlockageRockText();
            }
            
            else if (_hoveringOver.CompareTag("Breakable"))
            {
                GameManager.Instance.ShowNormalRockHoverText();
            }
        }
    }
}
