using UnityEngine;

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float acceleration = 2f;
    public float deceleration = 3f;
    public bool isControlled = false;

    [Header("Animation")]
    public string speedParameter = "Speed"; // Float parameter for blend tree
    public float maxAnimationSpeed = 1.5f; // Adjust based on your animations

    [Header("Ball Control")]
    public Transform ballHoldPosition;
    public float passForce = 10f;
    public float shootForce = 20f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 movementInput;
    private float currentSpeed;
    private BallController ball;
    private bool isActive;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Initialize animator
       // animator.SetFloat(speedParameter, 0f);
    }

    void Update()
    {
        if (isControlled)
        {
            HandlePlayerInput();
        }
        else
        {
            HandleAI();
        }

        UpdateAnimations();
        ApplyMovement();
    }

    void HandlePlayerInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        movementInput = new Vector3(horizontal, 0f, vertical).normalized;
        currentSpeed = movementInput.magnitude * moveSpeed;
    }

    void HandleAI()
    {
        // Basic AI that just stands still - replace with your AI logic
        movementInput = Vector3.zero;
        currentSpeed = 0f;
    }

    void ApplyMovement()
    {
        if (currentSpeed > 0.1f)
        {
            controller.Move(movementInput * currentSpeed * Time.deltaTime);
            RotateTowardsMovement();
        }
    }


    void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        movementInput = new Vector3(horizontal, 0f, vertical).normalized;

        // Smooth speed changes
        float targetSpeed = movementInput.magnitude > 0.1f ? moveSpeed : 0f;
        currentSpeed = Mathf.Lerp(
            currentSpeed,
            targetSpeed,
            (movementInput.magnitude > 0.1f ? acceleration : deceleration) * Time.deltaTime
        );

        // Apply movement
        if (currentSpeed > 0.1f)
        {
            controller.Move(movementInput * currentSpeed * Time.deltaTime);
            RotateTowardsMovement();
        }
    }

    void RotateTowardsMovement()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void UpdateAnimations()
    {
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / moveSpeed);
        animator.SetFloat(speedParameter, normalizedSpeed);
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
        // Add visual feedback if needed
        GetComponentInChildren<Renderer>().material.color = controlled ? Color.blue : Color.white;
    }

    public void Pass(Vector3 targetPosition)
    {
        if (ball != null)
        {
            ball.Pass(targetPosition, passForce);
            ball = null;
        }
    }

    public void Shoot(Vector3 direction)
    {
        if (ball != null)
        {
            ball.Shoot(direction, shootForce);
            ball = null;
        }
    }

    // Called from animation event
    /* public void ReleaseBall()
     {
         if (ball != null)
         {
             Vector3 direction = transform.forward;
             float force = Input.GetButton("Fire1") ? shootForce : passForce;
             ball.Shoot(direction, force);
             ball = null;
         }
     }*/

    /*void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && ball == null)
        {
            ball = other.GetComponent<BallController>();
            if (ball != null)
            {
                ball.AttachToPlayer(ballHoldPosition);
            }
        }
    }*/

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active && animator != null)
        {
            animator.SetFloat(speedParameter, 0f);
        }
    }
}