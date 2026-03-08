using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] private float _hp;
    public float PowerRequirement = 4f;

    [SerializeField] private GameObject soundBite;
    
    [SerializeField]
    private AudioClip breakingSound;
    
    public void TakeDamage()
    {
        _hp--;
        if (!(_hp <= 0)) return;
        var breakingSoundBite = Instantiate(soundBite, transform.position, Quaternion.identity);
        breakingSoundBite.GetComponent<AudioSource>().PlayOneShot(breakingSound);
        Destroy(breakingSoundBite, breakingSound.length);
        Destroy(gameObject);
    }
}
