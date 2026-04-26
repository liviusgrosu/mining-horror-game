using UnityEngine;

public class ZombieAnimationEvents : MonoBehaviour
{
    private EnemyCombat _combat;

    private void Awake()
    {
        _combat = GetComponentInParent<EnemyCombat>();
    }

    public void EnableDamageCollider()
    {
        _combat.EnableDamageCollider();
    }

    public void DisableDamageCollider()
    {
        _combat.DisableDamageCollider();
    }
}
