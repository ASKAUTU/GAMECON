using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float suckDuration = 0.4f;
    [SerializeField] private float stretchIntensity = 4f;
    [SerializeField] private float pushIntoWallAmount = 1.0f; // How far to push into the wall
    
    private Transform spawnPoint;
    private bool isDying = false;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private Vector3 originalScale;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        
        GameObject spawnObj = GameObject.Find("Spawn");
        if (spawnObj != null) spawnPoint = spawnObj.transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying) return;

        // Check if the collided object is a child of "Level"
        if (collision.transform.parent != null && collision.transform.parent.name == "Level")
        {
            // Calculate a point slightly inside the wall based on hit direction
            Vector2 hitPoint = collision.contacts[0].point;
            Vector2 hitNormal = collision.contacts[0].normal;
            // Target is past the hit point, "inside" the wall
            Vector3 targetInsideWall = (Vector3)(hitPoint - hitNormal * pushIntoWallAmount);
            targetInsideWall.z = transform.position.z;

            StartCoroutine(DieAndRespawn(targetInsideWall));
        }
    }

    private IEnumerator DieAndRespawn(Vector3 targetPos)
    {
        isDying = true;
        
        // 1. Disable movement and physics
        if (playerMovement != null) playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // 2. Sucking Effect (Move DEEP into wall while stretching and shrinking)
        float elapsed = 0;
        Vector3 startPos = transform.position;

        while (elapsed < suckDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / suckDuration;
            // Use an easing function for a snappier "suck"
            float easeT = t * t; 

            // Move towards the inside of the wall
            transform.position = Vector3.Lerp(startPos, targetPos, easeT);

            // Dynamic Stretch/Squash:
            // Y increases for stretch, but overall scale decreases towards 0 to "disappear"
            float currentVolume = 1.0f - easeT; 
            float stretchY = originalScale.y * (1 + (t * stretchIntensity)) * currentVolume;
            float shrinkX = originalScale.x * (1 - (t * 0.9f)) * currentVolume;
            
            transform.localScale = new Vector3(shrinkX, stretchY, originalScale.z);

            yield return null;
        }

        // Make sure it's completely invisible before teleporting
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(0.1f);

        // 3. Respawn
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
