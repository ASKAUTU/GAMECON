using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 30f;
    private float currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    
    public bool IsStunned { get; private set; }

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        // "PlayerDeath" 이펙트 풀이 없으면 자동 등록
        if (ObjectPooler.Instance != null && !ObjectPooler.Instance.poolDictionary.ContainsKey("PlayerDeath"))
        {
            GameObject vfx = Resources.Load<GameObject>("VFX/PlayerWallDead");
            if (vfx != null)
            {
                ObjectPooler.Instance.AddPool("PlayerDeath", vfx, 10);
            }
        }
    }

    private void Start()
    {
        // 소환 시 이펙트 버스트
        SpawnDeathVFX();
    }

    public void TakeDamage(float amount, Vector2 attackerPosition)
    {
        currentHealth -= amount;
        
        // 1. Flash effect
        if (sr != null) StartCoroutine(FlashRoutine());

        // 2. Knockback
        if (rb != null)
        {
            StopCoroutine(nameof(KnockbackRoutine));
            StartCoroutine(KnockbackRoutine(attackerPosition));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 attackerPos)
    {
        IsStunned = true;
        
        Vector2 knockbackDir = ((Vector2)transform.position - attackerPos).normalized;
        if (knockbackDir == Vector2.zero) knockbackDir = -transform.up;
        
        rb.linearVelocity = knockbackDir * 6f;

        // 0.2초 동안 스턴 (밀려나는 시간 확보)
        yield return new WaitForSeconds(0.2f);
        
        IsStunned = false;
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }

    private void Die()
    {
        SpawnDeathVFX();
        Destroy(gameObject);
    }

    private void SpawnDeathVFX()
    {
        if (ObjectPooler.Instance != null && ObjectPooler.Instance.poolDictionary.ContainsKey("PlayerDeath"))
        {
            ObjectPooler.Instance.SpawnFromPool("PlayerDeath", transform.position, Quaternion.identity);
        }
        else
        {
            // 풀링 실패 시 직접 생성 시도
            GameObject vfx = Resources.Load<GameObject>("VFX/PlayerWallDead");
            if (vfx != null)
            {
                GameObject go = Instantiate(vfx, transform.position, Quaternion.identity);
                Destroy(go, 2f);
            }
        }
    }
}
