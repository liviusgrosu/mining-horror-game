using UnityEngine;

// This script toggles the enemy GameObject
public class EnemyGOActivationZone : MonoBehaviour
{
    
    [SerializeField] private GameObject enemy;
    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        enemy.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        enemy.SetActive(false);
    }
}
