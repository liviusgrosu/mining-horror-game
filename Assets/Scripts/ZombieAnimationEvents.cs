using UnityEngine;

public class ZombieAnimationEvents : MonoBehaviour
{
    private ZombieBehaviour _zombie;

    private void Awake()
    {
        _zombie = GetComponentInParent<ZombieBehaviour>();
    }

    public void EnableDamageCollider()
    {
        _zombie.EnableDamageCollider();
    }

    public void DisableDamageCollider()
    {
        _zombie.DisableDamageCollider();
    }

}
