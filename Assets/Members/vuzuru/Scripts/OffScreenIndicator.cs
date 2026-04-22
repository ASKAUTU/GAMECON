using UnityEngine;

public class OffScreenIndicator : MonoBehaviour, IPoolable
{
    private Transform target;
    private Camera mainCam;
    private RectTransform rectTransform; // If it's UI
    private SpriteRenderer spriteRenderer; // If it's world space
    private bool isActive;

    [SerializeField] private float margin = 0.9f; // Screen margin

    private void Awake()
    {
        mainCam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnSpawn()
    {
        isActive = true;
    }

    public void OnReturnToPool()
    {
        isActive = false;
        target = null;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void LateUpdate()
    {
        if (!isActive || target == null || mainCam == null) return;

        Vector3 screenPos = mainCam.WorldToViewportPoint(target.position);

        // Check if on screen
        if (screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1 && screenPos.z > 0)
        {
            // On screen, could hide it or return to pool
            // But manager usually handles this.
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            return;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;

        // Clamp to screen edges
        if (screenPos.z < 0) // Behind camera
        {
            screenPos *= -1;
        }

        Vector3 clampedScreenPos = screenPos;
        float marginOffset = (1f - margin) / 2f;
        clampedScreenPos.x = Mathf.Clamp(clampedScreenPos.x, marginOffset, 1f - marginOffset);
        clampedScreenPos.y = Mathf.Clamp(clampedScreenPos.y, marginOffset, 1f - marginOffset);

        Vector3 worldPos = mainCam.ViewportToWorldPoint(new Vector3(clampedScreenPos.x, clampedScreenPos.y, mainCam.nearClipPlane + 1f));
        transform.position = worldPos;

        // Rotate towards target
        Vector3 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }
}
