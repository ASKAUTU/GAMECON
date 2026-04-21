using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PostProcessController : MonoBehaviour
{
    public static PostProcessController Instance { get; private set; }

    private Volume volume;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    [Header("Vignette Settings")]
    [SerializeField] private float defaultVignetteIntensity = 0.4f;
    [SerializeField] private Color defaultVignetteColor = Color.black;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        volume = GetComponent<Volume>();
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out chromaticAberration);
            volume.profile.TryGet(out lensDistortion);
        }

        ResetEffects();
    }

    private void Start()
    {
        PlayStartEffect(2.0f);
    }

    public void ResetEffects()
    {
        if (vignette != null)
        {
            vignette.intensity.Override(defaultVignetteIntensity);
            vignette.color.Override(defaultVignetteColor);
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.Override(0f);
        }
        if (lensDistortion != null)
        {
            lensDistortion.intensity.Override(0f);
            lensDistortion.scale.Override(1f);
        }
    }

    public void FlashVignette(float targetIntensity, float duration, Color? flashColor = null)
    {
        if (vignette == null) return;
        StartCoroutine(VignetteFlashRoutine(targetIntensity, duration, flashColor ?? Color.red));
    }

    private IEnumerator VignetteFlashRoutine(float targetIntensity, float duration, Color flashColor)
    {
        float elapsed = 0f;
        vignette.color.Override(flashColor);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentIntensity = Mathf.Lerp(targetIntensity, defaultVignetteIntensity, t);
            vignette.intensity.Override(currentIntensity);
            yield return null;
        }

        vignette.intensity.Override(defaultVignetteIntensity);
        vignette.color.Override(defaultVignetteColor);
    }

    public void HitStop(float duration = 0.05f)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public void PlayStartEffect(float duration)
    {
        if (chromaticAberration == null || lensDistortion == null) return;
        StopAllCoroutines();
        StartCoroutine(StartEffectRoutine(duration));
    }

    private IEnumerator StartEffectRoutine(float duration)
    {
        float elapsed = 0f;

        // Initial states
        chromaticAberration.intensity.Override(1f);
        lensDistortion.intensity.Override(-1f);
        lensDistortion.scale.Override(0.01f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Use smooth step for more "graceful" feel
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            chromaticAberration.intensity.Override(Mathf.Lerp(1f, 0f, smoothT));
            lensDistortion.intensity.Override(Mathf.Lerp(-1f, 0f, smoothT));
            lensDistortion.scale.Override(Mathf.Lerp(0.01f, 1f, smoothT));

            yield return null;
        }

        ResetEffects();
    }
}
