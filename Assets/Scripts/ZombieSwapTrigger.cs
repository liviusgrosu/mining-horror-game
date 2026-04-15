using UnityEngine;

public class ZombieSwapTrigger : MonoBehaviour
{
    [SerializeField] private GameObject zombieToDisable;
    [SerializeField] private GameObject zombieToEnable;

    private bool _triggered;
    
    private void OnTriggerEnter(Collider other)
    {
        if (_triggered && !other.CompareTag("Player"))
        {
            return;
        }

        _triggered = true;

        if (zombieToDisable)
        {
            zombieToDisable.SetActive(false);
        }

        if (zombieToEnable)
        {
            zombieToEnable.SetActive(true);
        }
    }
}
