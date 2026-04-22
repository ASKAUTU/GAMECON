using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] private GameObject flashObject;
    [SerializeField] private float duration = 0.05f;

    private void Awake()
    {
        if (flashObject != null) flashObject.SetActive(false);
    }

    public void Play()
    {
        if (flashObject == null)
        {
            Debug.LogWarning($"[MuzzleFlash] flashObject is NOT assigned on {gameObject.name}!");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Debug.Log("[MuzzleFlash] Playing flash effect");
        flashObject.SetActive(true);
        
        // Random rotation for variety
        flashObject.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        
        yield return new WaitForSeconds(duration);
        flashObject.SetActive(false);
    }
}
