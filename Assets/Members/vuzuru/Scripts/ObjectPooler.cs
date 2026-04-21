using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    public void AddPool(string tag, GameObject prefab, int size)
    {
        if (poolDictionary == null) poolDictionary = new Dictionary<string, Queue<GameObject>>();
        if (poolDictionary.ContainsKey(tag)) return;

        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        poolDictionary.Add(tag, objectPool);
    }

    void Start()
    {
        if (poolDictionary == null) poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = null;
        int attempts = poolDictionary[tag].Count;

        // Find a valid object in the queue
        for (int i = 0; i < attempts; i++)
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
            if (objectToSpawn != null) break;
        }

        // If no valid object found or all were destroyed, we might need a fallback or create a new one
        if (objectToSpawn == null)
        {
            // Here we could try to instantiate a new one from a stored prefab if we had the reference
            Debug.LogWarning("Pool " + tag + " is empty or contains only destroyed objects.");
            return null;
        }
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Call IPoolable methods if implemented
        IPoolable poolable = objectToSpawn.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnSpawn();
        }

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        if (obj == null) return;
        
        obj.SetActive(false);
        obj.transform.SetParent(null); // Unparent to prevent destruction if parent is destroyed

        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnReturnToPool();
        }
    }
}
