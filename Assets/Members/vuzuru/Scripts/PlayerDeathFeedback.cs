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
    [SerializeField] private float respawnDelay = 1.0f;

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
        
        // Ensure we get the "real" original scale
        originalScale = new Vector3(0.3f, 0.3f, 1f); // Fallback to expected scale if unsure
        if (transform.localScale != Vector3.zero) originalScale = transform.localScale;
        
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
        StopAllCoroutines(); // Stop any juice or movement
        StartCoroutine(DieAndRespawnRoutine(targetPos, hitPoint));
    }

    private IEnumerator DieAndRespawnRoutine(Vector3 targetPos, Vector2 hitPoint)
    {
        if (playerMovement != null) playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // 1. Suck into wall animation (or just squash if no wall)
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

        // 2. Spawn VFX and hide
        SpawnDeathVFX(hitPoint + (lastHitNormal * vfxOffsetFromWall));
        transform.localScale = Vector3.zero;
        if (sr != null) sr.enabled = false; // Completely hide
        
        yield return new WaitForSeconds(respawnDelay);

        // 3. Respawn
        playerHealth.ResetHealth();
    }

    private void HandleRespawn()
    {
        transform.position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        rb.simulated = true;
        if (sr != null) 
        {
            sr.enabled = true;
            sr.color = originalColor;
        }
        if (playerMovement != null) playerMovement.enabled = true;
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
        if (deathVFXPrefab == null) return;
        
        Quaternion rotation = deathVFXPrefab.transform.rotation;
        GameObject vfxGo = null;
        
        if (ObjectPooler.Instance != null && ObjectPooler.Instance.poolDictionary.ContainsKey("PlayerDeath"))
            vfxGo = ObjectPooler.Instance.SpawnFromPool("PlayerDeath", pos, rotation);
        else
        {
            vfxGo = Instantiate(deathVFXPrefab, pos, rotation);
            Destroy(vfxGo, 2f);
        }

        if (vfxGo != null)
        {
            ParticleSystem[] ps = vfxGo.GetComponentsInChildren<ParticleSystem>();
            foreach (var p in ps) p.Play();
        }
    }
}
