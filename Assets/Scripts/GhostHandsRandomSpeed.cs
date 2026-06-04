using UnityEngine;

public class GhostHandsRandomSpeed : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 1f;

    private static readonly int SpeedMult = Animator.StringToHash("SpeedMult");

    private void Awake()
    {
        _animator =  GetComponent<Animator>();
    }
    
    private void OnEnable()
    {
        if (!_animator)
        {
            return;
        }
        _animator.SetFloat(SpeedMult, Random.Range(minSpeed, maxSpeed));
    }
}
