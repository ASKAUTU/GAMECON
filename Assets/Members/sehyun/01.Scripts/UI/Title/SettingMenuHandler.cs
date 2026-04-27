using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingMenuHandler : MonoBehaviour
{
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private CanvasGroup settingMenuCanvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private string bgmVolumeParameter = "BgmVolume";
    [SerializeField] private string sfxVolumeParameter = "SfxVolume";

    private Resolution[] resolutions;

    private const string MasterVolumeKey = "MasterVolume";
    private const string BgmVolumeKey = "BgmVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string ResolutionIndexKey = "ResolutionIndex";

    private void Start()
    {
        resolutions = Screen.resolutions;

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        int savedResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, currentResolutionIndex);
        resolutionDropdown.value = savedResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        float savedMaster = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float savedBgm = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
        float savedSfx = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        masterVolumeSlider.value = savedMaster;
        bgmVolumeSlider.value = savedBgm;
        sfxVolumeSlider.value = savedSfx;

        OnMasterVolumeChanged(savedMaster);
        OnBgmVolumeChanged(savedBgm);
        OnSfxVolumeChanged(savedSfx);
        OnResolutionChanged(savedResolutionIndex);
    }

    public void OnMasterVolumeChanged(float value)
    {
        SetMixerVolume(masterVolumeParameter, value);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    public void OnBgmVolumeChanged(float value)
    {
        SetMixerVolume(bgmVolumeParameter, value);
        PlayerPrefs.SetFloat(BgmVolumeKey, value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        SetMixerVolume(sfxVolumeParameter, value);
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
    }

    public void OnResolutionChanged(int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        PlayerPrefs.SetInt(ResolutionIndexKey, index);
    }

    public void OnClickResetData()
    {
        PlayerPrefs.DeleteAll();
    }

    public void OnClickBack()
    {
        Fade(settingMenuCanvasGroup, false);
        Fade(mainMenuCanvasGroup, true);
    }

    public void OnClickQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void Fade(CanvasGroup canvasGroup, bool isOn)
    {
        canvasGroup.DOFade(isOn ? 1f : 0f, fadeDuration);
        canvasGroup.interactable = isOn;
        canvasGroup.blocksRaycasts = isOn;
    }

    private void SetMixerVolume(string parameter, float value)
    {
        float db = value <= 0f ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameter, db);
    }
}
