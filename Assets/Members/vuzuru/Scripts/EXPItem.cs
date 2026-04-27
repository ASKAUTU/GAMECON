using UnityEngine;
using System.Collections;

public class EXPItem : MonoBehaviour, IPoolable
{
    private float expValue;
    private Transform player;
    private bool isFollowing = false;
    private float followSpeed = 2f;
    private float acceleration = 5f;
    private float magnetRange = 5f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) p = GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    public void SetValue(float value)
    {
        expValue = value;
    }

    public void OnSpawn()
    {
        isFollowing = false;
        followSpeed = 2f;
        
        // Initial burst "푱푱푱푱"
        Vector2 burstDir = Random.insideUnitCircle.normalized;
        float burstForce = Random.Range(3f, 6f);
        rb.linearVelocity = burstDir * burstForce;
        
        // Slow down the burst over time
        StartCoroutine(DecelerateBurst());
    }

    public void OnReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
    }

    private IEnumerator DecelerateBurst()
    {
        float elapsed = 0;
        float duration = 0.5f;
        Vector2 initialVelocity = rb.linearVelocity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rb.linearVelocity = Vector2.Lerp(initialVelocity, Vector2.zero, elapsed / duration);
            yield return null;
        }
        
        rb.linearVelocity = Vector2.zero;
        isFollowing = true;
    }

    private void Update()
    {
        if (!isFollowing || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < magnetRange)
        {
            // Move towards player
            Vector2 direction = (player.position - transform.position).normalized;
            followSpeed += acceleration * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, player.position, followSpeed * Time.deltaTime);

            if (distance < 0.3f)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        if (PlayerExp.Instance != null)
        {
            PlayerExp.Instance.AddExp(expValue);
        }
        
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool("EXP", gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
