using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] private float _hp;
    public float PowerRequirement = 4f;

    public void TakeDamage()
    {
        _hp--;
        if (_hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
