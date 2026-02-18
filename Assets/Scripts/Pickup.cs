using System.Resources;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private Transform _camera;
    [SerializeField]
    private GameObject _hoveringOver;
    private IInteractable _hoveringOverInteractable;
    
    private void Start()
    {
        _camera = Camera.main.transform;
    }
    
    private void Update()
    {
        Debug.DrawRay(_camera.position, _camera.transform.forward * 1.2f, Color.red);
        if (Physics.Raycast(_camera.position, _camera.transform.forward * 1.2f, out var hit))
        {
            if (hit.collider.gameObject != _hoveringOver)
            {
                GameManager.Instance.TogglePickupText(false);
                GameManager.Instance.ToggleUpgradeText(false);
                
                _hoveringOverInteractable?.ToggleOutline(false);
                _hoveringOver = hit.collider.gameObject;
            }
            
            if (_hoveringOver.CompareTag("Mineral"))
            {
                GameManager.Instance.TogglePickupText(true);
                _hoveringOverInteractable = _hoveringOver.GetComponent<IInteractable>();
                _hoveringOverInteractable?.ToggleOutline(true);
            }

            if (_hoveringOver.CompareTag("Anvil"))
            {
                GameManager.Instance.ToggleUpgradeText(true);
            }
        }
        else
        {
            _hoveringOverInteractable?.ToggleOutline(false);
            _hoveringOver = null;
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (_hoveringOver.CompareTag("Mineral") && _hoveringOverInteractable != null)
            {
                GameManager.Instance.AddMineral(_hoveringOver.GetComponent<Mineral>().MineralName);
                _hoveringOverInteractable = null;
                Destroy(_hoveringOver);
            }
            else if (_hoveringOver.CompareTag("Anvil"))
            {
                GameManager.Instance.OpenUpgradeUI();
            }
        }
    }
}
