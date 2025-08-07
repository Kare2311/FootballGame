using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class SoccerPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 12f;

    [Header("Ball Control")]
    public Transform ballAnchor; // Must be assigned in Inspector!
    public float kickForce = 15f;
    public float passForce = 8f;
    public float controlRadius = 0.5f;

    [Header("Team Settings")]
    public List<SoccerPlayer> teammates = new List<SoccerPlayer>(); // Assign teammates in Inspector
    public float switchCooldown = 0.5f;

    // Internal
    private CharacterController controller;
    private Animator animator;
    private Ball currentBall;
    private bool hasBall;
    private float lastSwitchTime;
    private int currentTeamIndex;

    // Animation IDs
    private int animSpeed;
    private int animKick;
    private int animPass;
    private bool _isActivePlayer;
    private static SoccerPlayer _currentActivePlayer; // Track active player globally

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        animSpeed = Animator.StringToHash("Speed");
        animKick = Animator.StringToHash("Kick");
        animPass = Animator.StringToHash("Pass");

        // Initialize all players as inactive
        _isActivePlayer = false;
        animator.SetFloat(animSpeed, 0f);
    }

    void Start()
    {
        // Activate first player in team automatically
        if (teammates.Count > 0 && teammates[0] == this)
        {
            SetPlayerActive(true);
        }
    }

    void Update()
    {
        if (!_isActivePlayer)
        {
            animator.SetFloat(animSpeed, 0f);
            return;
        }

        HandleMovement();
        HandleBall();
        HandleSwitching();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 inputDir = new Vector3(horizontal, 0, vertical).normalized;
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        // Camera-relative movement
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        Vector3 moveDirection = (camForward * vertical + Camera.main.transform.right * horizontal).normalized * targetSpeed;

        controller.Move(moveDirection * Time.deltaTime);

        // Rotation
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Animation
        float speedPercent = moveDirection.magnitude / runSpeed;
        animator.SetFloat(animSpeed, speedPercent);
    }

    void HandleBall()
    {
        if (!hasBall) return;

        if (Input.GetKeyDown(KeyCode.E)) // Shoot
        {
            Kick(transform.forward, kickForce);
        }
        else if (Input.GetKeyDown(KeyCode.Q)) // Pass
        {
            SoccerPlayer receiver = FindClosestTeammate();
            if (receiver != null)
            {
                Vector3 dir = (receiver.transform.position - transform.position).normalized;
                Kick(dir, passForce);
            }
        }
    }

    SoccerPlayer FindClosestTeammate()
    {
        SoccerPlayer closest = null;
        float minDistance = Mathf.Infinity;

        foreach (SoccerPlayer teammate in teammates)
        {
            if (teammate == this || !teammate.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(transform.position, teammate.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = teammate;
            }
        }
        return closest;
    }

    void HandleSwitching()
    {
        if (Time.time - lastSwitchTime < switchCooldown) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int currentIndex = teammates.IndexOf(this);
            int nextIndex = (currentIndex + 1) % teammates.Count;
            teammates[nextIndex].SetPlayerActive(true);
            lastSwitchTime = Time.time;
        }
    }

    void SwitchToNextPlayer()
    {
        int nextIndex = (currentTeamIndex + 1) % teammates.Count;
        SoccerPlayer nextPlayer = teammates[nextIndex];

        // Disable current player
        SetPlayerActive(false);

        // Enable new player
        nextPlayer.SetPlayerActive(true);

        // Transfer ball
        if (hasBall && currentBall != null)
        {
            currentBall.Transfer(nextPlayer.ballAnchor);
            hasBall = false;
            nextPlayer.hasBall = true;
            nextPlayer.currentBall = currentBall;
            currentBall = null;
        }

        currentTeamIndex = nextIndex;
        lastSwitchTime = Time.time;
    }

    public void SetPlayerActive(bool state)
    {
        _isActivePlayer = state;

        // Update global reference
        if (state)
        {
            if (_currentActivePlayer != null && _currentActivePlayer != this)
            {
                _currentActivePlayer.SetPlayerActive(false);
            }
            _currentActivePlayer = this;
        }

        // Visual feedback
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = state ? Color.green : Color.white;
        }
    }

    void Kick(Vector3 direction, float force)
    {
        if (currentBall == null || !currentBall.IsControlled) return;

        currentBall.Kick(direction, force);
        animator.SetTrigger(animKick);
        hasBall = false;
        currentBall = null;
    }

    public void SetControlled(bool state)
    {
        enabled = state;
        // Add any visual feedback here (e.g., highlight player)
    }

    public bool IsControlled() => enabled;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !hasBall)
        {
            currentBall = other.GetComponent<Ball>();
            if (currentBall != null && !currentBall.IsControlled)
            {
                currentBall.TakeControl(ballAnchor);
                hasBall = true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (ballAnchor != null) Gizmos.DrawWireSphere(ballAnchor.position, 0.1f);
    }
}