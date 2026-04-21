using UnityEngine;

public class BulletHitEffect : MonoBehaviour, IPoolable
{
    private ParticleSystem ps;
    private float duration;
    private float timer;
    private bool isActive;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            duration = ps.main.duration + ps.main.startLifetime.constantMax;
        }
        else
        {
            duration = 1f; // Default duration
        }
    }

    public void OnSpawn()
    {
        timer = duration;
        isActive = true;
        if (ps != null)
        {
            ps.Play();
        }
    }

    public void OnReturnToPool()
    {
        isActive = false;
        if (ps != null)
        {
            ps.Stop();
        }
    }

    private void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            ObjectPooler.Instance.ReturnToPool("BulletHit", gameObject);
        }
    }
}
