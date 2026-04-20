using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Juice Settings")]
    [SerializeField] private float squashIntensity = 0.1f;
    [SerializeField] private float squashSpeed = 15f;
    [SerializeField] private float movementStretch = 0.15f;
    [SerializeField] private float springStiffness = 200f; // Snappiness of the spring
    [SerializeField] private float springDamping = 10f;   // How fast the bouncing dies down
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector3 originalScale;
    private Vector3 scaleVelocity; // Used for spring physics

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;

        if (moveInput != Vector2.zero)
        {
            RotateTowardsMovement();
        }

        UpdateJellyScale();
    }

    private void UpdateJellyScale()
    {
        Vector3 targetScale = originalScale;

        if (moveInput != Vector2.zero)
        {
            float speedRatio = rb.linearVelocity.magnitude / moveSpeed;
            float dynamicIntensity = squashIntensity + (speedRatio * movementStretch);
            float dynamicSpeed = squashSpeed + (speedRatio * 5f);
            float rhythmicOffset = Mathf.Sin(Time.time * dynamicSpeed) * dynamicIntensity;

            targetScale = new Vector3(originalScale.x - (rhythmicOffset * 0.5f), originalScale.y + rhythmicOffset, originalScale.z);
        }

        Vector3 force = (targetScale - transform.localScale) * springStiffness;
        scaleVelocity += force * Time.fixedDeltaTime;
        scaleVelocity -= scaleVelocity * springDamping * Time.fixedDeltaTime;

        transform.localScale += scaleVelocity * Time.fixedDeltaTime;
    }

    private void RotateTowardsMovement()
    {
        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
