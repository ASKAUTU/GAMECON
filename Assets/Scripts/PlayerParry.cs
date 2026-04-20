using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerParry : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;

    [Header("Parry Settings")]
    [SerializeField] private float parryWindow = 0.5f;
    [SerializeField] private float parryKnockback = 6f;
    
    private GameObject parryVFXPrefab;
    private bool isParryWindowActive = false;
    private Coroutine parryCoroutine;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        rb = GetComponent<Rigidbody2D>();
        
        // Load Parry VFX
        parryVFXPrefab = Resources.Load<GameObject>("VFX/Parry");
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnParry += HandleParrySuccess;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnParry -= HandleParrySuccess;
    }

    private void Update()
    {
        // F키 입력 체크 (Input System)
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (!isParryWindowActive && !playerHealth.IsDying)
            {
                if (parryCoroutine != null) StopCoroutine(parryCoroutine);
                parryCoroutine = StartCoroutine(ParryWindowRoutine());
            }
        }
    }

    private IEnumerator ParryWindowRoutine()
    {
        isParryWindowActive = true;
        playerHealth.IsParrying = true;
        
        Debug.Log("[PlayerParry] Parry Window OPEN");

        yield return new WaitForSeconds(parryWindow);

        playerHealth.IsParrying = false;
        isParryWindowActive = false;
        Debug.Log("[PlayerParry] Parry Window CLOSED");
    }

    private void HandleParrySuccess(Vector2 attackerPos)
    {
        Debug.Log("[PlayerParry] PARRY EXECUTED!");
        
        // 1. VFX Burst (플레이어와 적 사이)
        Vector2 playerPos = transform.position;
        Vector2 midPoint = (playerPos + attackerPos) / 2f;
        SpawnParryVFX(midPoint);

        // 2. Knockback 6
        if (rb != null)
        {
            Vector2 knockbackDir = (playerPos - attackerPos).normalized;
            if (knockbackDir == Vector2.zero) knockbackDir = -(Vector2)transform.up;
            
            rb.linearVelocity = knockbackDir * parryKnockback;
        }

        // 3. Camera Shake
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.3f, 0.4f);

        // 패링 성공 시 윈도우 즉시 종료
        if (parryCoroutine != null) StopCoroutine(parryCoroutine);
        playerHealth.IsParrying = false;
        isParryWindowActive = false;
    }

    private void SpawnParryVFX(Vector2 pos)
    {
        if (parryVFXPrefab != null)
        {
            // Parry는 자주 쓰이지 않을 수 있으므로 일단 직접 생성 (VFX 폴더에 있으면 풀링 권장하지만 패링은 별도 연출)
            GameObject vfx = Instantiate(parryVFXPrefab, pos, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
}
