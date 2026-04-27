using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public struct MonsterStat
{
    public string Key;
    public float HP;
    public float ATK;
    public float Speed;
    public float ExpYield;
    public int SpawnCount;
}

public class MonsterDataManager : MonoBehaviour
{
    public static MonsterDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("MonsterDataManager");
                instance = go.AddComponent<MonsterDataManager>();
            }
            return instance;
        }
    }
    private static MonsterDataManager instance;

    private Dictionary<string, MonsterStat> monsterStats = new Dictionary<string, MonsterStat>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadCSV();
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void LoadCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("CSV/Enemy/Stats/StatEnemy");
        if (csvFile == null)
        {
            Debug.LogError("Could not find StatEnemy.csv in Resources/CSV/Enemy/Stats/");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        
        // Skip header lines (First line is commas, second is column names)
        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 6) continue;

            try
            {
                MonsterStat stat = new MonsterStat();
                stat.Key = values[0];
                stat.HP = float.Parse(values[1]);
                stat.ATK = float.Parse(values[2]);
                stat.Speed = float.Parse(values[3]);
                stat.ExpYield = float.Parse(values[4]);
                // SpawnCount might be empty in some rows
                int.TryParse(values[5], out stat.SpawnCount);

                monsterStats[stat.Key] = stat;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to parse CSV line {i}: {line}. Error: {e.Message}");
            }
        }

        Debug.Log($"Loaded {monsterStats.Count} monster stats from CSV.");
    }

    public MonsterStat GetStat(string key)
    {
        if (monsterStats.TryGetValue(key, out MonsterStat stat))
        {
            return stat;
        }
        Debug.LogWarning($"Monster stat not found for key: {key}");
        return default;
    }

    public List<MonsterStat> GetAllStats()
    {
        return new List<MonsterStat>(monsterStats.Values);
    }
}
