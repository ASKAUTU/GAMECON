using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 5f;

    private float lifeTimer;
    private bool isActive = false;

    private TrailRenderer trail;

    private void Awake()
    {
        trail = GetComponentInChildren<TrailRenderer>();
    }

    public void OnSpawn()
    {
        lifeTimer = lifeTime;
        isActive = true;
        if (trail != null) trail.Clear();
    }

    public void OnReturnToPool()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive) return;

        transform.Translate(Vector3.up * speed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            ObjectPooler.Instance.ReturnToPool("Bullet", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        // 플레이어 본인은 무시
        if (collision.CompareTag("Player")) return;

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, transform.position);
            ObjectPooler.Instance.SpawnFromPool("BulletHit", transform.position, Quaternion.identity);
            ObjectPooler.Instance.ReturnToPool("Bullet", gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            ObjectPooler.Instance.SpawnFromPool("BulletHit", transform.position, Quaternion.identity);
            ObjectPooler.Instance.ReturnToPool("Bullet", gameObject);
        }
        else if (collision.transform.parent != null && collision.transform.parent.name == "Level")
        {
            ObjectPooler.Instance.SpawnFromPool("BulletHit", transform.position, Quaternion.identity);
            ObjectPooler.Instance.ReturnToPool("Bullet", gameObject);
        }
    }
}
