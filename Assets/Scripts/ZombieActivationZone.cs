using UnityEngine;

public class ZombieActivationZone : MonoBehaviour
{
    [SerializeField] private GameObject zombie;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        zombie.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        zombie.SetActive(false);
    }
}
