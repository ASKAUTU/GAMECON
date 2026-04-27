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
    private GameObject attackNoticePrefab;
    private GameObject currentNotice;

    public void Initialize(float speed, float dmg)
    {
        moveSpeed = speed;
        damage = dmg;
    }

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

        // Register AttackNotice Pool
        attackNoticePrefab = Resources.Load<GameObject>("VFX/AttackNotice");
        if (attackNoticePrefab != null && ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.AddPool("AttackNotice", attackNoticePrefab, 10);
        }
    }

    private void OnDisable()
    {
        // If enemy is disabled/destroyed, ensure the notice is handled
        if (currentNotice != null && currentNotice.activeInHierarchy)
        {
            ObjectPooler.Instance.ReturnToPool("AttackNotice", currentNotice);
            currentNotice = null;
        }
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
        Vector2 separation = CalculateSeparation();
        
        // Combine movement direction with separation (separation has higher priority if very close)
        Vector2 finalDirection = (direction + separation * 1.5f).normalized;
        rb.linearVelocity = finalDirection * moveSpeed;

        // Rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    private Vector2 CalculateSeparation()
    {
        Vector2 separationDir = Vector2.zero;
        float separationDistance = 0.8f; // Distance to keep from other enemies
        
        // Find nearby enemies
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, separationDistance);
        int count = 0;

        foreach (var col in nearbyEnemies)
        {
            if (col.gameObject != gameObject && col.CompareTag(gameObject.tag)) // Same tag usually means same type/enemy
            {
                Vector2 diff = (Vector2)transform.position - (Vector2)col.transform.position;
                float dist = diff.magnitude;
                if (dist < 0.01f) diff = Random.insideUnitCircle.normalized; // Prevent zero div
                
                // Strength increases as they get closer
                separationDir += diff.normalized / Mathf.Max(dist, 0.1f);
                count++;
            }
        }

        if (count > 0)
        {
            separationDir /= count;
        }

        return separationDir;
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

        float windUpTime = 0.6f;

        // 0. Show Attack Notice
        currentNotice = null;
        if (ObjectPooler.Instance != null)
        {
            currentNotice = ObjectPooler.Instance.SpawnFromPool("AttackNotice", transform.position, Quaternion.identity);
            if (currentNotice != null)
            {
                currentNotice.transform.SetParent(transform);
                currentNotice.transform.localPosition = Vector3.zero;
                
                AttackNotice noticeScript = currentNotice.GetComponent<AttackNotice>();
                if (noticeScript != null)
                {
                    noticeScript.SetDuration(windUpTime);
                }
            }
        }

        // 1. Wind up (Squash down)
        float elapsed = 0;
        while (elapsed < windUpTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windUpTime;
            transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z);
            yield return null;
        }

        currentNotice = null;

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
