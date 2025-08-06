using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 12f;
    public float acceleration = 10f;

    [Header("Ball Interaction")]
    public float kickForce = 15f;
    public float passForce = 8f;
    public Transform kickPoint;
    public float kickRadius = 0.5f;

    [Header("Animation Parameters")]
    public float animationDampTime = 0.1f;

    // Components
    private CharacterController controller;
    private Animator animator;
    private Camera mainCamera;

    // Movement
    private Vector3 moveDirection;
    private Vector3 inputDirection;
    private float currentSpeed;
    private float targetSpeed;
    private bool isRunning;

    // Animation IDs
    private int animSpeed;
    private int animKick;
    private int animPass;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        // Set animation parameter IDs
        animSpeed = Animator.StringToHash("Speed");
        animKick = Animator.StringToHash("Kick");
        animPass = Animator.StringToHash("Pass");
    }

    private void Update()
    {
        HandleInput();
        HandleMovement();
        HandleRotation();
        HandleKicking();
        UpdateAnimations();
    }

    private void HandleInput()
    {
        // Get raw input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Store input direction
        inputDirection = new Vector3(horizontal, 0, vertical).normalized;
    }

    private void HandleMovement()
    {
        // Calculate target speed based on input and running state
        targetSpeed = isRunning ? runSpeed : walkSpeed;
        targetSpeed *= inputDirection.magnitude; // Scale by input magnitude

        // Smoothly adjust current speed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // Convert input to world space relative to camera
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        moveDirection = (cameraForward * inputDirection.z + mainCamera.transform.right * inputDirection.x).normalized;
        moveDirection *= currentSpeed;

        // Apply movement
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleRotation()
    {
        // Only rotate if we have movement input
        if (inputDirection.magnitude > 0.1f)
        {
            // Calculate target rotation based on movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleKicking()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger(animKick);
            TryKickBall(kickForce);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger(animPass);
            TryKickBall(passForce);
        }
    }

    private void TryKickBall(float force)
    {
        Collider[] hitColliders = Physics.OverlapSphere(kickPoint.position, kickRadius);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Ball"))
            {
                Rigidbody ballRb = collider.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    Vector3 kickDirection = transform.forward;
                    ballRb.AddForce(kickDirection * force, ForceMode.Impulse);
                }
            }
        }
    }

    private void UpdateAnimations()
    {
        // Convert current speed to 0-1 range for blend tree
        float normalizedSpeed = currentSpeed / runSpeed;
        animator.SetFloat(animSpeed, normalizedSpeed, animationDampTime, Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (kickPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(kickPoint.position, kickRadius);
        }
    }
}