using System.Collections;
using UnityEngine;

public class ScreenShakeEffect : MonoBehaviour
{
    public static ScreenShakeEffect Instance;
    public AnimationCurve Curve;
    public float Duration = 1f;
    public bool IsCameraShaking;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    public void BeginShaking()
    {
        IsCameraShaking = true;
        StartCoroutine(Shaking());
    }
    
    private IEnumerator Shaking()
    {
        var startPosition = transform.position;
        var elapsedTime = 0f;
        
        while (elapsedTime < Duration)
        {
            elapsedTime += Time.deltaTime;
            var strength = Curve.Evaluate(elapsedTime / Duration);
            transform.position = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }
        
        transform.position = startPosition;
        IsCameraShaking = false;
    }
}
