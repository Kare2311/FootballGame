using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Settings")]
    public float followSharpness = 15f;
    public float maxControlDistance = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _isControlled;

    private Rigidbody _rb;
    private Transform _currentController;
    private Vector3 _controlOffset;

    // Public property for controlled state
    public bool IsControlled => _isControlled;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _isControlled = false;
    }

    private void FixedUpdate()
    {
        if (!_isControlled || _currentController == null) return;

        Vector3 targetPos = _currentController.position + _controlOffset;
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance > maxControlDistance)
        {
            Release();
            return;
        }

        _rb.linearVelocity = (targetPos - transform.position) * followSharpness;
        _rb.angularVelocity = Vector3.zero;
    }

    public void TakeControl(Transform controller)
    {
        _currentController = controller;
        _controlOffset = transform.position - controller.position;
        _isControlled = true;
        _rb.useGravity = false;
        _rb.linearDamping = 10f;
    }

    public void Kick(Vector3 direction, float force)
    {
        Release();
        _rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

    public void Transfer(Transform newAnchor)
    {
        TakeControl(newAnchor);
    }

    private void Release()
    {
        _isControlled = false;
        _currentController = null;
        _rb.useGravity = true;
        _rb.linearDamping = 0.5f;
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (_isControlled && _currentController != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _currentController.position);
        }
    }
}