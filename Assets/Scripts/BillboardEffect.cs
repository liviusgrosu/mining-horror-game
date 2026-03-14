using UnityEngine;

public class BillboardEffect : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;
    
    private Camera _cam;
    private float _zRotation;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_cam == null) return;

        _zRotation += speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(-_cam.transform.forward) * Quaternion.Euler(0f, 0f, _zRotation);
    }
}
