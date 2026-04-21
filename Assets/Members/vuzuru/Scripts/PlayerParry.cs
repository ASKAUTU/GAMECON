using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerParry : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private PlayerFeedback playerFeedback;

    [Header("Parry Settings")]
    [SerializeField] private float parryWindow = 0.5f;
    [SerializeField] private float parryKnockback = 12f;
    [SerializeField] private float parryCooldown = 1.0f;
    
    private GameObject parryVFXPrefab;
    private bool isParryWindowActive = false;
    private Coroutine parryCoroutine;
    private float nextParryTime;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        rb = GetComponent<Rigidbody2D>();
        playerFeedback = GetComponent<PlayerFeedback>();
        
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
            if (Time.time >= nextParryTime && !isParryWindowActive && !playerHealth.IsDying)
            {
                if (parryCoroutine != null) StopCoroutine(parryCoroutine);
                parryCoroutine = StartCoroutine(ParryWindowRoutine());
                if (playerFeedback != null) playerFeedback.PlayParryJuice();
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
        
        // 실패 시 쿨타임 적용
        nextParryTime = Time.time + parryCooldown;
        Debug.Log("[PlayerParry] Parry Window CLOSED - Cooldown started");
    }

    private void HandleParrySuccess(Vector2 attackerPos)
    {
        Debug.Log("[PlayerParry] PARRY EXECUTED!");
        
        // 1. VFX Burst (플레이어와 적 사이)
        Vector2 playerPos = transform.position;
        Vector2 midPoint = (playerPos + attackerPos) / 2f;
        SpawnParryVFX(midPoint);

        // 2. Knockback Both
        StartCoroutine(ParryKnockbackRoutine(attackerPos));

        // 3. Camera Shake
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 0.1f);

        // 패링 성공 시 로직
        // 쿨타임 초기화 (즉시 재시전 가능)
        nextParryTime = 0;

        // 다수의 적에게 동시에 맞을 때를 대비해 아주 짧은 시간(0.1초) 동안 패링 판정 유지 후 종료
        StartCoroutine(BriefParryMaintain());
    }

    private IEnumerator BriefParryMaintain()
    {
        // 기존 윈도우 루틴 중지
        if (parryCoroutine != null) StopCoroutine(parryCoroutine);
        
        // 아주 짧은 시간 동안만 패링 상태 유지 (동시 타격 방어)
        playerHealth.IsParrying = true;
        isParryWindowActive = true;
        
        yield return new WaitForSeconds(0.1f);
        
        playerHealth.IsParrying = false;
        isParryWindowActive = false;
    }

    private IEnumerator ParryKnockbackRoutine(Vector2 attackerPos)
    {
        playerHealth.IsStunned = true;
        
        Vector2 playerPos = transform.position;
        Vector2 knockbackDirToPlayer = (playerPos - attackerPos).normalized;
        if (knockbackDirToPlayer == Vector2.zero) knockbackDirToPlayer = -(Vector2)transform.up;

        // Player Knockback
        if (rb != null)
        {
            rb.linearVelocity = knockbackDirToPlayer * parryKnockback;
        }

        // Enemy Knockback (Attacker)
        Collider2D[] attackers = Physics2D.OverlapCircleAll(attackerPos, 1.5f);
        foreach (var col in attackers)
        {
            if (col.CompareTag("Player")) continue;
            
            Rigidbody2D enemyRb = col.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = -knockbackDirToPlayer * parryKnockback;
                
                EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(0, playerPos);
                }
            }
        }

        // 0.2초 동안 넉백 유지 (이동 입력 무시)
        yield return new WaitForSeconds(0.2f);
        
        playerHealth.IsStunned = false;
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
