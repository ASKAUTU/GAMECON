using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Wave Settings")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private int currentWave = 0;
    
    [Header("Spawn Settings")]
    [SerializeField] private GameObject normalEnemyPrefab;
    [SerializeField] private GameObject bossEnemyPrefab;
    [SerializeField] private Transform[] spawnZones;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isWaveActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        if (waveText == null)
        {
            GameObject waveObj = GameObject.Find("WaveText");
            if (waveObj != null) waveText = waveObj.GetComponent<TextMeshProUGUI>();
        }

        // Find EnemyZones 1-4
        List<Transform> zones = new List<Transform>();
        for (int i = 1; i <= 4; i++)
        {
            GameObject zone = GameObject.Find("EnemyZone" + i);
            if (zone == null) zone = GameObject.Find("EnemyZone (" + i + ")");
            if (zone != null) zones.Add(zone.transform);
        }
        spawnZones = zones.ToArray();
    }

    private void Start()
    {
        StartCoroutine(StartNextWave());
    }

    private void Update()
    {
        if (isWaveActive && activeEnemies.Count == 0)
        {
            isWaveActive = false;
            StartCoroutine(StartNextWave());
        }

        // Cleanup null entries (destroyed enemies)
        activeEnemies.RemoveAll(item => item == null || !item.activeInHierarchy);
    }

    private IEnumerator StartNextWave()
    {
        currentWave++;
        UpdateWaveUI();

        yield return new WaitForSeconds(timeBetweenWaves);

        SpawnWave();
        isWaveActive = true;
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = "Wave " + currentWave;
        }
    }

    private void SpawnWave()
    {
        if (currentWave == 10)
        {
            // Boss Wave
            SpawnEnemy(bossEnemyPrefab ?? normalEnemyPrefab, 1);
        }
        else
        {
            // Normal Wave: Amount increases with wave number
            int enemyCount = 3 + (currentWave * 2);
            SpawnEnemy(normalEnemyPrefab, enemyCount);
        }
    }

    private void SpawnEnemy(GameObject prefab, int count)
    {
        if (prefab == null || spawnZones.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            // Pick a random zone
            Transform zone = spawnZones[Random.Range(0, spawnZones.Length)];
            
            // localSpace의 -0.5 ~ 0.5 범위를 월드 좌표로 변환 (로컬 기준 스폰)
            Vector3 localRandomPos = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0
            );
            
            Vector3 worldPos = zone.TransformPoint(localRandomPos);

            GameObject enemy = Instantiate(prefab, worldPos, Quaternion.identity);
            activeEnemies.Add(enemy);
        }
    }
}
