using UnityEngine;

public class AttackNotice : MonoBehaviour, IPoolable
{
    private ParticleSystem ps;
    private float timer;
    private float duration;
    private bool isActive;
    private Vector3 initialScale;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        initialScale = transform.localScale;
    }

    public void OnSpawn()
    {
        isActive = true;
        if (ps != null) ps.Play();
    }

    public void OnReturnToPool()
    {
        isActive = false;
        if (ps != null) ps.Stop();
        transform.localScale = initialScale;
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
        this.timer = duration;
    }

    private void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        
        // Shrink scale as timer goes down
        transform.localScale = initialScale * t;

        if (timer <= 0)
        {
            ObjectPooler.Instance.ReturnToPool("AttackNotice", gameObject);
        }
    }
}
