using UnityEngine;
using System.Collections;

public class PlayerFeedback : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerMovement playerMovement;

    [Header("Hit Feedback")]
    [SerializeField] private Color hitColor = new Color(1f, 0f, 0.5f);
    [SerializeField] private float hitFlashDuration = 0.15f;
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float stunDuration = 0.4f;

    private Color originalColor;
    private bool isStunned = false;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    private void OnEnable()
    {
        playerHealth.OnDamageTaken += HandleDamage;
    }

    private void OnDisable()
    {
        playerHealth.OnDamageTaken -= HandleDamage;
    }

    private void HandleDamage(float current, float max, Vector2 attackerPos)
    {
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.2f, 0.2f);
        StartCoroutine(HitFeedbackRoutine(attackerPos));
    }

    private IEnumerator HitFeedbackRoutine(Vector2 attackerPosition)
    {
        SetStun(true);
        if (sr != null) sr.color = hitColor;

        Vector2 playerPos = transform.position;
        Vector2 knockbackDir = (playerPos - attackerPosition).normalized;
        if (knockbackDir.sqrMagnitude < 0.01f) knockbackDir = -(Vector2)transform.up;

        rb.linearVelocity = knockbackDir * knockbackForce;

        yield return new WaitForSeconds(hitFlashDuration);
        if (sr != null) sr.color = originalColor;

        yield return new WaitForSeconds(stunDuration - hitFlashDuration);
        SetStun(false);
    }

    private void SetStun(bool stunned)
    {
        isStunned = stunned;
        if (playerHealth != null) playerHealth.IsStunned = stunned;
        if (playerMovement != null) playerMovement.enabled = !stunned;
    }
}
