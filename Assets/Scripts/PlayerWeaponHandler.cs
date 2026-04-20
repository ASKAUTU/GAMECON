using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    [SerializeField] private string weaponPath = "Prefabs/Weapons/SQIGUN/SQIGUN";
    [SerializeField] private Vector3 weaponOffset = new Vector3(0.5f, 0, 0);
    
    private GameObject currentWeapon;

    private void Start()
    {
        SpawnWeapon();
    }

    private void SpawnWeapon()
    {
        GameObject weaponPrefab = Resources.Load<GameObject>(weaponPath);
        if (weaponPrefab != null)
        {
            currentWeapon = Instantiate(weaponPrefab, transform);
            currentWeapon.transform.localPosition = weaponOffset;
            
            // Register Bullet Pool
            GameObject bulletPrefab = Resources.Load<GameObject>("Prefabs/Weapons/SQIGUN/Bullet");
            if (bulletPrefab != null && ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.AddPool("Bullet", bulletPrefab, 5);
            }
            
            // 만약 프리팹에 스크립트가 없다면 추가 (보통은 붙어있음)
            if (currentWeapon.GetComponent<Weapon_SQIGUN>() == null)
            {
                currentWeapon.AddComponent<Weapon_SQIGUN>();
            }
        }
        else
        {
            Debug.LogError($"Weapon prefab not found at: {weaponPath}");
        }
    }
}
