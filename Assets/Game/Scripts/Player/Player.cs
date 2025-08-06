using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float kickForce = 10f;
    private Vector2 _moveInput;
    private Rigidbody _rb;
    private GameObject _ball;
    public bool hasBall;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Movement (WASD)
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // Kick (Spacebar)
    public void OnKick(InputAction.CallbackContext context)
    {
        if (context.performed && hasBall && _ball != null)
        {
            _ball.GetComponent<Rigidbody>().AddForce(transform.forward * kickForce, ForceMode.Impulse);
            hasBall = false;
        }
    }

    void FixedUpdate()
    {
        // Movement
        _rb.linearVelocity = new Vector3(_moveInput.x, 0, _moveInput.y) * moveSpeed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            _ball = other.gameObject;
            hasBall = true;
        }
    }
}