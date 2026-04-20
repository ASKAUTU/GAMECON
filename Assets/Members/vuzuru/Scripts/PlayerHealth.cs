using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event Action<float, float, Vector2> OnDamageTaken;
    public event Action<Vector3, Vector2> OnDeath; // targetPos, hitPoint (벽 사망 연출용)
    public event Action OnRespawn;
    public event Action<Vector2> OnParry; // attackerPos

    public bool IsStunned { get; set; } = false;
    public bool IsDying { get; private set; } = false;
    public bool IsParrying { get; set; } = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector2 attackerPosition)
    {
        Debug.Log($"[PlayerHealth] TakeDamage called. Amount: {amount}, IsDying: {IsDying}");
        if (IsDying) return;

        if (IsParrying)
        {
            Debug.Log("[PlayerHealth] PARRY SUCCESS!");
            OnParry?.Invoke(attackerPosition);
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[PlayerHealth] New Health: {currentHealth}/{maxHealth}. Invoking OnDamageTaken.");
        OnDamageTaken?.Invoke(currentHealth, maxHealth, attackerPosition);

        if (currentHealth <= 0)
        {
            Debug.Log("[PlayerHealth] Health reached 0. Calling Kill.");
            Kill(transform.position, transform.position);
        }
    }

    // 벽 충돌 등 즉사 시 호출
    public void Kill(Vector3 targetPos, Vector2 hitPoint)
    {
        if (IsDying) return;
        IsDying = true;
        OnDeath?.Invoke(targetPos, hitPoint);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        IsDying = false;
        IsStunned = false;
        OnRespawn?.Invoke();
        OnDamageTaken?.Invoke(currentHealth, maxHealth, Vector2.zero);
    }
}
