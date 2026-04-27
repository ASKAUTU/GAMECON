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
        // CSV에서 현재 웨이브 정보 찾기 (예: SQB003_1)
        // 여기서는 기본적으로 SQB003_웨이브번호 형태나 다른 키를 사용할 수 있음
        // 만약 특정 웨이브에 여러 종류가 있다면 확장이 필요하지만, 현재는 키_웨이브번호 패턴으로 검색
        
        string waveKey = $"SQB003_{currentWave}";
        MonsterStat stat = MonsterDataManager.Instance.GetStat(waveKey);

        if (string.IsNullOrEmpty(stat.Key))
        {
            // 데이터가 없으면 기본값으로 생성
            int enemyCount = 3 + (currentWave * 2);
            SpawnEnemyWithStats(normalEnemyPrefab, enemyCount, 30 + (currentWave * 5), 10 + (currentWave * 2), 1.8f, 10 + (currentWave * 5));
            return;
        }

        // Prefab 결정 (SQB003 -> Sqibo)
        GameObject prefab = normalEnemyPrefab;
        if (stat.Key.StartsWith("SQB003"))
        {
            prefab = Resources.Load<GameObject>("Prefabs/Enemy/Sqibo");
        }
        else if (stat.Key.StartsWith("E004"))
        {
            // 다른 적 프리팹이 있다면 추가
        }

        int count = stat.SpawnCount > 0 ? stat.SpawnCount : 3 + (currentWave * 2);
        SpawnEnemyWithStats(prefab ?? normalEnemyPrefab, count, stat.HP, stat.ATK, stat.Speed, stat.ExpYield);
    }

    private void SpawnEnemyWithStats(GameObject prefab, int count, float hp, float atk, float speed, float exp)
    {
        if (prefab == null || spawnZones.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            Transform zone = spawnZones[Random.Range(0, spawnZones.Length)];
            Vector3 localRandomPos = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            Vector3 worldPos = zone.TransformPoint(localRandomPos);

            GameObject enemy = Instantiate(prefab, worldPos, Quaternion.identity);
            
            // Stats 적용
            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null) controller.Initialize(speed, atk);

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null) health.Initialize(hp, exp);

            activeEnemies.Add(enemy);
        }
    }
}
