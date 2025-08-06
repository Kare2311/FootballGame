using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody _rb;
    private Vector3 _moveInput;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true; // Prevent physics from interfering with rotation
    }

    void Update()
    {
        // Get input (WASD or Arrow Keys)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        _moveInput = new Vector3(horizontal, 0, vertical).normalized;
    }

    void FixedUpdate()
    {
        // Move the player (using velocity)
        if (_moveInput != Vector3.zero)
        {
            // Apply movement in world space
            Vector3 moveVelocity = _moveInput * moveSpeed;
            _rb.linearVelocity = new Vector3(moveVelocity.x, _rb.linearVelocity.y, moveVelocity.z); // Preserve gravity

            // Rotate toward movement direction
            Quaternion targetRotation = Quaternion.LookRotation(_moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0); // Stop horizontal movement (keep gravity)
        }
    }
}