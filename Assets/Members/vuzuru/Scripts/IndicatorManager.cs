using UnityEngine;
using System.Collections.Generic;

public class IndicatorManager : MonoBehaviour
{
    public static IndicatorManager Instance;

    private Dictionary<EnemyHealth, OffScreenIndicator> indicators = new Dictionary<EnemyHealth, OffScreenIndicator>();
    private Camera mainCam;
    private GameObject cursorPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCam = Camera.main;
        cursorPrefab = Resources.Load<GameObject>("Prefabs/Cursor");
        
        if (cursorPrefab != null && ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.AddPool("Cursor", cursorPrefab, 10);
        }
    }

    private void Update()
    {
        // Find all active enemies in scene
        // Optimized: Only check objects with EnemyHealth
        EnemyHealth[] enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude);

        // Keep track of which enemies we've processed
        HashSet<EnemyHealth> currentEnemies = new HashSet<EnemyHealth>(enemies);

        // 1. Remove indicators for enemies that are no longer present
        List<EnemyHealth> toRemove = new List<EnemyHealth>();
        foreach (var enemy in indicators.Keys)
        {
            if (!currentEnemies.Contains(enemy) || enemy == null)
            {
                toRemove.Add(enemy);
            }
        }

        foreach (var enemy in toRemove)
        {
            if (indicators[enemy] != null)
            {
                ObjectPooler.Instance.ReturnToPool("Cursor", indicators[enemy].gameObject);
            }
            indicators.Remove(enemy);
        }

        // 2. Add or Update indicators for current enemies
        foreach (var enemy in currentEnemies)
        {
            Vector3 screenPos = mainCam.WorldToViewportPoint(enemy.transform.position);
            bool isOffScreen = screenPos.x <= 0 || screenPos.x >= 1 || screenPos.y <= 0 || screenPos.y >= 1 || screenPos.z <= 0;

            if (isOffScreen)
            {
                if (!indicators.ContainsKey(enemy))
                {
                    GameObject cursorGo = ObjectPooler.Instance.SpawnFromPool("Cursor", Vector3.zero, Quaternion.identity);
                    if (cursorGo != null)
                    {
                        OffScreenIndicator indicator = cursorGo.GetComponent<OffScreenIndicator>();
                        if (indicator == null) indicator = cursorGo.AddComponent<OffScreenIndicator>();
                        
                        indicator.SetTarget(enemy.transform);
                        indicators.Add(enemy, indicator);
                    }
                }
            }
            else
            {
                // If enemy is back on screen, remove its indicator
                if (indicators.ContainsKey(enemy))
                {
                    ObjectPooler.Instance.ReturnToPool("Cursor", indicators[enemy].gameObject);
                    indicators.Remove(enemy);
                }
            }
        }
    }
}
