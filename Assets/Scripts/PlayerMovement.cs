using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    [SerializeField]
    private float _movementSpeed = 6f;
    private float _yVelocity;
    [SerializeField]
    private float _gravity = -18f;

    // Looking
    private Camera _camera;
    private float _yRotation;
    [SerializeField]
    private float _mouseSensitivity = 100f;
    
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _camera = Camera.main;
    }

    void Update()
    {
        if (GameManager.Instance.InMenu)
        {
            return;
        }
        
        MovePlayer();
        Look();
    }

    private void Look()
    {
        var mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _mouseSensitivity;
        var mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * _mouseSensitivity;
        
        _yRotation -= mouseY;
        _yRotation = Mathf.Clamp(_yRotation, -80f, 80f);
        _camera.transform.localRotation = Quaternion.Euler(_yRotation, 0f, 0f);
        
        transform.Rotate(Vector3.up * mouseX);
    }

    private void MovePlayer()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        if (_controller.isGrounded && _yVelocity < 0)
        {
            _yVelocity = -2f;
        }
        
        var movementDir = transform.right * horizontal + transform.forward * vertical;
        movementDir *= _movementSpeed;
        
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            _yVelocity = Mathf.Sqrt(-2f * _gravity);
        }
        
        _yVelocity += _gravity * Time.deltaTime;
        var velocity = movementDir + Vector3.up * _yVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }
}
