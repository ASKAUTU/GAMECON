using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        
        // 1. 카메라 이동 부드럽게 계산
        Vector3 finalPos = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        
        // 2. 카메라 흔들림 오프셋 추가
        if (CameraShake.Instance != null)
        {
            if (CameraShake.Instance.ShakeOffset != Vector3.zero)
            {
                // Debug.Log($"[CameraFollow] Applying ShakeOffset: {CameraShake.Instance.ShakeOffset}");
            }
            finalPos += CameraShake.Instance.ShakeOffset;
        }
        else
        {
            Debug.LogWarning("[CameraFollow] CameraShake.Instance is NULL!");
        }

        transform.position = finalPos;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
