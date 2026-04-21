using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Juice/Animation")]
    [SerializeField] private float squashIntensity = 0.2f;
    [SerializeField] private float squashSpeed = 10f;
    [SerializeField] private float attackStretch = 0.5f;

    private Transform player;
    private Rigidbody2D rb;
    private Vector3 originalScale;
    private bool isAttacking = false;
    private float nextAttackTime;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        enemyHealth = GetComponent<EnemyHealth>();
        
        // Ensure EnemyHealth is attached
        if (enemyHealth == null)
        {
            enemyHealth = gameObject.AddComponent<EnemyHealth>();
        }

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) p = GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    private void FixedUpdate()
    {
        // 넉백 중이거나 공격 중이면 이동 로직 무시
        if (player == null || isAttacking || (enemyHealth != null && enemyHealth.IsStunned)) 
        {
            if (!isAttacking && (enemyHealth == null || !enemyHealth.IsStunned)) 
                rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackSequence());
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            MoveTowardsPlayer();
        }

        ApplyIdleJuice();
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    private void ApplyIdleJuice()
    {
        if (isAttacking) return;
        
        float rhythmicOffset = Mathf.Sin(Time.time * squashSpeed) * squashIntensity;
        transform.localScale = new Vector3(originalScale.x - (rhythmicOffset * 0.5f), originalScale.y + rhythmicOffset, originalScale.z);
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        // 1. Wind up (Squash down)
        float elapsed = 0;
        float windUpTime = 0.3f;
        while (elapsed < windUpTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windUpTime;
            transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z);
            yield return null;
        }

        // 2. Dash/Stretch Attack (The move juice effect)
        elapsed = 0;
        float dashTime = 0.15f;
        Vector3 dashStart = transform.position;
        Vector3 dashTarget = transform.position + transform.up * 1.5f; // Jump forward

        while (elapsed < dashTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashTime;
            transform.position = Vector3.Lerp(dashStart, dashTarget, t);
            transform.localScale = new Vector3(originalScale.x * 0.5f, originalScale.y * (1 + attackStretch), originalScale.z);
            yield return null;
        }

        // 3. Damage check
        if (player != null && Vector2.Distance(transform.position, player.position) < attackRange * 1.5f)
        {
            IDamageable damageable = player.GetComponent<IDamageable>();
            if (damageable != null) 
            {
                // Pass a position behind the enemy so the knockback always pushes the player forward
                Vector2 knockbackOrigin = (Vector2)transform.position - (Vector2)transform.up;
                damageable.TakeDamage(damage, knockbackOrigin);
            }
        }

        // 4. Recover
        elapsed = 0;
        float recoverTime = 0.3f;
        while (elapsed < recoverTime)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, elapsed / recoverTime);
            yield return null;
        }

        transform.localScale = originalScale;
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }
}
