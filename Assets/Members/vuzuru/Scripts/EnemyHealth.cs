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
        
        // 1. Flash and HitStop
        if (sr != null) StartCoroutine(FlashRoutine());
        if (PostProcessController.Instance != null && amount > 0) PostProcessController.Instance.HitStop(0.05f);

        // 2. Knockback and Tilt
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
        
        Vector2 playerPos = attackerPos;
        Vector2 enemyPos = transform.position;
        Vector2 knockbackDir = (enemyPos - playerPos).normalized;
        if (knockbackDir == Vector2.zero) knockbackDir = -transform.up;
        
        rb.linearVelocity = knockbackDir * 6f;

        // Tilt Juice
        float randomTilt = Random.Range(15f, 25f) * (Random.value > 0.5f ? 1f : -1f);
        Quaternion targetRotation = Quaternion.Euler(0, 0, randomTilt);
        Quaternion originalRotation = transform.rotation;

        float elapsed = 0;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Tilt and then return
            if (t < 0.3f)
                transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, t / 0.3f);
            else
                transform.rotation = Quaternion.Lerp(targetRotation, originalRotation, (t - 0.3f) / 0.7f);

            yield return null;
        }

        transform.rotation = originalRotation;
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
        GameObject vfxPrefab = Resources.Load<GameObject>("VFX/PlayerWallDead");
        if (vfxPrefab == null) return;

        Quaternion rotation = vfxPrefab.transform.rotation;
        GameObject vfxGo = null;
        
        if (ObjectPooler.Instance != null && ObjectPooler.Instance.poolDictionary.ContainsKey("PlayerDeath"))
        {
            vfxGo = ObjectPooler.Instance.SpawnFromPool("PlayerDeath", transform.position, rotation);
        }
        else
        {
            vfxGo = Instantiate(vfxPrefab, transform.position, rotation);
            Destroy(vfxGo, 2f);
        }

        if (vfxGo != null)
        {
            ParticleSystem[] ps = vfxGo.GetComponentsInChildren<ParticleSystem>();
            foreach (var p in ps) p.Play();
        }
    }
}
