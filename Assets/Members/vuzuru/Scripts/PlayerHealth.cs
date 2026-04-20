using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float suckDuration = 0.4f;
    [SerializeField] private float stretchIntensity = 4f;
    [SerializeField] private float pushIntoWallAmount = 1.0f; // How far to push into the wall
    [SerializeField] private float vfxOffsetFromWall = 0.0f; // 0 means exactly on the surface (half-in, half-out)
    
    private Transform spawnPoint;
    private Vector2 lastHitNormal; // Store the normal of the wall we hit
    private bool isDying = false;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private Vector3 originalScale;
    private GameObject deathVFXPrefab;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        
        deathVFXPrefab = Resources.Load<GameObject>("VFX/PlayerWallDead");
        
        GameObject spawnObj = GameObject.Find("Spawn");
        if (spawnObj != null) spawnPoint = spawnObj.transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying) return;

        if (collision.transform.parent != null && collision.transform.parent.name == "Level")
        {
            Vector2 hitPoint = collision.contacts[0].point;
            lastHitNormal = collision.contacts[0].normal; // Store normal to know which way is "out"
            
            Vector3 targetInsideWall = (Vector3)(hitPoint - lastHitNormal * pushIntoWallAmount);
            targetInsideWall.z = transform.position.z;

            StartCoroutine(DieAndRespawn(targetInsideWall, hitPoint));
        }
    }

    private void SpawnDeathVFX(Vector2 pos)
    {
        Quaternion prefabRotation = deathVFXPrefab != null ? deathVFXPrefab.transform.rotation : Quaternion.identity;

        if (ObjectPooler.Instance != null && ObjectPooler.Instance.poolDictionary.ContainsKey("PlayerDeath"))
        {
            ObjectPooler.Instance.SpawnFromPool("PlayerDeath", pos, prefabRotation);
        }
        else if (deathVFXPrefab != null)
        {
            GameObject vfx = Instantiate(deathVFXPrefab, pos, prefabRotation);
            Destroy(vfx, 2f);
        }
    }

    private IEnumerator DieAndRespawn(Vector3 targetPos, Vector2 hitPoint)
    {
        isDying = true;
        
        // 1. Disable movement and physics
        if (playerMovement != null) playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // 2. Sucking Effect
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

        // 3. Trigger Particle Effect (Spawn at wall surface + small offset towards outside)
        Vector2 vfxPos = hitPoint + (lastHitNormal * vfxOffsetFromWall);
        SpawnDeathVFX(vfxPos);

        // Make sure it's completely invisible
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(0.1f);

        // 4. Respawn
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            transform.position = Vector3.zero;
        }

        // 4. Reset state with a little "pop-in" bounce
        float popElapsed = 0;
        float popDuration = 0.2f;
        while (popElapsed < popDuration)
        {
            popElapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, popElapsed / popDuration);
            yield return null;
        }
        
        transform.localScale = originalScale;
        rb.simulated = true;
        if (playerMovement != null) playerMovement.enabled = true;
        
        isDying = false;
    }
}
