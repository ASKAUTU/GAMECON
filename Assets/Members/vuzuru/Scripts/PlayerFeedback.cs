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

    [Header("Juice Settings")]
    [SerializeField] private float attackSquashX = 1.2f;
    [SerializeField] private float attackSquashY = 0.8f;
    [SerializeField] private float juiceDuration = 0.1f;

    private Vector3 originalScale;
    private Coroutine juiceCoroutine;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        originalScale = transform.localScale;
    }

    [Header("Parry Juice")]
    [SerializeField] private Color parryFlashColor = new Color(1f, 1f, 0.5f, 1f);
    [SerializeField] private float parrySquashX = 0.8f;
    [SerializeField] private float parrySquashY = 1.2f;

    public void PlayParryJuice()
    {
        if (playerHealth.IsDying) return;
        if (juiceCoroutine != null) StopCoroutine(juiceCoroutine);
        juiceCoroutine = StartCoroutine(ParryJuiceRoutine());
    }

    private IEnumerator ParryJuiceRoutine()
    {
        float elapsed = 0;
        float duration = juiceDuration * 1.5f;
        Vector3 targetScale = new Vector3(originalScale.x * parrySquashX, originalScale.y * parrySquashY, originalScale.z);

        if (sr != null) sr.color = parryFlashColor;

        // Squash/Stretch and Fade
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Scale
            if (t < 0.5f)
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t * 2f);
            else
                transform.localScale = Vector3.Lerp(targetScale, originalScale, (t - 0.5f) * 2f);

            // Color Fade
            if (sr != null) sr.color = Color.Lerp(parryFlashColor, originalColor, t);

            yield return null;
        }

        transform.localScale = originalScale;
        if (sr != null) sr.color = originalColor;
        juiceCoroutine = null;
    }

    public void PlayAttackJuice()
    {
        if (playerHealth.IsDying) return;
        if (juiceCoroutine != null) StopCoroutine(juiceCoroutine);
        juiceCoroutine = StartCoroutine(AttackJuiceRoutine());
    }

    private IEnumerator AttackJuiceRoutine()
    {
        float elapsed = 0;
        Vector3 targetScale = new Vector3(originalScale.x * attackSquashX, originalScale.y * attackSquashY, originalScale.z);

        // Squash
        while (elapsed < juiceDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / juiceDuration);
            yield return null;
        }

        // Return to normal
        elapsed = 0;
        while (elapsed < juiceDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / juiceDuration);
            yield return null;
        }

        transform.localScale = originalScale;
        juiceCoroutine = null;
    }

    private void OnEnable()
    {
        playerHealth.OnDamageTaken += HandleDamage;
        playerHealth.OnRespawn += ResetJuice;
    }

    private void OnDisable()
    {
        playerHealth.OnDamageTaken -= HandleDamage;
        playerHealth.OnRespawn -= ResetJuice;
    }

    private void ResetJuice()
    {
        if (juiceCoroutine != null) StopCoroutine(juiceCoroutine);
        transform.localScale = originalScale;
        juiceCoroutine = null;
    }

    private void HandleDamage(float current, float max, Vector2 attackerPos)
    {
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.2f, 0.2f);
        if (PostProcessController.Instance != null) PostProcessController.Instance.FlashVignette(0.7f, 0.4f);
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
