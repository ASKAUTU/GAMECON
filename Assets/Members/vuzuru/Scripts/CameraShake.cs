using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private static CameraShake _instance;
    public static CameraShake Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<CameraShake>();
            }
            return _instance;
        }
    }

    public Vector3 ShakeOffset { get; private set; }

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    public void Shake(float duration, float magnitude)
    {
        Debug.Log($"[CameraShake] Shake called! Duration: {duration}, Magnitude: {magnitude}");
        StopAllCoroutines();
        StartCoroutine(DoShake(duration, magnitude));
    }

    private System.Collections.IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            ShakeOffset = new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return null;
        }

        ShakeOffset = Vector3.zero;
        Debug.Log("[CameraShake] Shake finished.");
    }
}
