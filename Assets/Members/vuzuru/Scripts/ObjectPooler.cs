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

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

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

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // If the object is active, it means we ran out of objects and are reusing the oldest one.
        // Alternatively, we could instantiate a new one here to expand the pool.
        
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
        obj.SetActive(false);
        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnReturnToPool();
        }
    }
}
