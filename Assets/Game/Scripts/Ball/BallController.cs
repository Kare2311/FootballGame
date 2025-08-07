using UnityEngine;

public class BallController : MonoBehaviour
{
    public PlayerController currentHolder;
    public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetHolder(PlayerController holder)
    {
        currentHolder = holder;
        rb.isKinematic = true;
        transform.SetParent(holder.ballHoldPosition);
        transform.localPosition = Vector3.zero;
    }

    public void Pass(Vector3 targetPosition, float force)
    {
        Release();
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public void Shoot(Vector3 direction, float force)
    {
        Release();
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

    public void ResetPosition(Vector3 position)
    {
        Release();
        transform.position = position;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void Release()
    {
        if (currentHolder != null)
        {
            transform.SetParent(null);
            currentHolder = null;
            rb.isKinematic = false;
        }
    }
}