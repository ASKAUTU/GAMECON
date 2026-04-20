using UnityEngine;
using System.Collections;

public class PlayerDeathFeedback : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Death Settings")]
    [SerializeField] private float suckDuration = 0.4f;
    [SerializeField] private float stretchIntensity = 4f;
    [SerializeField] private float pushIntoWallAmount = 1.0f;
    [SerializeField] private float vfxOffsetFromWall = 0.0f;

    private Vector3 originalScale;
    private Color originalColor;
    private Transform spawnPoint;
    private GameObject deathVFXPrefab;
    private Vector2 lastHitNormal;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        
        originalScale = transform.localScale;
        if (sr != null) originalColor = sr.color;

        deathVFXPrefab = Resources.Load<GameObject>("VFX/PlayerWallDead");
        GameObject spawnObj = GameObject.Find("Spawn");
        if (spawnObj != null) spawnPoint = spawnObj.transform;
    }

    private void OnEnable()
    {
        playerHealth.OnDeath += StartDeathAnimation;
        playerHealth.OnRespawn += HandleRespawn;
    }

    private void OnDisable()
    {
        playerHealth.OnDeath -= StartDeathAnimation;
        playerHealth.OnRespawn -= HandleRespawn;
    }

    // 벽 충돌 감지 (기존 로직 복구)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (playerHealth.IsDying) return;

        if (collision.transform.parent != null && collision.transform.parent.name == "Level")
        {
            Vector2 hitPoint = collision.contacts[0].point;
            lastHitNormal = collision.contacts[0].normal;
            Vector3 targetInsideWall = (Vector3)(hitPoint - lastHitNormal * pushIntoWallAmount);
            targetInsideWall.z = transform.position.z;
            
            playerHealth.Kill(targetInsideWall, hitPoint);
        }
    }

    private void StartDeathAnimation(Vector3 targetPos, Vector2 hitPoint)
    {
        StartCoroutine(DieAndRespawnRoutine(targetPos, hitPoint));
    }

    private IEnumerator DieAndRespawnRoutine(Vector3 targetPos, Vector2 hitPoint)
    {
        if (playerMovement != null) playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // 1. Suck into wall animation
        float elapsed = 0;
        Vector3 startPos = transform.position;
        while (elapsed < suckDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / suckDuration;
            float easeT = t * t; 
            transform.position = Vector3.Lerp(startPos, targetPos, easeT);
            
            float currentVolume = 1.0f - easeT; 
            float stretchY = originalScale.y * (1 + (t * stretchIntensity)) * currentVolume;
            float shrinkX = originalScale.x * (1 - (t * 0.9f)) * currentVolume;
            transform.localScale = new Vector3(shrinkX, stretchY, originalScale.z);
            yield return null;
        }

        // 2. Spawn VFX
        SpawnDeathVFX(hitPoint + (lastHitNormal * vfxOffsetFromWall));
        transform.localScale = Vector3.zero;
        
        yield return new WaitForSeconds(0.5f);

        // 3. Respawn
        playerHealth.ResetHealth();
    }

    private void HandleRespawn()
    {
        transform.position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        rb.simulated = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (sr != null) sr.color = originalColor;
        StartCoroutine(PopUpRoutine());
    }

    private IEnumerator PopUpRoutine()
    {
        float popElapsed = 0;
        float popDuration = 0.2f;
        while (popElapsed < popDuration)
        {
            popElapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, popElapsed / popDuration);
            yield return null;
        }
        transform.localScale = originalScale;
    }

    private void SpawnDeathVFX(Vector2 pos)
    {
        if (ObjectPooler.Instance != null && ObjectPooler.Instance.poolDictionary.ContainsKey("PlayerDeath"))
            ObjectPooler.Instance.SpawnFromPool("PlayerDeath", pos, Quaternion.identity);
        else if (deathVFXPrefab != null)
        {
            GameObject vfx = Instantiate(deathVFXPrefab, pos, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
}
