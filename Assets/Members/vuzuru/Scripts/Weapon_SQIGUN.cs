using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon_SQIGUN : MonoBehaviour
{
    [SerializeField] private string bulletTag = "Bullet";
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 3f;

    private float nextFireTime;
    private Camera mainCam;

    private PlayerFeedback playerFeedback;

    private void Awake()
    {
        mainCam = Camera.main;
        playerFeedback = GetComponentInParent<PlayerFeedback>();
    }

    private void Update()
    {
        RotateTowardsMouse();
        HandleShooting();
    }

    private void RotateTowardsMouse()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCam.transform.position.z));
        
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    private void HandleShooting()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void Shoot()
    {
        if (ObjectPooler.Instance == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = ObjectPooler.Instance.SpawnFromPool(bulletTag, spawnPos, transform.rotation);
        
        // 플레이어 본인(부모)과의 충돌 방지 (레이어 설정이 안 되어 있을 경우 대비)
        if (bullet != null)
        {
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            Collider2D playerCollider = GetComponentInParent<Collider2D>();
            if (bulletCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, playerCollider);
            }
        }
        
        if (playerFeedback != null) playerFeedback.PlayAttackJuice();
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 0.05f);
    }
}
