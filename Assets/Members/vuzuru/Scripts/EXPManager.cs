using UnityEngine;

public class EXPManager : MonoBehaviour
{
    public static EXPManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EXPManager");
                instance = go.AddComponent<EXPManager>();
            }
            return instance;
        }
    }
    private static EXPManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterPool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void RegisterPool()
    {
        if (ObjectPooler.Instance != null)
        {
            GameObject expPrefab = Resources.Load<GameObject>("Prefabs/EXP");
            if (expPrefab != null)
            {
                // EXPItem 스크립트가 프리팹에 없다면 추가
                if (expPrefab.GetComponent<EXPItem>() == null)
                {
                    // Note: In editor this might be tricky, but for runtime it's fine
                    // Ideally the prefab should have it.
                }
                ObjectPooler.Instance.AddPool("EXP", expPrefab, 50);
            }
        }
    }

    public void SpawnEXP(Vector3 position, float totalExp)
    {
        if (totalExp <= 0) return;

        // 10당 EXP1개, 최대 10개 고정
        // 10보다 작아도 무조건 1개
        int count = Mathf.CeilToInt(totalExp / 10f);
        count = Mathf.Min(count, 10);
        
        float expPerObject = totalExp / count;

        for (int i = 0; i < count; i++)
        {
            GameObject expGo = ObjectPooler.Instance.SpawnFromPool("EXP", position, Quaternion.identity);
            if (expGo != null)
            {
                EXPItem expItem = expGo.GetComponent<EXPItem>();
                if (expItem == null) expItem = expGo.AddComponent<EXPItem>();
                expItem.SetValue(expPerObject);
            }
        }
    }
}
