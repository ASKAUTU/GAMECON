using UnityEngine;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Transform foregroundBarTransform; 
    [SerializeField] private Transform ghostBarTransform;      

    [Header("Settings")]
    [SerializeField] private float ghostDelay = 0.5f;
    [SerializeField] private float ghostShrinkSpeed = 1.5f;

    private Coroutine ghostCoroutine;

    private void Awake()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            SetBarScale(foregroundBarTransform, playerHealth.CurrentHealth / playerHealth.MaxHealth);
            SetBarScale(ghostBarTransform, playerHealth.CurrentHealth / playerHealth.MaxHealth);
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamageTaken += UpdateHealthBar;
            playerHealth.OnRespawn += HandleRespawn;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamageTaken -= UpdateHealthBar;
            playerHealth.OnRespawn -= HandleRespawn;
        }
    }

    private void HandleRespawn()
    {
        UpdateHealthBar(playerHealth.MaxHealth, playerHealth.MaxHealth, Vector2.zero);
        if (ghostCoroutine != null) StopCoroutine(ghostCoroutine);
        SetBarScale(ghostBarTransform, 1f);
    }

    private void UpdateHealthBar(float current, float max, Vector2 attackerPos)
    {
        if (max <= 0) return;
        
        float targetScaleX = Mathf.Clamp01(current / max);
        
        // 메인 바 즉시 감소
        SetBarScale(foregroundBarTransform, targetScaleX);

        // 잔상 애니메이션
        if (ghostCoroutine != null) StopCoroutine(ghostCoroutine);
        
        if (gameObject.activeInHierarchy)
            ghostCoroutine = StartCoroutine(GhostBarRoutine(targetScaleX));
        else
            SetBarScale(ghostBarTransform, targetScaleX);
    }

    private void SetBarScale(Transform t, float scaleX)
    {
        if (t == null) return;
        Vector3 scale = t.localScale;
        scale.x = scaleX;
        t.localScale = scale;
    }

    private IEnumerator GhostBarRoutine(float targetScaleX)
    {
        yield return new WaitForSeconds(ghostDelay);
        if (ghostBarTransform == null) yield break;

        float currentScaleX = ghostBarTransform.localScale.x;

        while (Mathf.Abs(currentScaleX - targetScaleX) > 0.001f)
        {
            currentScaleX = Mathf.MoveTowards(currentScaleX, targetScaleX, Time.deltaTime * ghostShrinkSpeed);
            SetBarScale(ghostBarTransform, currentScaleX);
            yield return null;
        }
        
        SetBarScale(ghostBarTransform, targetScaleX);
    }
}
